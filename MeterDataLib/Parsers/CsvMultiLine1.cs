using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MeterDataLib.Parsers
{


    public class CsvMultiLine1 : IParser
    {
        public   string Name => "MultiLineCSV1";

        public bool CanParse(List<CsvLine> lines)
        {
            if (lines.Count < 5) return false;
            // CHECK HEADER ROW
            if (
                    lines[0].ColCount == 4
                && lines[1].ColCount == 6
                && lines[2].ColCount == 2
                && lines[3].ColCount > 26
                && lines[4].ColCount == lines[3].ColCount
                && lines[0].GetStringUpper(0) == "NMI"
                && lines[1].GetStringUpper(0) == "STREAM ID"
                && lines[2].GetStringUpper(0) == "LOCAL TIME"
                && lines[3].GetStringUpper(0) == "DATE/TIME"
                && lines[4].GetDate(0, "yyyyMMdd") != null
                )
                return true;
            return false;
        }
        async Task IParser.Parse(SimpleCsvReader reader, ParserResult result, Func<ParserResult, Task>? callBack)
        {
            string filename = reader.Filename;
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start(); 

        
            {
                CsvLine line;
                string nmi = string.Empty;
                string network = string.Empty;
                string meter = string.Empty;
                string uom = string.Empty;
                string channel = string.Empty;
                string equipment = string.Empty;
                int interval = 0;
                int expectedPeriods = 0;
                string localTime = string.Empty;
  

                try
                {
                    while ((line = await reader.ReadAsync()).Eof == false)
                    {
                        if (timer.ElapsedMilliseconds > 100)
                        {
                            result.Progress = $"reading line {line.LineNumber}" ;
                            timer.Restart();
                            if (callBack != null)
                            {
                                await callBack(result);
                            }
                        }

                        if (line.GetStringUpper(0) == "NMI")
                        {
                            nmi = line.GetStringUpper(1);
                            network = line.GetStringUpper(3);
                            continue;
                        }
                        else if (line.GetStringUpper(0) == "STREAM ID")
                        {
                            meter = line.GetStringUpper(1).Replace("METER", "").Trim();
                            channel = line.GetStringUpper(3);
                            uom = line.GetStringUpper(4);
                            if (UnitOfMeasureExtensions.ToUom(uom) == UnitOfMeasure.Other)
                            {
                                result.LogMessages.Add(new FileLogMessage($"Invalid UOM {uom}  ", LogLevel.Warning, filename, line.LineNumber, 4));
                            }
                            continue;
                        }
                        else if (line.GetStringUpper(0) == "LOCAL TIME")
                        {
                            localTime = line.GetStringUpper(1);
                            continue;
                        }
                        else if (line.GetStringUpper(0) == "DATE/TIME")
                        {
                            if (line.ColCount > 288)
                            {
                                interval = 5;
                            }
                            else if (line.ColCount > 96)
                            {
                                interval = 15;
                            }
                            else if (line.ColCount > 48)
                            {
                                interval = 30;
                            }
                            else if (line.ColCount > 24)
                            {
                                interval = 60;
                            }
                            else
                            {
                                result.LogMessages.Add(new FileLogMessage("Invalid inter or column count- cannot process file", Microsoft.Extensions.Logging.LogLevel.Error, filename, line.LineNumber, line.ColCount - 1));
                                return ;
                            }
                            expectedPeriods = 60 * 24 / interval;
                            continue;
                        }
                        else if (line.GetDate(0, "yyyyMMdd") != null)
                        {
                            DateTime readDate = line.GetDate(0, "yyyyMMdd")!.Value;

                            var siteDay = result.SitesDays.FirstOrDefault(x => x.Site == nmi && x.Date == readDate);
                            if (siteDay == null)
                            {
                                siteDay = new SiteDay()
                                {
                                    Site = nmi,
                                    Date = readDate,
                                    Channels = new Dictionary<string, ChannelDay>(),
                                    TimeZoneName = localTime,
                                    UCT_Offset = (localTime == "AEST" ? 10 : null),
                                };
                                result.SitesDays.Add(siteDay);
                            }


                            ChannelDay channelDay = new ChannelDay()
                            {
                                Channel = channel,
                                IntervalMinutes = interval,
                                Readings = new decimal[expectedPeriods],
                                TimeStampUtc = DateTime.UtcNow,
                                MeterId = meter,
                                RegisterId = channel,
                                SourceFile = filename,
                            };


                            if (siteDay.Channels.ContainsKey(channel) == false)
                            {
                                siteDay.Channels.Add(channel, channelDay);
                            }
                            else
                            {
                                result.LogMessages.Add(new FileLogMessage($"Duplicate channel {channel} in file - old data will be replaced ", Microsoft.Extensions.Logging.LogLevel.Warning, filename, line.LineNumber, 0));
                                siteDay.Channels[channel] = channelDay;
                            }


                            string quality = line.GetStringUpper(expectedPeriods + 1);

                            var uomResult = CsvParserLib.UpdateUom(channelDay, uom);
                            CsvParserLib.UpdateFromAemoChannel(channelDay);




                            for (int col = 1; col <= expectedPeriods; col++)
                            {
                                if (line.GetDecimalCol(col) != null)
                                {
                                    var read = line.GetDecimalCol(col);
                                    if (read == null)
                                    {
                                        result.LogMessages.Add(new FileLogMessage($"Invalid decimal {line.GetString(col)}", Microsoft.Extensions.Logging.LogLevel.Error, filename, line.LineNumber, col));
                                        read = 0;
                                    }
                                    channelDay.Readings[col - 1] = read.Value * uomResult.mult;
                                }
                            }

                            channelDay.Total = channelDay.Readings.Sum();







                            continue;
                        }
                        else if (line.GetString(0).Equals("Total for Period", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.LogMessages.Add(new FileLogMessage(ex.Message, Microsoft.Extensions.Logging.LogLevel.Critical, filename, reader.LineNumber, 0));
                }
            }
            //     result.Success = result.SitesDays.Any() && result.Errors == 0;
            return  ;
        }
    }
}
