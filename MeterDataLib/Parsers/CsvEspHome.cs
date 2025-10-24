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
                            if ( readDateTimeUtc.Second != 0)
                            {
                                readDateTimeUtc = new DateTime(readDateTimeUtc.Year, readDateTimeUtc.Month, readDateTimeUtc.Day, readDateTimeUtc.Hour, readDateTimeUtc.Minute, 0);
                            }
                            if ( readDateTimeLocal.Second != 0)
                            {
                                readDateTimeLocal = new DateTime(readDateTimeLocal.Year, readDateTimeLocal.Month, readDateTimeLocal.Day, readDateTimeLocal.Hour, readDateTimeLocal.Minute, 0);
                            }

                            var totalWattsIn     = line.GetIntMandatory(2);
                            var totalWattsOut = line.GetIntCol(3);
                            var record = new DataLine(readDateTimeUtc, readDateTimeLocal, totalWattsIn, totalWattsOut ?? 0 , line.LineNumber);
                            if ( previousRecord != null  && previousRecord.ReadDateUtc == readDateTimeUtc )
                            {
                                // duplicate record skip it
                                continue;

                            }
                            if ( previousRecord != null && record.ReadDateUtc < previousRecord.ReadDateUtc )
                            {
                                result.LogMessages.Add(new FileLogMessage($"Read date time UTC {record.ReadDateUtc} is earlier than previous read date time UTC {previousRecord.ReadDateUtc} ", Microsoft.Extensions.Logging.LogLevel.Error, filename, line.LineNumber, 0));
                                continue;
                            }

                            if (previousRecord != null)
                            {
                                record.PreviousWattsOut = previousRecord.WattsOut;
                                record.PreviousWattsIn = previousRecord.WattsIn; ;
                                record.PreviousReadDateUtc = previousRecord.PreviousReadDateUtc;
                            }
                            else
                            {
                                record.PreviousWattsOut = record.WattsOut;
                                record.PreviousWattsIn = record.WattsIn; ;
                                record.PreviousReadDateUtc = record.ReadDateUtc;

                            }
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

                //int[] allowedIntervals = [1, 5, 15, 30, 60];
                //var list = records.GroupBy(x => x.ReadDateLocal.Date).Where(x => x.Count() > 1).OrderBy(x => x.Key).ToList();
                //int counterTotal = list.Count;
                //int counter = 0;

                //foreach (var siteDayGroup in list)
                //{
                //    counter++;
                //    cancellationToken?.ThrowIfCancellationRequested();
                //    if (timer.ElapsedMilliseconds > 100)
                //    {
                //        int percentage = counter * 100 / counterTotal;
                //        result.Progress = $"parsing {percentage}%";
                //        timer.Restart();
                //        if (callBack != null)
                //        {
                //            await callBack(result);
                //        }
                //    }
                //    var siteDay = new SiteDay()
                //    {
                //        SiteCode = siteCode,
                //        Date = siteDayGroup.Key,
                //        Channels = [],
                //        UCT_Offset = siteDayGroup.First().UtcOffset
                //    };
                //    result.SitesDays.Add(siteDay);

                //    var firstRead = siteDayGroup.OrderBy(x => x.ReadDateLocal).First();
                //    var secondRead = siteDayGroup.OrderBy(x => x.ReadDateLocal).Skip(1).First();

                //    var readInterval = (int)secondRead.ReadDateUtc.Subtract(firstRead.ReadDateUtc).TotalMinutes;
                //    if (!allowedIntervals.Contains(readInterval))
                //    {
                //        result.LogMessages.Add(new FileLogMessage($"Invalid interval [{readInterval}] between reads found ", Microsoft.Extensions.Logging.LogLevel.Error, filename, firstRead.LineNumber, 0));
                //        continue;
                //    }
                //    int expectedPeriods = 60 * 24 / readInterval;

                //    var channelDay = new ChannelDay()
                //    {
                //        Channel = "E1",
                //        ChannelType = ChanelType.ActiveEnergyConsumption,
                //        ChannelNumber = "1",
                //        IsNet = true,
                //        UnitOfMeasure = UnitOfMeasure.kWh,
                //        OriginalUnitOfMeasure = UnitOfMeasure.W,
                //        OriginalUnitOfMeasureSymbol = "W",
                //        IntervalMinutes = readInterval,
                //        Readings = new decimal[expectedPeriods],
                //        TimeOfUseClasses = new TimeOfUseClass[expectedPeriods],
                //        TimeStampUtc = DateTime.UtcNow,
                //        MeterId = "Unknown",
                //        RegisterId = "E1",
                //        SourceFile = filename,
                //        OverallQuality = Quality.Actual,

                //    };
                //    var channelDayCost = new ChannelDay()
                //    {
                //        Channel = "COST",
                //        IntervalMinutes = 60,
                //        ChannelType = ChanelType.Cost,
                //        ChannelNumber = "1",
                //        Readings = new decimal[expectedPeriods],
                //        UnitOfMeasure = UnitOfMeasure.Dollars,
                //        TimeOfUseClasses = new TimeOfUseClass[expectedPeriods],
                //        TimeStampUtc = DateTime.UtcNow,
                //        MeterId = "Unknown",
                //        RegisterId = "COST",
                //        SourceFile = filename,
                //        OverallQuality = Quality.Actual,
                //    };

                //    siteDay.Channels.Add(channelDay.Channel, channelDay);
                //    siteDay.Channels.Add(channelDayCost.Channel, channelDayCost);

                //    foreach (var record in siteDayGroup)
                //    {
                //        int index = (record.ReadDateLocal.Hour * 60 + record.ReadDateLocal.Minute) / readInterval;
                //        if (index < 0 || index >= channelDay.Readings.Length)
                //        {
                //            result.LogMessages.Add(new FileLogMessage($"Invalid index period found  {index} ", LogLevel.Error, filename, record.LineNumber));
                //            continue;
                //        }
                //        channelDay.Readings[index] += record.ReadValue / 1000;
                //        channelDay.TimeOfUseClasses[index] = record.Tou;
                //        channelDayCost.Readings[index] += record.CostDollars;
                //        channelDayCost.TimeOfUseClasses[index] = record.Tou;

                //    }
                //    channelDay.Total = channelDay.Readings.Sum();
                //    channelDayCost.Total = channelDayCost.Readings.Sum();
                //}
            }
            catch (Exception ex)
            {
                result.LogMessages.Add(new FileLogMessage(ex.Message, Microsoft.Extensions.Logging.LogLevel.Critical, filename, 0, 0));
            }

            return;
        }
 


        record DataLine(
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
