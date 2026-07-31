using Microsoft.Extensions.Logging;

namespace MeterDataLib.Parsers
{
    public class CsvMultiLine2 : IParser
    {
        public string Name => "CsvMultiLine2";

        public bool CanParse(List<CsvLine> lines)
        {
            if (lines.Count < 5) return false;

            // This format has blank lines in the file, but the CSV reader skips them
            // So we need to check the non-blank lines:
            // Line 0: NMI line with 4 columns
            // Line 1: Stream ID with multiple columns  
            // Line 2: LOCAL TIME with 2 columns
            // Line 3: Date/Time header with many columns
            // Line 4: First data row

            // The key difference from CsvMultiLine1 is that Stream ID column 1 contains
            // meter info with commas (e.g., "Meter 253011620,253011620,253011620")
            // which means after quote parsing it will still have commas in the string
            if (lines[0].ColCount == 4
                && lines[0].GetStringUpper(0) == "NMI"
                && lines[1].ColCount >= 6
                && lines[1].GetStringUpper(0) == "STREAM ID"
                && lines[1].GetStringUpper(1).Contains(",") // Multiple meter numbers separated by commas
                && lines[2].ColCount == 2
                && lines[2].GetStringUpper(0) == "LOCAL TIME"
                && lines[3].ColCount > 26
                && lines[3].GetStringUpper(0) == "DATE/TIME"
                && lines[4].GetDate(0, "yyyyMMdd") != null
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

            result.ParserName = Name;

            try
            {
                CsvLine line;
                string nmi = string.Empty;
                string network = string.Empty;
                string localTime = string.Empty;

                // Track if we're in a channel block
                bool inChannelBlock = false;
                string currentMeter = string.Empty;
                string currentChannel = string.Empty;
                string currentUom = string.Empty;
                string currentType = string.Empty; // "Solar" or "CONSUMPTION"
                int interval = 0;
                int expectedPeriods = 0;

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

                    // Parse NMI line
                    if (line.GetStringUpper(0) == "NMI")
                    {
                        nmi = line.GetStringUpper(1);
                        if (line.ColCount >= 4)
                        {
                            network = line.GetStringUpper(3);
                        }
                        continue;
                    }

                    // Parse Stream ID line (start of a channel block)
                    if (line.GetStringUpper(0) == "STREAM ID")
                    {
                        inChannelBlock = true;

                        // Column 1: Meter info (e.g., "Meter 253011620,253011620,253011620")
                        string meterInfo = line.GetStringUpper(1);
                        // Extract first meter number
                        if (meterInfo.Contains("METER"))
                        {
                            currentMeter = meterInfo.Replace("METER", "").Trim().Split(',')[0].Trim();
                        }
                        else
                        {
                            currentMeter = meterInfo.Split(',')[0].Trim();
                        }

                        // Column 2 or 3: Channel (e.g., "B1", "E1", "E2")
                        currentChannel = line.GetStringUpper(2);
                        if (string.IsNullOrWhiteSpace(currentChannel) && line.ColCount > 3)
                        {
                            currentChannel = line.GetStringUpper(3);
                        }

                        // Column 4: UOM (e.g., "KWH")
                        currentUom = line.GetStringUpper(4);

                        // Column 5: Type (e.g., "Solar", "CONSUMPTION")
                        currentType = line.GetStringUpper(5);

                        continue;
                    }

                    // Parse LOCAL TIME line
                    if (line.GetStringUpper(0) == "LOCAL TIME")
                    {
                        localTime = line.GetStringUpper(1);
                        continue;
                    }

                    // Parse Date/Time header line (determine interval)
                    if (line.GetStringUpper(0) == "DATE/TIME")
                    {
                        // Determine interval based on column count
                        // 5-minute intervals have 288 data columns plus Date/Time, Quality, Total = 290+ columns
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
                            result.LogMessages.Add(new FileLogMessage(
                                "Invalid interval or column count - cannot process file",
                                LogLevel.Error, filename, line.LineNumber, line.ColCount - 1));
                            return;
                        }
                        expectedPeriods = 60 * 24 / interval;
                        continue;
                    }

                    // Parse data rows (date in format yyyyMMdd)
                    DateTime? readDate = line.GetDate(0, "yyyyMMdd");
                    if (readDate != null && inChannelBlock)
                    {
                        // Get or create the SiteDay
                        var siteDay = result.SitesDays.FirstOrDefault(x => x.SiteCode == nmi && x.Date == readDate.Value);
                        if (siteDay == null)
                        {
                            siteDay = new SiteDay()
                            {
                                SiteCode = nmi,
                                Date = readDate.Value,
                                Channels = new Dictionary<string, ChannelDay>(),
                                TimeZoneName = localTime,
                                UCT_Offset = (localTime == "AEST" ? 10 : null),
                            };
                            result.SitesDays.Add(siteDay);
                        }

                        // Create the ChannelDay
                        ChannelDay channelDay = new()
                        {
                            Channel = currentChannel,
                            IntervalMinutes = interval,
                            Readings = new decimal[expectedPeriods],
                            TimeStampUtc = DateTime.UtcNow,
                            MeterId = currentMeter,
                            RegisterId = currentChannel,
                            SourceFile = filename,
                        };

                        // Add or replace channel
                        if (siteDay.Channels.ContainsKey(currentChannel) == false)
                        {
                            siteDay.Channels.Add(currentChannel, channelDay);
                        }
                        else
                        {
                            result.LogMessages.Add(new FileLogMessage(
                                $"Duplicate channel {currentChannel} for date {readDate.Value:yyyy-MM-dd} - old data will be replaced",
                                LogLevel.Warning, filename, line.LineNumber, 0));
                            siteDay.Channels[currentChannel] = channelDay;
                        }

                        // Get quality (second to last column)
                        string quality = line.GetStringUpper(expectedPeriods + 1);

                        // Update UOM and channel type
                        var uomResult = CsvParserLib.UpdateUom(channelDay, currentUom);
                        if (!uomResult.isValid)
                        {
                            result.LogMessages.Add(new FileLogMessage(
                                $"Invalid UOM {currentUom}",
                                LogLevel.Warning, filename, line.LineNumber, 4));
                        }

                        // Update from AEMO channel naming convention
                        CsvParserLib.UpdateFromAemoChannel(channelDay);

                        // Override channel type based on the type field
                        // For Solar/Generation, keep values positive (same convention as NEM12)
                        decimal multiplier = uomResult.mult;
                        if (currentType.Contains("SOLAR") || currentType.Contains("GENERATION"))
                        {
                            channelDay.ChannelType = ChanelType.ActiveEnergyGeneration;
                        }
                        else if (currentType.Contains("CONSUMPTION"))
                        {
                            channelDay.ChannelType = ChanelType.ActiveEnergyConsumption;
                        }

                        // Parse readings
                        for (int col = 1; col <= expectedPeriods; col++)
                        {
                            var read = line.GetDecimalCol(col);
                            if (read == null)
                            {
                                if (!string.IsNullOrWhiteSpace(line.GetString(col)))
                                {
                                    result.LogMessages.Add(new FileLogMessage(
                                        $"Invalid decimal {line.GetString(col)}",
                                        LogLevel.Error, filename, line.LineNumber, col));
                                }
                                read = 0;
                            }
                            channelDay.Readings[col - 1] = read.Value * multiplier;
                        }

                        channelDay.Total = channelDay.Readings.Sum();
                        continue;
                    }

                    // Check for end of channel block
                    if (line.GetStringUpper(0).Contains("TOTAL FOR PERIOD"))
                    {
                        inChannelBlock = false;
                        // Reset channel-specific variables
                        currentMeter = string.Empty;
                        currentChannel = string.Empty;
                        currentUom = string.Empty;
                        currentType = string.Empty;
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                result.LogMessages.Add(new FileLogMessage(
                    ex.Message,
                    LogLevel.Critical, filename, reader.LineNumber, 0));
            }

            return;
        }
    }
}
