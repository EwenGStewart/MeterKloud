﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MeterDataLib.Parsers
{


    public class CsvSingleLineSimpleEBKvaPF : Parser
    {
        public override string Name => "SingleLineWith_E_B_KVA_PF";

        public override bool CanParse(Stream stream, string filename, string? mimeType)
        {

            if (!CsvParserLib.ValidateMime(mimeType))
            {
                return false;
            }
            var lines = CsvParserLib.GetFirstXLines(stream, filename, 2);
            if (lines.Count < 2) return false;

            // CHECK HEADER ROW
            if (
                    lines[0].ColCount == 9
                && lines[1].ColCount == lines[0].ColCount
                && lines[0].GetStringUpper(0) == "NMI"
                && lines[0].GetStringUpper(1) == "METERSERIAL"
                && lines[0].GetStringUpper(2) == "PERIOD"
                && lines[0].GetStringUpper(3) == "LOCAL TIME"
                && lines[0].GetStringUpper(4) == "E KWH AT METER"
                && lines[0].GetStringUpper(5) == "B KWH AT METER"
                && lines[0].GetStringUpper(6) == "KW"
                && lines[0].GetStringUpper(7) == "KVA"
                && lines[0].GetStringUpper(8) == "PF"
                && lines[1].GetDate(3, "dd/MM/yyyy HH:mm") != null
                )
                return true;
            return false;
        }
        public override ParserResult Parse(Stream stream, string filename)
        {
            var result = new ParserResult();
            result.FileName = filename;
            result.ParserName = Name;

            var records = new List<DataLine>();
            try
            {

                using (var reader = new SimpleCsvReader(stream, filename))
                {
                    CsvLine line;
                    // skip the first line 
                    reader.Read();
                    try
                    {
                        while ((line = reader.Read()).Eof == false)
                        {
                            var record = new DataLine
                                (
                                NMI: line.GetStringUpper(0),
                                MeterSerial: line.GetStringUpper(1),
                                Period: line.GetIntCol(2),
                                LocalTime: line.GetDate(3, "dd/MM/yyyy HH:mm"),
                                EKwhAtMeter: line.GetDecimalCol(4),
                                BKwhAtMeter: line.GetDecimalCol(5),
                                KW: line.GetDecimalCol(6),
                                KVA: line.GetDecimalCol(7),
                                PF: line.GetDecimalCol(8),
                                lineNumber: line.LineNumber
                                );
                            records.Add(record);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.LogMessages.Add(new FileLogMessage(ex.Message, Microsoft.Extensions.Logging.LogLevel.Critical, filename, reader.LineNumber, 0));
                    }


                    // validate 
                    var badPeriods = records.Where(x => x.Period == null || x.Period < 1 || x.Period > 288).ToList();
                    foreach (var bad in badPeriods)
                    {
                        result.LogMessages.Add(new FileLogMessage($"Invalid period {bad.Period}", Microsoft.Extensions.Logging.LogLevel.Error, filename, bad.lineNumber, 2));
                    }
                    records.RemoveAll(x => x.Period == null || x.Period < 1 || x.Period > 288);
                    // bad dates 
                    var badDates = records.Where(x => x.LocalTime == null).ToList();
                    foreach (var bad in badDates)
                    {
                        result.LogMessages.Add(new FileLogMessage($"Invalid date  ", Microsoft.Extensions.Logging.LogLevel.Error, filename, bad.lineNumber, 3));
                    }
                    records.RemoveAll(x => x.LocalTime == null);
                    // bad values
                    var badValues = records.Where(x => x.EKwhAtMeter == null).ToList();
                    foreach (var bad in badValues)
                    {
                        result.LogMessages.Add(new FileLogMessage($"Invalid E value  ", Microsoft.Extensions.Logging.LogLevel.Error, filename, bad.lineNumber, 4));
                    }
                    records.RemoveAll(x => x.EKwhAtMeter == null);

                    foreach (var siteDayGroup in records
                                                .GroupBy(x => new { x.NMI, ReadDate = x.LocalTime!.Value.Date })
                                                .OrderBy(x => x.Key.NMI)
                                                .ThenBy(x => x.Key.ReadDate)
                                                )
                    {
                        var siteDay = new SiteDay()
                        {
                            Site = siteDayGroup.Key.NMI,
                            Date = siteDayGroup.Key.ReadDate,
                            Channels = new Dictionary<string, ChannelDay>(),
                        };
                        result.SitesDays.Add(siteDay);


                        int channelNum = 0;
                        foreach (var meter in siteDayGroup.GroupBy(x => x.MeterSerial))
                        {
                            channelNum++;
                            var maxPeriod = meter.Max(x => x.Period);
                            var interval =
                                    maxPeriod > 288 ? 0 :
                                    maxPeriod > 96 ? 5 :
                                    maxPeriod > 48 ? 15 :
                                    maxPeriod > 24 ? 30 :
                                    0;
                            if (interval == 0)
                            {
                                result.LogMessages.Add(new FileLogMessage($"Invalid index period found {maxPeriod}", Microsoft.Extensions.Logging.LogLevel.Error, filename, meter.First().lineNumber, 2));
                                continue;
                            }
                            string channelStrNum = channelNum < 10 ? channelNum.ToString() : ((char)('A' + channelNum - 10)).ToString();

                            int expectedPeriods = 60 * 24 / interval;

                            ChannelDay eChannel = new ChannelDay()
                            {
                                Channel = $"E{channelStrNum}",
                                IntervalMinutes = interval,
                                ChannelNumber = channelStrNum,
                                ChannelType = ChanelType.ActivePowerConsumption,
                                Ignore = false,
                                Readings = new decimal[expectedPeriods],
                                TimeStampUtc = DateTime.UtcNow,
                                MeterId = meter.Key,
                                RegisterId = $"E{channelStrNum}",
                                SourceFile = filename,
                                UnitOfMeasure = UnitOfMeasure.kWh,
                                OverallQuality = Quality.Actual,
                                Total = 0,
                                IsAvg = false,
                                IsNet = false,
                                IsCheck = false
                            };
                            siteDay.Channels.Add(eChannel.Channel, eChannel);

                            ChannelDay bChannel =
                            new ChannelDay()
                            {
                                Channel = $"B{channelStrNum}",
                                IntervalMinutes = interval,
                                ChannelNumber = channelStrNum,
                                ChannelType = ChanelType.ActivePowerGeneration,
                                Ignore = false,
                                Readings = new decimal[expectedPeriods],
                                TimeStampUtc = DateTime.UtcNow,
                                MeterId = meter.Key,
                                RegisterId = $"B{channelStrNum}",
                                SourceFile = filename,
                                UnitOfMeasure = UnitOfMeasure.kWh,
                                OverallQuality = Quality.Actual,
                                Total = 0,
                                IsAvg = false,
                                IsNet = false,
                                IsCheck = false
                            };
                            siteDay.Channels.Add(bChannel.Channel, bChannel);

                            ChannelDay kw =
                           new ChannelDay()
                           {
                               Channel = $"KW{channelStrNum}",
                               IntervalMinutes = interval,
                               ChannelNumber = channelStrNum,
                               ChannelType = ChanelType.RealPower,
                               Ignore = false,
                               Readings = new decimal[expectedPeriods],
                               TimeStampUtc = DateTime.UtcNow,
                               MeterId = meter.Key,
                               RegisterId = $"KW{channelStrNum}",
                               SourceFile = filename,
                               UnitOfMeasure = UnitOfMeasure.kW,
                               OverallQuality = Quality.Actual,
                               Total = 0,
                               IsAvg = false,
                               IsNet = false,
                               IsCheck = false
                           };
                            siteDay.Channels.Add(kw.Channel, kw);

                            ChannelDay kva =
                            new ChannelDay()
                            {
                                Channel = $"KVA{channelStrNum}",
                                IntervalMinutes = interval,
                                ChannelNumber = channelStrNum,
                                ChannelType = ChanelType.ApparentPower,
                                Ignore = false,
                                Readings = new decimal[expectedPeriods],
                                TimeStampUtc = DateTime.UtcNow,
                                MeterId = meter.Key,
                                RegisterId = $"KVA{channelStrNum}",
                                SourceFile = filename,
                                UnitOfMeasure = UnitOfMeasure.kVA,
                                OverallQuality = Quality.Actual,
                                Total = 0,
                                IsAvg = false,
                                IsNet = false,
                                IsCheck = false
                            };
                            siteDay.Channels.Add(kva.Channel, kva);

                            ChannelDay pf =
                            new ChannelDay()
                            {
                                Channel = $"PF{channelStrNum}",
                                IntervalMinutes = interval,
                                ChannelNumber = channelStrNum,
                                ChannelType = ChanelType.PowerFactor,
                                Ignore = false,
                                Readings = new decimal[expectedPeriods],
                                TimeStampUtc = DateTime.UtcNow,
                                MeterId = meter.Key,
                                RegisterId = $"PF{channelStrNum}",
                                SourceFile = filename,
                                UnitOfMeasure = UnitOfMeasure.pf,
                                OverallQuality = Quality.Actual,
                                Total = 0,
                                IsAvg = false,
                                IsNet = false,
                                IsCheck = false
                            };
                            siteDay.Channels.Add(pf.Channel, pf);

                            foreach (var record in meter)
                            {
                                int index = record.Period!.Value - 1;
                                if (index < 0 || index >= eChannel.Readings.Length)
                                {
                                    result.LogMessages.Add(new FileLogMessage($"Invalid period found  {record.Period!.Value} ", Microsoft.Extensions.Logging.LogLevel.Error, filename, record.lineNumber));
                                    continue;
                                }
                                decimal e = record.EKwhAtMeter ?? 0;
                                decimal b = record.BKwhAtMeter ?? 0;
                                decimal kwValue = record.KW ?? 0;
                                decimal kvaValue = record.KVA ?? 0;
                                decimal pfValue = record.PF ?? 1;
                                eChannel.Readings[index] = e;
                                bChannel.Readings[index] = b;
                                kw.Readings[index] = kwValue;
                                kva.Readings[index] = kvaValue;
                                pf.Readings[index] = pfValue;
                            }
                            eChannel.Total = eChannel.Readings.Sum();
                            bChannel.Total = bChannel.Readings.Sum();
                            kw.Total = kw.Readings.Max();
                            kva.Total = kva.Readings.Max();
                            pf.Total = pf.Readings.Max();

                        }

                    }
                }

            }
            catch (Exception ex)
            {
                result.LogMessages.Add(new FileLogMessage(ex.Message, Microsoft.Extensions.Logging.LogLevel.Critical, filename, 0, 0));
            }

            return result;
        }


        record DataLine(string NMI, string MeterSerial, int? Period, DateTime? LocalTime, decimal? EKwhAtMeter, decimal? BKwhAtMeter, decimal? KW, decimal? KVA, decimal? PF, int lineNumber)
        { }



    }
}