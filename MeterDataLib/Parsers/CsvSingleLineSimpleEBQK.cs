using Microsoft.Extensions.Logging;
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


    public class CsvSingleLineSimpleEBQK : IParser
    {
        public string Name => "SingleLineWith_E_B_K_Q";

        public static IParser? GetParser(Stream stream, string filename, string? mimeType)
        {

            if (!CsvParserLib.ValidateMime(mimeType))
            {
                return null;
            }
            var lines = CsvParserLib.GetFirstXLines(stream, filename, 2);
            if (lines.Count < 2) return null;

            // CHECK HEADER ROW
            if (
                    lines[0].ColCount > 4
                && lines[1].ColCount > 4
                && lines[0].GetStringUpper(0) == "READ DATETIME"
                && lines[0].GetStringUpper(1) == "EXPORT KWH"
                && lines[0].GetStringUpper(2) == "IMPORT KWH"
                && lines[0].GetStringUpper(4) == "EXPORT KVARH"
                && lines[0].GetStringUpper(5) == "IMPORT KVARH"
                && lines[1].GetDate(0, new string[] { "yyyy-MM-dd HH:mm:ss", "d/MM/yyyy H:mm" }) != null
                )
                return new CsvSingleLineSimpleEBQK();
            return null;
        }
        public ParserResult Parse(Stream stream, string filename)
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

                    while ((line = reader.Read()).Eof == false)
                    {
                        try
                        {
                            var record = new DataLine
                                (
                                LocalTime: line.GetDateMandatory(0,  new string[] { "yyyy-MM-dd HH:mm:ss", "d/MM/yyyy H:mm" }),
                                E: line.GetDecimalCol(1) ?? 0,
                                B: line.GetDecimalCol(2) ?? 0,
                                Q: line.GetDecimalCol(4) ?? 0,
                                K: line.GetDecimalCol(5) ?? 0,
                                LineNumber: reader.LineNumber
                                );
                            records.Add(record);
                        }
                        catch (Exception ex)
                        {
                            result.LogMessages.Add(new FileLogMessage(ex.Message, Microsoft.Extensions.Logging.LogLevel.Critical, filename, reader.LineNumber, 0));
                        }
                    }
                }

                int[] allowedIntervals = new int[] { 1, 5, 15, 30, 60 };
                foreach (var siteDayGroup in records
                                            .GroupBy(x => new { ReadDate = x.LocalTime.Date })
                                            .Where(x => x.Count() > 1)
                                            .OrderBy(x => x.Key.ReadDate)
                                            )
                {
                    var siteDay = new SiteDay()
                    {
                        Site = "UNKNOWN",
                        Date = siteDayGroup.Key.ReadDate,
                        Channels = new Dictionary<string, ChannelDay>(),
                    };
                    result.SitesDays.Add(siteDay);

                    var read1 = siteDayGroup.OrderBy(x => x.LocalTime).First();
                    var read2 = siteDayGroup.OrderBy(x => x.LocalTime).Skip(1).First();
                    var interval = (int)(read2.LocalTime.Subtract(read1.LocalTime).TotalMinutes);

                    if (!allowedIntervals.Contains(interval))
                    {
                        result.LogMessages.Add(new FileLogMessage($"Invalid interval [{interval}] between reads found ", Microsoft.Extensions.Logging.LogLevel.Error, filename, 0, 2));
                        continue;
                    }

                    int expectedPeriods = 60 * 24 / interval;

                    ChannelDay eChannel = new ChannelDay()
                    {
                        Channel = $"E1",
                        IntervalMinutes = interval,
                        ChannelNumber = "1",
                        ChannelType = ChanelType.ActivePowerConsumption,
                        Ignore = false,
                        Readings = new decimal[expectedPeriods],
                        TimeStampUtc = DateTime.UtcNow,
                        MeterId = "all",
                        RegisterId = $"E1",
                        SourceFile = filename,
                        UnitOfMeasure = UnitOfMeasure.kWh,
                        OverallQuality = Quality.Actual,
                        Total = 0,
                        IsAvg = false,
                        IsNet = false,
                        IsCheck = false
                    };
                    siteDay.Channels.Add(eChannel.Channel, eChannel);

                    ChannelDay bChannel = new ChannelDay()
                    {
                        Channel = $"B1",
                        IntervalMinutes = interval,
                        ChannelNumber = "1",
                        ChannelType = ChanelType.ActivePowerGeneration,
                        Ignore = false,
                        Readings = new decimal[expectedPeriods],
                        TimeStampUtc = DateTime.UtcNow,
                        MeterId = "all",
                        RegisterId = $"B1",
                        SourceFile = filename,
                        UnitOfMeasure = UnitOfMeasure.kWh,
                        OverallQuality = Quality.Actual,
                        Total = 0,
                        IsAvg = false,
                        IsNet = false,
                        IsCheck = false
                    };
                    siteDay.Channels.Add(bChannel.Channel, bChannel);

                    ChannelDay qChannel = new ChannelDay()
                    {
                        Channel = $"Q1",
                        IntervalMinutes = interval,
                        ChannelNumber = "1",
                        ChannelType = ChanelType.ReactivePowerConsumption,
                        Ignore = false,
                        Readings = new decimal[expectedPeriods],
                        TimeStampUtc = DateTime.UtcNow,
                        MeterId = "all",
                        RegisterId = $"Q1",
                        SourceFile = filename,
                        UnitOfMeasure = UnitOfMeasure.kVArh,
                        OverallQuality = Quality.Actual,
                        Total = 0,
                        IsAvg = false,
                        IsNet = false,
                        IsCheck = false
                    };
                    siteDay.Channels.Add(qChannel.Channel, qChannel);

                    ChannelDay kChannel = new ChannelDay()
                    {
                        Channel = $"K1",
                        IntervalMinutes = interval,
                        ChannelNumber = "1",
                        ChannelType = ChanelType.ApparentPower,
                        Ignore = false,
                        Readings = new decimal[expectedPeriods],
                        TimeStampUtc = DateTime.UtcNow,
                        MeterId = "all",
                        RegisterId = $"K1",
                        SourceFile = filename,
                        UnitOfMeasure = UnitOfMeasure.kVArh,
                        OverallQuality = Quality.Actual,
                        Total = 0,
                        IsAvg = false,
                        IsNet = false,
                        IsCheck = false
                    };
                    siteDay.Channels.Add(kChannel.Channel, kChannel);



                    foreach (var record in siteDayGroup)
                    {
                        int index = (record.LocalTime.Hour * 60 + record.LocalTime.Minute) / interval;
                        if (index < 0 || index >= eChannel.Readings.Length)
                        {
                            result.LogMessages.Add(new FileLogMessage($"Invalid index period found  {index} ", LogLevel.Error, filename, record.LineNumber));
                            continue;
                        }

                        eChannel.Readings[index] = record.E;
                        bChannel.Readings[index] = record.B;
                        qChannel.Readings[index] = record.Q;
                        kChannel.Readings[index] = record.K;
                    }
                    eChannel.Total = eChannel.Readings.Sum();
                    bChannel.Total = bChannel.Readings.Sum();
                    kChannel.Total = kChannel.Readings.Sum();
                    qChannel.Total = qChannel.Readings.Sum();
                }

            }
            catch (Exception ex)
            {
                result.LogMessages.Add(new FileLogMessage(ex.Message, Microsoft.Extensions.Logging.LogLevel.Critical, filename, 0, 0));
            }

            return result;
        }


        record DataLine(DateTime LocalTime, decimal E, decimal B, decimal Q, decimal K, int LineNumber){ }



    }
}
