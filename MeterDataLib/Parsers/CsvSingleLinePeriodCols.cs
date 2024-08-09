using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MeterDataLib.Parsers
{


    public class CsvSingleLineMultiColPeriod : IParser
    {
        public   string Name => "SingleLineByPeriod";

        public bool CanParse(List<CsvLine> lines)
        {
            if (lines.Count < 2) return false;
            // CHECK HEADER ROW
            if (
                    lines[0].ColCount > 24 + 5
                && lines[1].ColCount == lines[0].ColCount
                && lines[0].GetStringUpper(0) == "NMI"
                && lines[0].GetStringUpper(1) == "METER SERIAL NUMBER"
                && lines[0].GetStringUpper(2) == "CON/GEN"
                && lines[0].GetStringUpper(3) == "DATE"
                && lines[0].GetStringUpper(4) == "ESTIMATED?"
                && lines[1].GetDate(3, "d/MM/yyyy") != null
                && (lines[1].GetStringUpper(2) == "CONSUMPTION" || lines[1].GetStringUpper(2) == "GENERATION")
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
                // skip the first line 
                await reader.ReadAsync();
                try
                {
                    while ((line = await reader.ReadAsync()).Eof == false)
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
                        string conGen = line.GetStringUpper(2);
                        var readDate = line.GetDate(3, "d/MM/yyyy");
                        if (readDate == null || readDate.Value.Hour > 0 || readDate.Value.Minute > 0)
                        {
                            result.LogMessages.Add(new FileLogMessage($"Invalid date {line.GetStringUpper(3)};", Microsoft.Extensions.Logging.LogLevel.Error, filename, line.LineNumber, 3));
                            continue;
                        }
                        string estimated = line.GetStringUpper(4);
                        int interval = 0;
                        int expectedPeriods = 0;
                        if (line.ColCount >= 288 + 5)
                        {
                            interval = 5;
                            expectedPeriods = 288;
                        }
                        else if (line.ColCount >= 96 + 5)
                        {
                            interval = 15;
                            expectedPeriods = 96;
                        }
                        else if (line.ColCount >= 48 + 5)
                        {
                            interval = 30;
                            expectedPeriods = 48;
                        }
                        else if (line.ColCount >= 24 + 5)
                        {
                            interval = 60;
                            expectedPeriods = 24;
                        }
                        else
                        {
                            result.LogMessages.Add(new FileLogMessage("Invalid interval or column count- cannot process line", Microsoft.Extensions.Logging.LogLevel.Error, filename, line.LineNumber, line.ColCount - 1));
                            continue;
                        }
                        decimal[] reads = new decimal[expectedPeriods];
                        for (int period = 0; period < expectedPeriods; period++)
                        {
                            var read = line.GetDecimalCol(5 + period);
                            if (read != null)
                            {
                                reads[period] = read.Value;
                            }
                            else
                            {
                                result.LogMessages.Add(new FileLogMessage($"Invalid decimal value {line.GetString(5 + period)}", Microsoft.Extensions.Logging.LogLevel.Error, filename, line.LineNumber, line.ColCount - 1));
                                continue;

                            }
                        }

                        var siteDate = result.SitesDays.FirstOrDefault(x => x.SiteCode == nmi && x.Date == readDate.Value);
                        if (siteDate == null)
                        {
                            siteDate = new SiteDay()
                            {
                                SiteCode = nmi,
                                Date = readDate.Value,
                                Channels = new Dictionary<string, ChannelDay>(),

                            };
                            result.SitesDays.Add(siteDate);
                        }

                        int channelNum = 1;
                        string channel = (conGen == "CONSUMPTION" ? "E" : conGen == "GENERATION" ? "B" : "?") + channelNum.ToString();
                        while (siteDate.Channels.ContainsKey(channel) && channelNum < 10)
                        {
                            channelNum++;
                            channel = (conGen == "CONSUMPTION" ? "E" : conGen == "GENERATION" ? "B" : "?") + channelNum.ToString();
                        }
                        if (channelNum > 10)
                        {
                            result.LogMessages.Add(new FileLogMessage($"Duplicate streams detected {nmi} {meter} {conGen} {channel}", Microsoft.Extensions.Logging.LogLevel.Error, filename, line.LineNumber, line.ColCount - 1));
                            continue;
                        }


                        siteDate.Channels.Add(channel, new ChannelDay()
                        {
                            Channel = channel,
                            IntervalMinutes = interval,
                            ChannelNumber = channelNum.ToString(),
                            ChannelType = (conGen == "CONSUMPTION" ? ChanelType.ActivePowerConsumption : ChanelType.ActivePowerGeneration),
                            Ignore = false,
                            Readings = reads,
                            TimeStampUtc = DateTime.UtcNow,
                            MeterId = meter,
                            RegisterId = channel,
                            SourceFile = filename,
                            UnitOfMeasure = UnitOfMeasure.kWh,
                            OverallQuality = (estimated == "NO" ? Quality.Actual : Quality.Substituted),
                            Total = reads.Sum(),
                            IsAvg = false,
                            IsNet = false,
                            IsCheck = false,

                        });

                    }
                }
                catch (Exception ex)
                {
                    result.LogMessages.Add(new FileLogMessage(ex.Message, Microsoft.Extensions.Logging.LogLevel.Critical, filename, reader.LineNumber, 0));
                }
            }

            return ;
        }
    }

}
