using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MeterDataLib.Parsers
{


    public class CsvRedEnergyFormat : IParser
    {
        public string Name => "Csv Red Energy";

        private static readonly int[] AllowedIntervals = [1, 5, 15, 30, 60];

        public bool CanParse(List<CsvLine> lines)
        {
            if (lines.Count < 2) return false;

            const string hdrCheck = "NMI        ,INTERVAL_DATETIME   ,INTERVAL ,INTERVAL_LENGTH ,SERIAL  ,QUALITY_METHOD ,SUFFIX_E ,VALUE_E ,UOM_E ,SUFFIX_K ,VALUE_K ,UOM_K ,SUFFIX_Q ,VALUE_Q ,UOM_Q ,SUFFIX_B ,VALUE_B ,UOM_B";
            string[] hdrArray = hdrCheck.Split(',', StringSplitOptions.TrimEntries);

            // CHECK HEADER ROW
            if (lines[0].ColCount != hdrArray.Length) return false;
            for (int i = 0; i < hdrArray.Length; i++)
            {
                if (lines[i].GetStringUpper(0) != hdrArray[i]) return false;
            }
            return true;
        }
        async Task IParser.Parse(SimpleCsvReader reader, ParserResult result, Func<ParserResult, Task>? callBack, CancellationToken? cancellationToken)
        {
            string filename = reader.Filename;
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            var records = new List<DataLine>();
            try
            {


                {
                    CsvLine line;
                    // skip the first line 
                    await reader.ReadAsync();

                    while ((line = await reader.ReadAsync()).Eof == false)
                    {
                        cancellationToken?.ThrowIfCancellationRequested();
                        try
                        {
                            if (timer.ElapsedMilliseconds > 100)
                            {
                                result.Progress = $"reading line {line.LineNumber}";
                                timer.Restart();
                                if (callBack != null)
                                {
                                    await callBack(result);
                                }
                            }

                            //NMI        ,MM   ,INTERVAL ,INTERVAL_LENGTH ,SERIAL  ,QUALITY_METHOD ,SUFFIX_E ,VALUE_E ,UOM_E ,SUFFIX_K ,VALUE_K ,UOM_K ,SUFFIX_Q ,VALUE_Q ,UOM_Q ,SUFFIX_B ,VALUE_B ,UOM_B
                            string nmi = line.GetStringUpperMandatory(0);
                            DateTime localTime = line.GetDateMandatory(1, ["dd/MM/yyyy HH:mm:ss"]);
                            int interval = line.GetIntMandatory(2, 1, 24 * 60);  // period can be 1 to 1440 minutes
                            int intervalLength = line.GetIntMandatory(3, 1, 60);  // interval length can be 1 to 60 minutes
                            string serial = line.GetStringUpperMandatory(4);
                            string qualityMethod = line.GetStringUpperMandatory(5);
                            string suffixE = line.GetString(6);
                            decimal e = line.GetDecimalCol(7) ?? 0;
                            string uomE = line.GetString(8);
                            string suffixK = line.GetString(9);
                            decimal k = line.GetDecimalCol(10) ?? 0;
                            string uomK = line.GetString(11);
                            string suffixQ = line.GetString(12);
                            decimal q = line.GetDecimalCol(13) ?? 0;
                            string uomQ = line.GetString(14);
                            string suffixB = line.GetString(15);
                            decimal b = line.GetDecimalCol(16) ?? 0;
                            string uomB = line.GetString(17);

                            if (!AllowedIntervals.Contains(interval))
                            {
                                result.LogMessages.Add(new FileLogMessage($"Invalid interval [{interval}] between reads found ", Microsoft.Extensions.Logging.LogLevel.Error, filename, 0, 2));
                                continue;
                            }

                            if (!string.IsNullOrWhiteSpace(suffixE))
                            {

                                var record = new CsvRedEnergyFormat.DataLine
                               (
                               Nmi: nmi,
                               LocalTime: localTime,
                               Interval: interval,
                               IntervalLength: intervalLength,
                               Serial: serial,
                               QualityMethod: qualityMethod,
                               Suffix: suffixE,
                               Value: e,
                               Uom: uomE,
                               LineNumber: line.LineNumber
                               );
                                records.Add(record);
                            }
                            if (!string.IsNullOrWhiteSpace(suffixK))
                            {
                                var record = new CsvRedEnergyFormat.DataLine
                               (
                               Nmi: nmi,
                               LocalTime: localTime,
                               Interval: interval,
                               IntervalLength: intervalLength,
                               Serial: serial,
                               QualityMethod: qualityMethod,
                               Suffix: suffixK,
                               Value: k,
                               Uom: uomK,
                               LineNumber: line.LineNumber
                               );
                                records.Add(record);
                            }
                            if (!string.IsNullOrWhiteSpace(suffixQ))
                            {
                                var record = new CsvRedEnergyFormat.DataLine
                               (
                               Nmi: nmi,
                               LocalTime: localTime,
                               Interval: interval,
                               IntervalLength: intervalLength,
                               Serial: serial,
                               QualityMethod: qualityMethod,
                               Suffix: suffixQ,
                               Value: q,
                               Uom: uomQ,
                               LineNumber: line.LineNumber
                               );
                                records.Add(record);
                            }
                            if (!string.IsNullOrWhiteSpace(suffixB))
                            {

                                var record = new CsvRedEnergyFormat.DataLine
                               (
                               Nmi: nmi,
                               LocalTime: localTime,
                               Interval: interval,
                               IntervalLength: intervalLength,
                               Serial: serial,
                               QualityMethod: qualityMethod,
                               Suffix: suffixB,
                               Value: b,
                               Uom: uomB,
                               LineNumber: line.LineNumber
                               );
                                records.Add(record);
                            }


                        }
                        catch (Exception ex)
                        {
                            result.LogMessages.Add(new FileLogMessage(ex.Message, Microsoft.Extensions.Logging.LogLevel.Critical, filename, reader.LineNumber, 0));
                        }
                    }
                }

               
                var list = records.GroupBy(x => new { x.Nmi, x.ReadDate })
                                  .Where(x => x.Count() > 1)
                                  .OrderBy(x => x.Key.Nmi)
                                  .ThenBy(x => x.Key.ReadDate)
                                  .ToList();

                int counterTotal = list.Count;
                int counter = 0;

                foreach (var siteDayGroup in list)
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    counter++;
                    if (timer.ElapsedMilliseconds > 100)
                    {
                        int percentage = counter * 100 / counterTotal;
                        result.Progress = $"parsing {percentage}%";

                        timer.Restart();
                        if (callBack != null)
                        {
                            await callBack(result);
                        }
                    }
                    var siteDay = new SiteDay()
                    {
                        SiteCode = siteDayGroup.Key.Nmi,
                        Date = siteDayGroup.Key.ReadDate,
                        Channels = [],
                    };
                    result.SitesDays.Add(siteDay);

                    // Transform the data into a dictionary of channels 















                    int interval = siteDayGroup.First().Interval;
                    if (!allowedIntervals.Contains(interval))
                    {
                        result.LogMessages.Add(new FileLogMessage($"Invalid interval [{interval}] between reads found ", Microsoft.Extensions.Logging.LogLevel.Error, filename, 0, 2));
                        continue;
                    }

                    int expectedPeriods = 60 * 24 / interval;

                    ChannelDay eChannel = new()
                    {
                        Channel = $"E1",
                        IntervalMinutes = interval,
                        ChannelNumber = "1",
                        ChannelType = ChanelType.ActiveEnergyConsumption,
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

                    ChannelDay bChannel = new()
                    {
                        Channel = $"B1",
                        IntervalMinutes = interval,
                        ChannelNumber = "1",
                        ChannelType = ChanelType.ActiveEnergyGeneration,
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

                    ChannelDay qChannel = new()
                    {
                        Channel = $"Q1",
                        IntervalMinutes = interval,
                        ChannelNumber = "1",
                        ChannelType = ChanelType.ReactiveEnergyConsumption,
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

                    ChannelDay kChannel = new()
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

            return;
        }

        //NMI        ,INTERVAL_DATETIME   ,INTERVAL ,INTERVAL_LENGTH ,SERIAL  ,QUALITY_METHOD ,SUFFIX_E ,VALUE_E ,UOM_E ,SUFFIX_K ,VALUE_K ,UOM_K ,SUFFIX_Q ,VALUE_Q ,UOM_Q ,SUFFIX_B ,VALUE_B ,UOM_B
        record DataLine(string Nmi, DateTime LocalTime, int Interval, int IntervalLength,
                        string Serial,
                        string QualityMethod,
                        string Suffix, decimal Value, string Uom
                        int LineNumber)
        {
            public DateTime StartTime => LocalTime.AddMinutes(-1 * IntervalLength);
            public DateTime ReadDate => StartTime.Date;
        }



    }
}
