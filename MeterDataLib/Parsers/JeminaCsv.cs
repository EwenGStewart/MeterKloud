using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace MeterDataLib.Parsers
{
    public class JeminaCsv : IParser
    {
        public string Name => "Jemina";

        public bool CanParse(List<CsvLine> lines)
        {
            if (lines.Count < 2) return false;
            try
            {
                // CHECK HEADER ROW
                if (

                       lines[0].GetStringUpper(0) == "NMI"
                    && lines[0].GetStringUpper(1) == "METER SERIAL NUMBER"
                    && lines[0].GetStringUpper(2) == "CON/GEN"
                    && lines[0].GetStringUpper(3) == "DATE"
                    && lines[0].GetStringUpper(4) == "ESTIMATED?"
                    && lines[0].GetStringUpper(5) == "00:00 - 00:30"
                    && lines[1].GetDecimalMandatory(5) >= decimal.MinValue
                    && lines[0].ColCount == 48+5 /* 48 reads + 5 initial columns */
                    && lines[1].ColCount == 48 + 5 /* 48 reads + 5 initial columns */
                    )
                    return true;
            }
            catch
            {
                // ignore errors
                return false;
            }
            return false;

        }

        const string SUFFIXES ="123456789ABCDEFGHJKLMNPQRSTUVWXYZ";

        async Task IParser.Parse(SimpleCsvReader reader, ParserResult result, Func<ParserResult, Task>? callBack, CancellationToken? cancellationToken)
        {


            string filename = reader.Filename;
            var records = new List<DataLine>();
            var timer = new System.Diagnostics.Stopwatch();
            Dictionary<string,int> meters = new Dictionary<string,int>();
            try
            {
                {
                    CsvLine line;
                    // skip the first line 
                    await reader.ReadAsync();
                    timer.Start();
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

                            string nmi = line.GetStringUpper(0);
                            string meter = line.GetStringUpper(1);
                            // remove the leading zeros from meter 
                            meter = meter.TrimStart('0');
                            int channelNum = 1;
                            if ( meters.Count == 0 || meters.ContainsKey(meter) == false )
                            {
                                meters[meter] = meters.Count + 1;
                                channelNum = meters[meter];
                            }
                            else
                            {
                                channelNum = meters[meter];
                            }

                            if ( channelNum > SUFFIXES.Length )
                            {
                                result.LogMessages.Add(new FileLogMessage($"Too many channels for meter {meter}", Microsoft.Extensions.Logging.LogLevel.Error, filename, reader.LineNumber, 1));
                                continue;
                            }
                            string sufffixChannel = SUFFIXES[channelNum - 1].ToString();

                            string conGen = line.GetStringUpper(2);
                            string suffix = "";
                            if ( conGen == "CONSUMPTION")
                            {
                                suffix = "E" + sufffixChannel;

                            }
                            else if (conGen == "GENERATION")
                            {
                                suffix = "B" + sufffixChannel;
                            }
                            else
                            {
                                result.LogMessages.Add(new FileLogMessage($"Invalid CON/GEN value {conGen}", Microsoft.Extensions.Logging.LogLevel.Error, filename, reader.LineNumber, 2));
                                continue;
                            }
                          
                    
                            var readDate = line.GetDateMandatory(3, ["yyyy-MM-dd" ]);
                            string estimatedTxt = line.GetStringUpper(4);
                            bool actualread = estimatedTxt == "NO";
                            decimal[] reads = new decimal[48];
                            for (int i = 0; i < 48; i++)
                            {
                                int colIndex = 5 + i;
                                reads[i] = line.GetDecimalMandatory(5 + i);
                            }

                            records.Add(
                              new DataLine(nmi, meter, suffix, readDate,   reads, actualread  , line.LineNumber)
                              );

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

              
                var list = records
                                            .GroupBy(x => new { Nmi = x.Nmi, ReadDate = x.ReadDate.Date })
                                            .Where(x => x.Count() > 1)
                                            .OrderBy(x => x.Key.Nmi)
                                            .ThenBy(x => x.Key.ReadDate)
                                            .ToList()
                                            ;
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

                    var siteDay = new SiteDay()
                    {
                        SiteCode = siteDayGroup.Key.Nmi,
                        Date = siteDayGroup.Key.ReadDate,
                        UCT_Offset =10, 
                        TimeZoneName = "Australia/Sydney",
                        Channels = [],
                    };
                    result.SitesDays.Add(siteDay);
                    foreach (var record in siteDayGroup )
                    {
                        var firstRead = siteDayGroup.OrderBy(x => x.ReadDate).First();
                        var channel = record.Suffix;
                        ChannelDay channelDay = new()
                        {
                            Channel = channel,
                            ChannelNumber = channel.Substring(1,1),
                            IntervalMinutes = 30,
                            Readings =  record.Reads,
                            TimeStampUtc = DateTime.UtcNow,
                            MeterId = record.Meter,
                            RegisterId = channel,
                            SourceFile = filename,
                            Total = record.Reads.Sum(),
                            OverallQuality = record.Actual ? Quality.Actual : Quality.Estimated,
                        };
                        CsvParserLib.UpdateUom(channelDay, "KWH");
                        CsvParserLib.UpdateFromAemoChannel(channelDay);
                        siteDay.Channels.Add(channel, channelDay);
                       
                    }
                }

            }
            catch (Exception ex)
            {
                result.LogMessages.Add(new FileLogMessage(ex.Message, Microsoft.Extensions.Logging.LogLevel.Critical, filename, 0, 0));
            }


        }


        record DataLine(
            string Nmi,
            string Meter,
            string Suffix,
            DateTime ReadDate,
            decimal[] Reads,
            bool Actual,
            int LineNumber
            )
        {
            public int Interval => 30; // 30 minutes
        }
    }

}
