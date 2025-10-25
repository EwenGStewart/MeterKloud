using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace MeterDataLib.Parsers
{
    public class CsvEspHome : IParser
    {
        public string Name => "CsvEspHome";

        public bool CanParse(List<CsvLine> lines)
        {
            if (lines.Count < 2) return false;

            // CHECK HEADER ROW
            if (
                lines[0].GetString(0).Contains("# Version: powershare", StringComparison.OrdinalIgnoreCase)
                && lines[1].GetStringUpper(0) == "DATETIME_UTC"
                && lines[1].GetStringUpper(1) == "DATETIME_LOCAL"
                && lines[1].GetStringUpper(2) == "TOTAL_WATT_HOURS_IN"
                && lines[1].GetStringUpper(3) == "TOTAL_WATT_HOURS_OUT"
                )
                return true;
            return false;
        }
        async Task IParser.Parse(SimpleCsvReader reader, ParserResult result, Func<ParserResult, Task>? callBack, CancellationToken? cancellationToken)
        {
            string filename = reader.Filename;
            string siteCode = "ESPHOME";
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            var records = new List<DataLine>();
            try
            {


                {
                    CsvLine line;
                    // skip the first 2 lines 
                    await reader.ReadAsync();
                    await reader.ReadAsync();


                    DateTime previousDateTime = DateTime.MinValue;
                    int previousTotalWattsIn = 0;
                    int previousTotalWattsOut = 0;
                    DataLine? previousRecord = null;

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




                            var readDateTimeUtc = line.GetDateMandatory(0, ["yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd hh:mm:ss tt", "yyyy-M-d H:mm:ss", "yyyy-M-d h:mm:ss tt", "yyyy-MM-dd HH:mm", "yyyy-MM-dd hh:mm tt", "dd/MM/yyyy HH:mm:ss", "d/M/yyyy H:mm:ss", "dd/MM/yyyy HH:mm", "d/M/yyyy hh:mm:ss tt", "dd/MM/yyyy hh:mm:ss tt", "d/M/yyyy h:mm:ss tt", "dd/MM/yyyy h:mm:ss tt"]);
                            var readDateTimeLocal = line.GetDateMandatory(1, ["yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd hh:mm:ss tt", "yyyy-M-d H:mm:ss", "yyyy-M-d h:mm:ss tt", "yyyy-MM-dd HH:mm", "yyyy-MM-dd hh:mm tt", "dd/MM/yyyy HH:mm:ss", "d/M/yyyy H:mm:ss", "dd/MM/yyyy HH:mm", "d/M/yyyy hh:mm:ss tt", "dd/MM/yyyy hh:mm:ss tt", "d/M/yyyy h:mm:ss tt", "dd/MM/yyyy h:mm:ss tt"]);

                            // remove the seconds part
                            if (readDateTimeUtc.Second != 0)
                            {
                                readDateTimeUtc = new DateTime(readDateTimeUtc.Year, readDateTimeUtc.Month, readDateTimeUtc.Day, readDateTimeUtc.Hour, readDateTimeUtc.Minute, 0);
                            }
                            if (readDateTimeLocal.Second != 0)
                            {
                                readDateTimeLocal = new DateTime(readDateTimeLocal.Year, readDateTimeLocal.Month, readDateTimeLocal.Day, readDateTimeLocal.Hour, readDateTimeLocal.Minute, 0);
                            }

                            var totalWattsIn = line.GetIntMandatory(2);
                            var totalWattsOut = line.GetIntCol(3);
                            var record = new DataLine(readDateTimeUtc, readDateTimeLocal, totalWattsIn, totalWattsOut ?? 0, line.LineNumber);
                            if (previousRecord == null)
                            {
                                previousRecord = record;
                                continue;
                            }
                            record.PreviousWattsIn = previousRecord.WattsIn;
                            record.PreviousWattsOut = previousRecord.WattsOut;
                            record.PreviousReadDateUtc = previousRecord.ReadDateUtc;




                            if (previousRecord.PeriodStartUtc == record.PeriodStartUtc)
                            {
                                // duplicate record in same period - we can skip it unless its a reset 
                                if ( record.IsRollOverOrReset )
                                {
                                    // reset - so we need to keep this record
                                    result.LogMessages.Add(new FileLogMessage($"Reset detected record found for UTC period starting {record.PeriodStartUtc}", Microsoft.Extensions.Logging.LogLevel.Information, filename, line.LineNumber, 0));
                                    // we wont add this but instead update previous record to this one
                                    previousRecord = record;
                                    continue;
                                }
                                else
                                {
                                    // skip 
                                    result.LogMessages.Add(new FileLogMessage($"Duplicate record found for period starting {record.PeriodStartUtc} - skipping", Microsoft.Extensions.Logging.LogLevel.Information, filename, line.LineNumber, 0));
                                    continue;
                                }
                            }

                            if (record.ReadDateUtc < previousRecord.ReadDateUtc)
                            {
                                // hmm - probably not good 
                                result.LogMessages.Add(new FileLogMessage($"Read date time UTC {record.ReadDateUtc} is earlier than previous read date time UTC {previousRecord.ReadDateUtc} ", Microsoft.Extensions.Logging.LogLevel.Warning, filename, line.LineNumber, 0));
                                // skip - but reset previous record
                                previousRecord = record;
                                continue;
                            }
                            previousRecord = record;
                            if (record.Valid)
                            {
                                if ( record.IntervalMinutes > 1 )
                                {
                                    var intervals = record.CreateOneMinuteIntervals();
                                    records.AddRange(intervals);
                                    continue;
                                }
                                else
                                {
                                    records.Add(record);
                                }
                            }
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

                int[] allowedIntervals = [1, 5, 15, 30, 60];
                // group by the local start of period date 
                var list = records.GroupBy(x => x.PeriodStartLocal.Date).Where(x => x.Count() > 1).OrderBy(x => x.Key).ToList();
                int counterTotal = list.Count;
                int counter = 0;

                foreach (var siteDayGroup in list)
                {
                    counter++;
                    cancellationToken?.ThrowIfCancellationRequested();
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
                    double offset = (double)siteDayGroup.Last().UtcOffset;
                    var siteDay = new SiteDay()
                    {
                        SiteCode = siteCode,
                        Date = siteDayGroup.Key,
                        Channels = [],
                        UCT_Offset = (decimal)offset
                    };
                    result.SitesDays.Add(siteDay);

                        var firstRead = siteDayGroup.OrderBy(x => x.ReadDateLocal).First();
                        var secondRead = siteDayGroup.OrderBy(x => x.ReadDateLocal).Skip(1).First();
                        var lastRead = siteDayGroup.OrderBy(x => x.ReadDateLocal).Last();


                        // 1 minute intervals only for now
                        int expectedPeriods = 60 * 24;

                        var channelDayIn = new ChannelDay()
                        {
                            Channel = "E1",
                            ChannelType = ChanelType.ActiveEnergyConsumption,
                            ChannelNumber = "1",
                            IsNet = true,
                            UnitOfMeasure = UnitOfMeasure.kWh,
                            OriginalUnitOfMeasure = UnitOfMeasure.W,
                            OriginalUnitOfMeasureSymbol = "W",
                            IntervalMinutes = 1,
                            Readings = new decimal[expectedPeriods],
                            TimeStampUtc = DateTime.UtcNow,
                            MeterId = "Unknown",
                            RegisterId = "E1",
                            SourceFile = filename,
                            OverallQuality = Quality.Actual,
                        };
                        var channelDayOut = new ChannelDay()
                        {
                            Channel = "B1",
                            ChannelType = ChanelType.ActiveEnergyGeneration,
                            ChannelNumber = "1",
                            IsNet = true,
                            UnitOfMeasure = UnitOfMeasure.kWh,
                            OriginalUnitOfMeasure = UnitOfMeasure.W,
                            OriginalUnitOfMeasureSymbol = "W",
                            IntervalMinutes = 1,
                            Readings = new decimal[expectedPeriods],
                            TimeStampUtc = DateTime.UtcNow,
                            MeterId = "Unknown",
                            RegisterId = "B1",
                            SourceFile = filename,
                            OverallQuality = Quality.Actual,
                        };
                        siteDay.Channels.Add(channelDayIn.Channel, channelDayIn);
                        siteDay.Channels.Add(channelDayOut.Channel, channelDayOut);
                        foreach (var record in siteDayGroup)
                        {
                            int index = (record.PeriodStartLocal.Hour * 60 + record.PeriodStartLocal.Minute)  ;
                            if (index < 0 || index >= expectedPeriods)
                            {
                                result.LogMessages.Add(new FileLogMessage($"Invalid index period found  {index} ", LogLevel.Error, filename, record.LineNumber));
                                continue;
                            }
                            channelDayIn.Readings[index] += record.kWhIn;
                            channelDayOut.Readings[index] -= record.kWhOut;
                        }
                        channelDayIn.Total = channelDayIn.Readings.Sum();
                        channelDayOut.Total = channelDayOut.Readings.Sum();
                    }
                }
            catch (Exception ex)
            {
                result.LogMessages.Add(new FileLogMessage(ex.Message, Microsoft.Extensions.Logging.LogLevel.Critical, filename, 0, 0));
            }

            return;
        }



        public record DataLine(
            DateTime ReadDateUtc,
            DateTime ReadDateLocal,
            int WattsIn,
            int WattsOut,
            int LineNumber
            )
        {
            public int PreviousWattsIn { get; set; } = 0;
            public int PreviousWattsOut { get; set; } = 0;
            public DateTime PreviousReadDateUtc { get; set; }
            public decimal UtcOffset => GetUtcOffsetAsDecimal(ReadDateUtc, ReadDateLocal);

            public DateTime PeriodStartUtc => (PreviousReadDateUtc.Second == 0)
                                                ? PreviousReadDateUtc
                                                : new DateTime(PreviousReadDateUtc.Year, PreviousReadDateUtc.Month, PreviousReadDateUtc.Day, PreviousReadDateUtc.Hour, PreviousReadDateUtc.Minute, 0);


            public DateTime PeriodEndUtcExclusive => ReadDateUtc.Second == 0
                                                    ? ReadDateUtc
                                                    : new DateTime(ReadDateUtc.Year, ReadDateUtc.Month, ReadDateUtc.Day, ReadDateUtc.Hour, ReadDateUtc.Minute, 0)
                                                    ;


            private DateTime PeriodStartLocalNotRoundedToMinute => PeriodStartUtc.AddHours((double)UtcOffset);
            public DateTime PeriodStartLocal => (PeriodStartLocalNotRoundedToMinute.Second == 0)
                                                ? PeriodStartLocalNotRoundedToMinute
                                                : new DateTime(PeriodStartLocalNotRoundedToMinute.Year, PeriodStartLocalNotRoundedToMinute.Month, PeriodStartLocalNotRoundedToMinute.Day, PeriodStartLocalNotRoundedToMinute.Hour, PeriodStartLocalNotRoundedToMinute.Minute, 0);



            private DateTime PeriodEndLocalExclusiveNotRoundedToMinute => PeriodEndUtcExclusive.AddHours((double)UtcOffset);
            public DateTime PeriodEndLocalExclusive => (PeriodEndLocalExclusiveNotRoundedToMinute.Second == 0)
                                                        ? PeriodEndLocalExclusiveNotRoundedToMinute
                                                        : new DateTime(PeriodEndLocalExclusiveNotRoundedToMinute.Year, PeriodEndLocalExclusiveNotRoundedToMinute.Month, PeriodEndLocalExclusiveNotRoundedToMinute.Day, PeriodEndLocalExclusiveNotRoundedToMinute.Hour, PeriodEndLocalExclusiveNotRoundedToMinute.Minute, 0);




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


            public int IntervalMinutes
            {
                get
                {
                    if (PreviousReadDateUtc == DateTime.MinValue )
                    {
                        return 0;
                    }
                    return  Math.Max( (int)PeriodEndUtcExclusive.Subtract(PeriodStartUtc).TotalMinutes, 0 );
                }
            }

            public bool InitialRead => PreviousReadDateUtc == DateTime.MinValue || PreviousReadDateUtc >= ReadDateUtc || IntervalMinutes <= 0;

            public bool IsRollOverOrReset => WattsIn < PreviousWattsIn || WattsOut < PreviousWattsOut;

            public bool Valid => !InitialRead && !IsRollOverOrReset;

            public decimal kWhOut
            {
                get
                {

                    if (!Valid)
                    {
                        return 0;
                    }
                    return (decimal)(WattsOut - PreviousWattsOut) / 1000;
                }
            }

            public decimal kWhIn
            {
                get
                {
                    if (!Valid)
                    {
                        return 0;
                    }
                    return (decimal)(WattsIn - PreviousWattsIn) / 1000;
                }
            }


            public List<DataLine> CreateOneMinuteIntervals()
            {

                if (InitialRead || IsRollOverOrReset)
                {
                    return [];
                }
                if (IntervalMinutes == 1)
                {

                    return [this];
                }

                var result = new List<DataLine>();

                int totalMinutes = IntervalMinutes;

                decimal deltaWattsIn = (decimal)(WattsIn - PreviousWattsIn) / totalMinutes;
                decimal deltaWattsOut = (decimal)(WattsOut - PreviousWattsOut) / totalMinutes;
                decimal currentWattsIn = PreviousWattsIn;
                decimal currentWattsOut = PreviousWattsOut;
                double offset = (double)UtcOffset;

                // starting at PeriodStartUtc - increment in 1 minute intervals until PeriodStartUtc == PeriodEndUtcExclusive
                DateTime current = PeriodStartUtc;





                while (current < PeriodEndUtcExclusive)
                {
                    int previousWattsInInt = (int)Math.Round(currentWattsIn);
                    int previousWattsOutInt = (int)Math.Round(currentWattsOut);
                    
                    currentWattsIn += deltaWattsIn;
                    currentWattsOut += deltaWattsOut;

                    var utcTime = current.AddMinutes(1);
                    var newLocalTime = utcTime.AddHours(offset);
                    int newWattsInInt = (int)Math.Round(currentWattsIn);
                    int newWattsOutInt = (int)Math.Round(currentWattsOut);
                    if (newWattsInInt < previousWattsInInt)
                    {
                        newWattsInInt = previousWattsInInt;
                    }
                    if (newWattsOutInt < previousWattsOutInt)
                    {
                        newWattsOutInt = previousWattsOutInt;
                    }
                    var newDataLine = new DataLine(utcTime, newLocalTime, newWattsInInt, newWattsOutInt, LineNumber)
                    {
                        PreviousWattsIn = previousWattsInInt,
                        PreviousWattsOut = previousWattsOutInt,
                        PreviousReadDateUtc = current
                    };

                    result.Add(newDataLine);
                    current = utcTime;
                }
                return result;


            }
        }
    }
}
