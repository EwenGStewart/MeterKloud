using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MeterDataLib.Parsers
{


    public class CsvSingleLineMultiColPeriod2 : IParser
    {
        public   string Name => "SingleLineByPeriod2";

        public bool CanParse(List<CsvLine> lines)
        {
 
            if (lines.Count < 2) return false;

            // CHECK HEADER ROW
            if (
                    lines[0].ColCount > 24 + 5
                && lines[1].ColCount == lines[0].ColCount
                && lines[0].GetStringUpper(0) == "NMI"
                && lines[0].GetStringUpper(1) == "QUALITY_METHOD"
                && lines[0].GetStringUpper(2) == "METER_NUMBER"
                && lines[0].GetStringUpper(3) == "DATE"
                && lines[0].GetStringUpper(4) == "UNIT_OF_MEASURE"
                && lines[0].GetStringUpper(5) == "TARIFF_DESCRIPTION"
                && lines[1].GetDate(3, "dd/MM/yyyy") != null
                && (lines[1].GetStringUpper(5) == "CONSUMPTION" || lines[1].GetStringUpper(5) == "GENERATION")
                )
            {
                return true;
            }
            return false;
        }
        async Task IParser.Parse(SimpleCsvReader reader, ParserResult result, Func<ParserResult, Task>? callBack, CancellationToken? cancellationToken)
        {
            string filename = reader.Filename;
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();  
            {
                CsvLine line;
                // skip the first line 
                await  reader.ReadAsync();
                cancellationToken?.ThrowIfCancellationRequested();
                try
                {
                    while ((line = await reader.ReadAsync()).Eof == false)
                    {
                        cancellationToken?.ThrowIfCancellationRequested();
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
                        string meter = line.GetStringUpper(2);
                        string conGen = line.GetStringUpper(5);
                        var readDate = line.GetDate(3, "dd/MM/yyyy");
                        if (readDate == null || readDate.Value.Hour > 0 || readDate.Value.Minute > 0)
                        {
                            result.LogMessages.Add(new FileLogMessage($"Invalid date {line.GetStringUpper(3)};", Microsoft.Extensions.Logging.LogLevel.Error, filename, line.LineNumber, 3));
                            continue;
                        }
                        string estimated = line.GetStringUpper(1);
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


                        var siteDate = result.SitesDays.FirstOrDefault(x => x.SiteCode == nmi && x.Date == readDate.Value);
                        if (siteDate == null)
                        {
                            siteDate = new SiteDay()
                            {
                                SiteCode = nmi,
                                Date = readDate.Value,
                                Channels = [],

                            };
                            result.SitesDays.Add(siteDate);
                        }

                        string uomStr = line.GetStringUpper(4);


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

                        var channelDay =
                            new ChannelDay()
                            {
                                Channel = channel,
                                IntervalMinutes = interval,
                                ChannelNumber = channelNum.ToString(),
                                ChannelType = (conGen == "CONSUMPTION" ? ChanelType.ActivePowerConsumption : ChanelType.ActivePowerGeneration),
                                Ignore = false,
                                Readings = new decimal[expectedPeriods],
                                TimeStampUtc = DateTime.UtcNow,
                                MeterId = meter,
                                RegisterId = channel,
                                SourceFile = filename,

                                OverallQuality = (estimated == "NO" ? Quality.Actual : Quality.Substituted),

                                IsAvg = false,
                                IsNet = false,
                                IsCheck = false,

                            };
                        siteDate.Channels.Add(channel, channelDay);
                        var (uomMultiplier, isValid) = CsvParserLib.UpdateUom(channelDay, uomStr);
                        if (!isValid)
                        {
                            result.LogMessages.Add(new FileLogMessage($"Invalid uom  detected {nmi} {meter} {uomStr}", Microsoft.Extensions.Logging.LogLevel.Error, filename, line.LineNumber, line.ColCount - 1));
                            continue;
                        }

                        for (int period = 0; period < expectedPeriods; period++)
                        {
                            var read = line.GetDecimalCol(6 + period);
                            if (read != null)
                            {
                                channelDay.Readings[period] = read.Value * uomMultiplier;
                            }
                            else
                            {
                                result.LogMessages.Add(new FileLogMessage($"Invalid decimal value {line.GetString(5 + period)}", Microsoft.Extensions.Logging.LogLevel.Error, filename, line.LineNumber, line.ColCount - 1));
                                continue;
                            }
                        }
                        channelDay.Total = channelDay.Readings.Sum();
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
