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


    public class CsvPowerPal : Parser
    {
        public override string Name => "CsvPowerPal";

        public override bool CanParse (Stream stream, string filename, string? mimeType)
        {

            if (!CsvParserLib.ValidateMime(mimeType))
            {
                return false;
            }
            var lines = CsvParserLib.GetFirstXLines(stream, filename, 1);
            if (lines.Count < 1) return false ;

            // CHECK HEADER ROW
            if (
                lines[0].ColCount == 5
                && lines[0].GetStringUpper(0) == "DATETIME_UTC"
                && lines[0].GetStringUpper(1) == "DATETIME_LOCAL"
                && lines[0].GetStringUpper(2) == "WATT_HOURS"
                && lines[0].GetStringUpper(3) == "COST_DOLLARS"
                && lines[0].GetStringUpper(4) == "IS_PEAK"
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

                    while ((line = reader.Read()).Eof == false)
                    {
                        try
                        {
                            var readDateTimeUtc = line.GetDate(0, new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-M-d h:mm:ss", "yyyy-MM-dd HH:mm" });
                            var readDateTimeLocal = line.GetDate(1, new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-M-d h:mm:ss", "yyyy-MM-dd HH:mm" });
                            var readValue = line.GetDecimalCol(2);
                            var costDollars = line.GetDecimalCol(3);
                            var isPeak = line.GetStringUpper(4);
                            if (readDateTimeUtc == null)
                            {
                                result.LogMessages.Add(new FileLogMessage($"Invalid date or time {line.GetString(0)}", Microsoft.Extensions.Logging.LogLevel.Error, filename, reader.LineNumber, 6));
                                continue;
                            }

                            if (readDateTimeLocal == null)
                            {
                                result.LogMessages.Add(new FileLogMessage($"Invalid date or time {line.GetString(1)}", Microsoft.Extensions.Logging.LogLevel.Error, filename, reader.LineNumber, 7));
                                continue;
                            }

                            if (readValue == null)
                            {
                                result.LogMessages.Add(new FileLogMessage($"Invalid read {line.GetString(2)}", Microsoft.Extensions.Logging.LogLevel.Error, filename, reader.LineNumber, 8));
                                continue;
                            }


                            var record = new DataLine(readDateTimeUtc.Value, readDateTimeLocal.Value, readValue.Value, costDollars ?? 0, isPeak, line.LineNumber);
                            records.Add(record);
                        }
                        catch (Exception ex)
                        {
                            result.LogMessages.Add(new FileLogMessage(ex.Message, Microsoft.Extensions.Logging.LogLevel.Critical, filename, reader.LineNumber, 0));
                            if (result.Errors > 100)
                            {
                                throw new Exception("Too many errors");
                            }
                        }
                    }
                }

                int[] allowedIntervals = new int[] { 1, 5, 15, 30, 60 };
                foreach (var siteDayGroup in records
                                            .GroupBy(x => x.ReadDateLocal.Date)
                                            .Where(x => x.Count() > 1)
                                            .OrderBy(x => x.Key)
                                            )
                {
                    var siteDay = new SiteDay()
                    {
                        Site = "Unknown",
                        Date = siteDayGroup.Key,
                        Channels = new Dictionary<string, ChannelDay>(),
                        UCT_Offset = siteDayGroup.First().UtcOffset
                    };
                    result.SitesDays.Add(siteDay);

                    var firstRead = siteDayGroup.OrderBy(x => x.ReadDateLocal).First();
                    var secondRead = siteDayGroup.OrderBy(x => x.ReadDateLocal).Skip(1).First();

                    var readInterval = (int)secondRead.ReadDateUtc.Subtract(firstRead.ReadDateUtc).TotalMinutes;
                    if (!allowedIntervals.Contains(readInterval))
                    {
                        result.LogMessages.Add(new FileLogMessage($"Invalid interval [{readInterval}] between reads found ", Microsoft.Extensions.Logging.LogLevel.Error, filename, firstRead.LineNumber, 0));
                        continue;
                    }
                    int expectedPeriods = 60 * 24 / readInterval;

                    var channelDay = new ChannelDay()
                    {
                        Channel = "E1",
                        ChannelType = ChanelType.ActivePowerConsumption,
                        ChannelNumber = "1",
                        IsNet = true,
                        UnitOfMeasure = UnitOfMeasure.kWh,
                        OriginalUnitOfMeasure = UnitOfMeasure.W,
                        OriginalUnitOfMeasureSymbol = "W",
                        IntervalMinutes = readInterval,
                        Readings = new decimal[expectedPeriods],
                        TimeOfUseClasses = new TimeOfUseClass[expectedPeriods],
                        TimeStampUtc = DateTime.UtcNow,
                        MeterId = "Unknown",
                        RegisterId = "E1",
                        SourceFile = filename,
                        OverallQuality = Quality.Actual,

                    };
                    var channelDayCost = new ChannelDay()
                    {
                        Channel = "COST",
                        IntervalMinutes = 60,
                        ChannelType = ChanelType.Cost,
                        ChannelNumber = "1",
                        Readings = new decimal[expectedPeriods],
                        UnitOfMeasure = UnitOfMeasure.Dollars,
                        TimeOfUseClasses = new TimeOfUseClass[expectedPeriods],
                        TimeStampUtc = DateTime.UtcNow,
                        MeterId = "Unknown",
                        RegisterId = "COST",
                        SourceFile = filename,
                        OverallQuality = Quality.Actual,
                    };

                    siteDay.Channels.Add(channelDay.Channel, channelDay);
                    siteDay.Channels.Add(channelDayCost.Channel, channelDayCost);

                    foreach (var record in siteDayGroup)
                    {
                        int index = (record.ReadDateLocal.Hour * 60 + record.ReadDateLocal.Minute) / readInterval;
                        if (index < 0 || index >= channelDay.Readings.Length)
                        {
                            result.LogMessages.Add(new FileLogMessage($"Invalid index period found  {index} ", LogLevel.Error, filename, record.LineNumber));
                            continue;
                        }
                        channelDay.Readings[index] += record.ReadValue / 1000;
                        channelDay.TimeOfUseClasses[index] = record.Tou;
                        channelDayCost.Readings[index] += record.CostDollars;
                        channelDayCost.TimeOfUseClasses[index] = record.Tou;

                    }
                    channelDay.Total = channelDay.Readings.Sum();
                    channelDayCost.Total = channelDayCost.Readings.Sum();
                }
            }
            catch (Exception ex)
            {
                result.LogMessages.Add(new FileLogMessage(ex.Message, Microsoft.Extensions.Logging.LogLevel.Critical, filename, 0, 0));
            }

            return result;
        }


        record DataLine(
            DateTime ReadDateUtc,
            DateTime ReadDateLocal,
            decimal ReadValue,
            decimal CostDollars,
            string IsPeak,
            int LineNumber
            )
        {

            public decimal UtcOffset => GetUtcOffsetAsDecimal(ReadDateUtc, ReadDateLocal);
            public decimal ReadValueKwh => ReadValue / 1000;

            public bool Peak => IsPeak.Length > 0
                && (IsPeak.StartsWith("Y", StringComparison.OrdinalIgnoreCase)
                   || IsPeak.StartsWith("T", StringComparison.OrdinalIgnoreCase)
                   || IsPeak.StartsWith("1", StringComparison.OrdinalIgnoreCase)
                );


            public TimeOfUseClass Tou => Peak ? TimeOfUseClass.Peak : TimeOfUseClass.Offpeak;


            public static decimal GetUtcOffsetAsDecimal(DateTime utcDate, DateTime localDate)
            {
                // Ensure the DateTime objects are in the correct kind
                utcDate = DateTime.SpecifyKind(utcDate, DateTimeKind.Utc);
                localDate = DateTime.SpecifyKind(localDate, DateTimeKind.Local);

                // Calculate the offset
                TimeSpan offset = localDate - utcDate;

                // Convert the offset to a decimal number
                decimal offsetAsDecimal = (decimal)Math.Round(offset.TotalHours, 1);

                return offsetAsDecimal;
            }


        }






    }
}