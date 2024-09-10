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


    public class CsvSingleLine7 : IParser
    {
        public   string Name => "SingleLineFromLine7";

        public bool CanParse(List<CsvLine> lines)
        {

 
            if (lines.Count < 5) return false;

            // CHECK HEADER ROW
            if (
                lines[0].GetStringUpper(0) == "ACCOUNT NUMBER"
                && lines[3].ColCount == 11
                && lines[4].ColCount == 11
                && lines[3].GetStringUpper(0) == "NMI"
                && lines[3].GetStringUpper(1) == "METER SERIAL NUMBER"
                && lines[3].GetStringUpper(2) == "STREAM"
                && lines[3].GetStringUpper(3) == "SUFFIX"
                && lines[3].GetStringUpper(4) == "REGISTER DESCRIPTION"
                && lines[3].GetStringUpper(5) == "UOM"
                && lines[3].GetStringUpper(6) == "READING DATE"
                && lines[3].GetStringUpper(7) == "READING START TIME"
                && lines[3].GetStringUpper(8) == "READING END TIME"
                && lines[3].GetStringUpper(9) == "UNITS"
                && lines[3].GetStringUpper(10) == "READ TYPE"
                && lines[4].GetDate(6, ["d/M/yyyy", "dd/MM/yyyy"]) != null
                && lines[4].GetTime(7, "HH:mm") != null
                && lines[4].GetTime(8, "HH:mm") != null
                )
                return true;
            return false;
        }


        async Task IParser.Parse(SimpleCsvReader reader, ParserResult result, Func<ParserResult, Task>? callBack, CancellationToken? cancellationToken)
        {


            string filename = reader.Filename;
            var records = new List<DataLine>();
            var timer = new System.Diagnostics.Stopwatch();
            try
            {
                {
                    CsvLine line;
                    // skip the first line 
                    await reader.ReadAsync(4);
                 
                    timer.Start();
                    while ((line = await  reader.ReadAsync()).Eof == false)
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

                            string nmi = line.GetStringUpper(0);
                            string meter = line.GetStringUpper(1);
                            string marketStream = line.GetStringUpper(2);
                            string suffix = line.GetStringUpper(3);
                            string registerDescription = line.GetStringUpper(4);
                            string uom = line.GetStringUpper(5);
                            var readDate = line.GetDate(6, ["d/M/yyyy", "dd/MM/yyyy"]);
                            var startReadTime = line.GetTime(7, "HH:mm");
                            var endReadTime = line.GetTime(8, "HH:mm");
                            decimal Units = line.GetDecimalCol(9) ?? 0;
                            string readType = line.GetStringUpper(10);

                            if (readDate == null)
                            {
                                result.LogMessages.Add(new FileLogMessage($"Invalid date or time {line.GetString(6)}", Microsoft.Extensions.Logging.LogLevel.Error, filename, reader.LineNumber, 6));
                                continue;
                            }

                            if (startReadTime == null)
                            {
                                result.LogMessages.Add(new FileLogMessage($"Invalid start time {line.GetString(7)}", Microsoft.Extensions.Logging.LogLevel.Error, filename, reader.LineNumber, 7));
                                continue;
                            }

                            if (endReadTime == null)
                            {
                                result.LogMessages.Add(new FileLogMessage($"Invalid end time {line.GetString(8)}", Microsoft.Extensions.Logging.LogLevel.Error, filename, reader.LineNumber, 8));
                                continue;
                            }


                            var record = new DataLine(nmi, meter, marketStream, suffix, registerDescription, uom, readDate.Value, startReadTime.Value, endReadTime.Value, Units, readType, line.LineNumber);
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

                int[] allowedIntervals = [1, 5, 15, 30, 60];
                var list = records
                                            .GroupBy(x => new { Nmi = x.Nmi, ReadDate = x.ReadDate.Date })
                                            .Where(x => x.Count() > 1)
                                            .OrderBy(x => x.Key.Nmi)
                                            .ThenBy(x => x.Key.ReadDate)
                                            .ToList()
                                            ;
                int counterTotal = list.Count;
                int counter = 0;

                foreach (var siteDayGroup in list  )
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

                    var siteDay = new SiteDay()
                    {
                        SiteCode = siteDayGroup.Key.Nmi,
                        Date = siteDayGroup.Key.ReadDate,
                        Channels = [],
                    };
                    result.SitesDays.Add(siteDay);
                    foreach (var channelGroup in siteDayGroup.GroupBy(x => x.Suffix).Where(x => x.Count() > 1))
                    {
                        var firstRead = siteDayGroup.OrderBy(x => x.StartDateTime).First();
                        var readInterval = firstRead.Interval;
                        var readExpectedPeriods = 60 * 24 / readInterval;
                        var channel = channelGroup.Key;
                        ChannelDay channelDay = new()
                        {
                            Channel = channel,
                            IntervalMinutes = readInterval,
                            Readings = new decimal[readExpectedPeriods],
                            TimeOfUseClasses = new TimeOfUseClass[readExpectedPeriods],
                            TimeStampUtc = DateTime.UtcNow,
                            MeterId = channelGroup.First().Meter,
                            RegisterId = channel,
                            SourceFile = filename,
                        };
                        if (siteDay.Channels.ContainsKey(channel) == false)
                        {
                            siteDay.Channels.Add(channel, channelDay);
                        }
                        else
                        {
                            result.LogMessages.Add(new FileLogMessage($"Duplicate channel {channel} in file - old data will be replaced ", Microsoft.Extensions.Logging.LogLevel.Warning, filename, firstRead.LineNumber, 0));
                            siteDay.Channels[channel] = channelDay;
                        }
                        var (multiplier, isValid) = CsvParserLib.UpdateUom(channelDay, channelGroup.First().Uom);
                        CsvParserLib.UpdateFromAemoChannel(channelDay);

                        foreach (var record in channelGroup)
                        {
                            int index = (record.StartDateTime.Hour * 60 + record.StartDateTime.Minute) / readInterval;
                            if (index < 0 || index >= channelDay.Readings.Length)
                            {
                                result.LogMessages.Add(new FileLogMessage($"Invalid index period found  {index} ", LogLevel.Error, filename, record.LineNumber));
                                continue;
                            }
                            channelDay.Readings[index] = record.Units * multiplier;
                            channelDay.TimeOfUseClasses[index] = record.Tou;
                        }
                        channelDay.Total = channelDay.Readings.Sum();
                    }
                }

            }
            catch (Exception ex)
            {
                result.LogMessages.Add(new FileLogMessage(ex.Message, Microsoft.Extensions.Logging.LogLevel.Critical, filename, 0, 0));
            }

          
        }


        record DataLine(string Nmi,
            string Meter,
            string Stream,
            string Suffix,
            string RegisterDescription,
            string Uom,
            DateTime ReadDate,
            TimeOnly ReadStartTime,
            TimeOnly ReadEndTime,
            decimal Units,
            string ReadType,
            int LineNumber
            )
        {
            public DateTime StartDateTime => ReadDate.Add(ReadStartTime.ToTimeSpan());

            public bool EndOfDay => ReadEndTime == new TimeOnly(0, 0);

            public DateTime EndDateTimeExclusive => EndOfDay ? ReadDate.AddDays(1) : ReadDate.Add(ReadEndTime.ToTimeSpan());

            public DateTime EndDateTimeInclusive => EndDateTimeExclusive.Subtract(new TimeSpan(0, 1, 0));
            public int Interval => EndDateTimeExclusive.Subtract(StartDateTime).Minutes;
            public TimeOfUseClass Tou => RegisterDescription == "PEAK" ? TimeOfUseClass.Peak : TimeOfUseClass.Offpeak;

        }






    }
}
