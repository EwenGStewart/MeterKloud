using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MeterDataLib.Parsers
{


    public class CsvByChannel : IParser
    {
        public string Name => "CsvByChannel";

        public bool CanParse(List<CsvLine> lines)
        {
            if (lines.Count < 2) return false;
            if (
               lines[0].ColCount > 2
               && lines[0].GetStringUpper(0) == "NMI"
               && lines[0].GetStringUpper(1) == "READ DATE/TIME"
               && lines[1].GetStringUpper(0).Length >= 10
               && lines[1].GetStringUpper(0).Length <= 11
               && lines[1].GetDate(1, ["dd.MM.yyyy HH:mm:ss", "dd/MM/yyyy HH:mm:ss"]) is not null
               )
                return true;
            return false;
        }





        async Task IParser.Parse(SimpleCsvReader reader, ParserResult result, Func<ParserResult, Task>? callBack, CancellationToken? cancellationToken   )
        {
            string filename = reader.Filename;
            var records = new List<DataLine>();
            try
            {
                CsvLine header;
                int channels;
                string[] channelNames;
               

              //  Console.WriteLine($"{DateTime.Now:hh:mm:ss:fff} CsvByChannel Parsing file {filename}");
                

                CsvLine line;

                // skip the first line 
                header = await reader.ReadAsync();

                channels = header.ColCount - 2;
                if (channels < 1)
                {
                    result.LogMessages.Add(new FileLogMessage($"Invalid header found {header}", Microsoft.Extensions.Logging.LogLevel.Error, filename, reader.LineNumber, 5));
                    return ;
                }

                channelNames = new string[channels];
                for (int i = 0; i < channels; i++)
                {
                    channelNames[i] = header.GetStringUpper(i + 2);
                }

             //   Console.WriteLine($"{DateTime.Now:hh:mm:ss:fff} CsvByChannel start lines  file {filename}");
                line = await reader.ReadAsync();
                var timer = new System.Diagnostics.Stopwatch();
                timer.Start();
                while (!line.Eof)
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

                    
                        var readDateTime = line.GetDate(1, ["dd.MM.yyyy HH:mm:ss", "dd/MM/yyyy HH:mm:ss"]);
                        var nmi = line.GetStringUpper(0);
                        if (string.IsNullOrEmpty(nmi))
                        {
                            continue;
                        }
                        decimal[] reads = new decimal[channels];
                        for (int i = 0; i < channels; i++)
                        {
                            reads[i] = line.GetDecimalCol(i + 2) ?? 0;
                        }
                        if (readDateTime == null)
                        {
                            result.LogMessages.Add(new FileLogMessage($"Invalid date or time {line.GetString(0)}", Microsoft.Extensions.Logging.LogLevel.Error, filename, reader.LineNumber, 6));
                            continue;
                        }
           
                        var record = new DataLine(readDateTime.Value, nmi, reads, line.LineNumber);
                        records.Add(record);
                        line = await reader.ReadAsync();

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
 
                int[] allowedIntervals = [1, 5, 15, 30, 60];
                int counter = 0;
                var list = records
                    .GroupBy(x => new { x.Nmi, ReadDate = x.ReadDate.Date })
                    .Where(x => x.Count() > 1)
                    .OrderBy(x => x.Key.Nmi)
                    .ThenBy(x => x.Key.ReadDate)
                    .ToList();
                var counterTotal = list.Count;
                foreach (var siteDayGroup in list)
                {
                    counter++;
                    cancellationToken?.ThrowIfCancellationRequested();
                    if (timer.ElapsedMilliseconds > 100)
                    {
                        int percentage = counter * 100 / counterTotal ;
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
                        UCT_Offset = 10,
                        TimeZoneName = "AEST"
                    };
                    result.SitesDays.Add(siteDay);

                    var firstRead = siteDayGroup.OrderBy(x => x.ReadDate).First();
                    var secondRead = siteDayGroup.OrderBy(x => x.ReadDate).Skip(1).First();

                    var readInterval = (int)secondRead.ReadDate.Subtract(firstRead.ReadDate).TotalMinutes;
                    if (!allowedIntervals.Contains(readInterval))
                    {
                        result.LogMessages.Add(new FileLogMessage($"Invalid interval [{readInterval}] between reads found ", Microsoft.Extensions.Logging.LogLevel.Error, filename, firstRead.LineNumber, 0));
                        continue;
                    }
                    int expectedPeriods = 60 * 24 / readInterval;



                    for (int i = 0; i < channels; i++)
                    {
                        string channel = channelNames[i];

                        var channelDay = new ChannelDay()
                        {
                            Channel = channel,
                            IntervalMinutes = readInterval,
                            Readings = new decimal[expectedPeriods],
                            TimeStampUtc = DateTime.UtcNow,
                            RegisterId = channel,
                            SourceFile = filename,
                            OverallQuality = Quality.Actual,
                        };
                        CsvParserLib.UpdateFromAemoChannel(channelDay);
                        siteDay.Channels.Add(channel, channelDay);

                        foreach (var record in siteDayGroup)
                        {
                            int index = (record.ReadDate.Hour * 60 + record.ReadDate.Minute) / readInterval;
                            if (index < 0 || index >= channelDay.Readings.Length)
                            {
                                continue;
                            }
                            decimal value = record.Reads[i];
                            channelDay.Readings[index] += value;
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

        record DataLine(
            DateTime ReadDate,
            string Nmi,
            decimal[] Reads,
            int LineNumber
                        )
        { }


    }
}
