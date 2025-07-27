using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MeterDataLib.Parsers
{
    public class Nem12 : IParser
    {
 

        private CsvLine CsvLine { get; set; } = new CsvLine([], 0, true);
        string IParser.Name => "NEM12";

        const int MAX_ERRORS = 10;
        const int MAX_WARNINGS = 100;
        
        
    
        async Task IParser.Parse(SimpleCsvReader csvReader, ParserResult result, Func<ParserResult, Task>? callBack, CancellationToken? cancellationToken)
        {

            CsvLine? last100 = null;
            CsvLine? last200 = null;
            CsvLine? last300 = null;
            SiteDay? lastSiteDay = null;
            ChannelDay? lastChannelDay = null;
            string? sourceMDP = null;
            string? nmi = null;
            string? meterSerial = null;
            string? channel = null;
            string? registerId = null;
            string? uom = null;
            int intervalLength = 0;
            UnitOfMeasure unitOfMeasure = UnitOfMeasure.Other;
            string? OriginalUnitOfMeasureSymbol = null;
            UnitOfMeasure? OriginalUnitOfMeasure = null;
            decimal uomConversionFactor = 1;
            bool skip300 = false;
            int[] validIntervalLengths = [5, 15, 30, 60];

            bool negativeReadsDetected = false;


            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            while (true)
            {
                cancellationToken?.ThrowIfCancellationRequested();
                var line =  await csvReader.ReadAsync();
                if (line.Eof)
                {
                    result.Progress = "Loading completed";
                    break;
                }



                if ( timer.ElapsedMilliseconds > 100)
                {
                    result.Progress = $"reading line {line.LineNumber}";
                    timer.Restart();
                    if (callBack != null)
                    {
                        await callBack(result);
                    }
                }

                int recordType = line.GetIntCol(0) ?? 0;
                switch (recordType)
                {
                    case 100:
                        skip300 = false;
                        last100 = line;
                        lastChannelDay = null;
                        last300 = null;
                        lastSiteDay = null;

                        if (line.GetStringUpper(1) != "NEM12")
                        {
                            result.LogMessages.Add(new FileLogMessage($"Invalid File type {line.GetStringUpper(1)} - File cannot be loaded ", LogLevel.Error, csvReader.Filename, csvReader.LineNumber, 1));
                            return; 
                        }
                        sourceMDP = line.GetStringUpper(3);

                        break;
                    case 200:
                        skip300 = false;
                        last200 = line;
                        lastChannelDay = null;
                        last300 = null;
                        lastSiteDay = null;

                        nmi = line.GetStringUpper(1);
                        registerId = line.GetStringUpper(3);
                        meterSerial = line.GetStringUpper(6);
                        channel = line.GetStringUpper(4);
                        uom = line.GetStringUpper(7);
                        intervalLength = line.GetIntCol(8) ?? 0;

                        // validate Errors 
                        if (string.IsNullOrWhiteSpace(nmi))
                        {
                            result.LogMessages.Add(new FileLogMessage($"Missing NMI - 200 Block will be ignored", LogLevel.Error, csvReader.Filename, csvReader.LineNumber, 1));
                            skip300 = true;
                            break;
                        }
                        if (string.IsNullOrWhiteSpace(channel))
                        {
                            result.LogMessages.Add(new FileLogMessage($"Missing channel - 200 Block will be ignored", LogLevel.Error, csvReader.Filename, csvReader.LineNumber, 4));
                            skip300 = true;
                            break;
                        }




                        if (!validIntervalLengths.Contains(intervalLength))
                        {
                            result.LogMessages.Add(new FileLogMessage($"Interval length  {intervalLength}  is invalid - expected : 5,15,30,60  200 block will be ignored", LogLevel.Error, csvReader.Filename, csvReader.LineNumber, 8));
                            skip300 = true;
                            break;
                        }

                        if (string.IsNullOrWhiteSpace(uom))
                        {
                            result.LogMessages.Add(new FileLogMessage($"Missing UOM - 200 Block will be ignored", LogLevel.Error, csvReader.Filename, csvReader.LineNumber, 7));
                            skip300 = true;
                            break;
                        }

                        uom ??= string.Empty;
                        var uomEnum = UnitOfMeasureExtensions.ToUom(uom);
                        if (uomEnum == UnitOfMeasure.Other)
                        {
                            result.LogMessages.Add(new FileLogMessage($"Invalid UOM {uom}  ", LogLevel.Warning, csvReader.Filename, last200.LineNumber, 7));
                            unitOfMeasure = uomEnum;
                            OriginalUnitOfMeasureSymbol = uom;
                            OriginalUnitOfMeasure = UnitOfMeasure.Other;
                            uomConversionFactor = 1;
                        }
                        else
                        {

                            var (stdUnitOfMeasure, stdConversionFactor) = uomEnum.ToStandardUnit();
                            if (stdUnitOfMeasure != uomEnum || stdConversionFactor != 1m)
                            {
                                unitOfMeasure = stdUnitOfMeasure;
                                uomConversionFactor = stdConversionFactor;
                                OriginalUnitOfMeasureSymbol = uom;
                                OriginalUnitOfMeasure = uomEnum;
                            }
                            else
                            {

                                unitOfMeasure = stdUnitOfMeasure;
                                uomConversionFactor = 1m;
                                OriginalUnitOfMeasureSymbol = null;
                                OriginalUnitOfMeasure = null;


                            }



                        }









                        // soft warnings - should not occur but can still process file 
                        if (string.IsNullOrWhiteSpace(registerId))
                        {
                            result.LogMessages.Add(new FileLogMessage($"Missing Register ID", LogLevel.Warning, csvReader.Filename, csvReader.LineNumber, 3));
                            break;
                        }

                        if (string.IsNullOrWhiteSpace(meterSerial))
                        {
                            result.LogMessages.Add(new FileLogMessage($"Missing meterSerial ID", LogLevel.Warning, csvReader.Filename, csvReader.LineNumber, 6));
                            break;
                        }

                        break;
                    case 300:
                        if (skip300 || string.IsNullOrWhiteSpace(nmi))
                        {
                            break;
                        }
                        {
                            last300 = line;
                            var dateOfRead = line.GetDate(1, "yyyyMMdd");
                            if (dateOfRead == null)
                            {
                                result.LogMessages.Add(new FileLogMessage($"Invalid Date {line.Columns[1]} - line is ignored", LogLevel.Warning, csvReader.Filename, csvReader.LineNumber, 1));
                                break;
                            }

                            lastSiteDay = result.SitesDays.Where(zz => zz.SiteCode == nmi && zz.Date == dateOfRead).FirstOrDefault();
                            nmi ??= string.Empty;
                            if (lastSiteDay == null)
                            {
                                lastSiteDay = new SiteDay() { SiteCode = nmi, Date = dateOfRead.Value, TimeZoneName = "E. Australia Standard Time", UCT_Offset = 10 };
                                result.SitesDays.Add(lastSiteDay);
                            }
                            channel ??= string.Empty;

                            int expectedReadings = 60 * 24 / intervalLength;
                            // determine the actual reads 
                            int actualIntervalLength = line.ColCount > 60 * 24 / 5 ? 5 : line.ColCount > 60 * 24 / 15 ? 15 : line.ColCount > 60 * 24 / 30 ? 30 : line.ColCount > 24 ? 60 : 0;
                            if (actualIntervalLength == 0)
                            {
                                result.LogMessages.Add(new FileLogMessage($"Invalid number of readings {line.ColCount} - line is ignored", LogLevel.Error, csvReader.Filename, csvReader.LineNumber, 2));
                                break;
                            }
                            bool invalidIntervalWarningIssued = false;
                            if (actualIntervalLength != intervalLength)
                            {
                                if (!invalidIntervalWarningIssued)
                                {
                                    invalidIntervalWarningIssued = true;
                                    result.LogMessages.Add(new FileLogMessage($"Interval does not match actual number of readings. Actual={actualIntervalLength} Expected={intervalLength}  - Interval will be adjusted", LogLevel.Warning, csvReader.Filename, csvReader.LineNumber, 2));
                                }
                                intervalLength = actualIntervalLength;
                                expectedReadings = 60 * 24 / intervalLength;
                            }



                            lastChannelDay = new ChannelDay()
                            {
                                Channel = channel,
                                SourceFile = csvReader.Filename,
                                ChannelNumber = channel.Substring(1, 1),
                                Ignore = false,
                                IntervalMinutes = intervalLength,
                                RegisterId = registerId ?? string.Empty,
                                MeterId = meterSerial ?? string.Empty,
                                UnitOfMeasure = unitOfMeasure,
                                OriginalUnitOfMeasureSymbol = OriginalUnitOfMeasureSymbol,
                                OriginalUnitOfMeasure = OriginalUnitOfMeasure,
                                SourceProviderCode = sourceMDP ?? string.Empty,
                                Readings = new decimal[expectedReadings],
                                Controlled = false
                            };

                            // load the data 
                            bool errorInReads = false;




                            for (int i = 0; i < expectedReadings; i++)
                            {
                                var reading = line.GetDecimalCol(2 + i);
                                if (reading == null)
                                {
                                    result.LogMessages.Add(new FileLogMessage($"Invalid read value {line.GetStringUpper(2 + i)} for site {nmi} date {dateOfRead:yyyy-MM-dd}  day will be skipped", LogLevel.Error, csvReader.Filename, csvReader.LineNumber, 2 + i));
                                    errorInReads = true;
                                    break;
                                }
                                lastChannelDay.Readings[i] = reading.Value * uomConversionFactor;
                            }
                            if (errorInReads)
                            {
                                break;
                            }

                            CheckForNegativeReads(csvReader, result, lastChannelDay, nmi, channel, ref negativeReadsDetected, dateOfRead);

                            var qualityMethod = line.GetStringUpper(1 + expectedReadings + 1);
                            string quality = qualityMethod.Length > 0 ? qualityMethod.Substring(0, 1) : string.Empty;
                            var reasonCode = line.GetIntCol(1 + expectedReadings + 2);
                            var reasonDesc = line.GetString(1 + expectedReadings + 3);
                            if (quality.Length > 1)
                            {
                                string estimationType = qualityMethod.Substring(1);
                                //lastChannelDay.Metadata.Add("EstimationType", estimationType);
                                lastChannelDay.Metadata.Add(new MeterDataInfo(MetaDataName.EstimationType, estimationType));

                            }
                            var qualityCode = QualityExtensions.ToQuality(quality);
                            if (qualityCode == Quality.Unknown)
                            {
                                result.LogMessages.Add(new FileLogMessage($"Invalid Quality Code {quality} for site {nmi} date {dateOfRead:yyyy-MM-dd}  - quality will be ignored ", LogLevel.Warning, csvReader.Filename, csvReader.LineNumber, 1 + expectedReadings + 1));
                                break;
                            }
                            else if (qualityCode == Quality.Variable)
                            {
                                lastChannelDay.ReadQualities = new Quality[expectedReadings];
                            }
                            else
                            {
                                lastChannelDay.OverallQuality = qualityCode;
                            }

                            if (reasonCode != null && reasonCode.Value > 0)
                            {
                                //lastChannelDay.Metadata.Add("ReasonCode", reasonCode.Value.ToString());
                                lastChannelDay.Metadata.Add(new MeterDataInfo(MetaDataName.ReasonCode, reasonCode.Value));
                            }

                            if (!string.IsNullOrWhiteSpace(reasonDesc))
                            {
                                //lastChannelDay.Metadata.Add("ReasonDesc", reasonDesc);
                                lastChannelDay.Metadata.Add(new MeterDataInfo(MetaDataName.ReasonDescription, reasonDesc));
                            }


                            DateTime updateDateTime = line.GetDate(1 + expectedReadings + 4, "yyyyMMddHHmmss") ?? dateOfRead ?? new DateTime(1901, 1, 1);

                            lastChannelDay.TimeStampUtc = updateDateTime.AddHours(-10);
                            lastChannelDay.OverallQuality = qualityCode;
                            lastChannelDay.Total = lastChannelDay.Readings.Sum();


                            CsvParserLib.UpdateFromAemoChannel(lastChannelDay);

                            //test for duplicate channel
                            if (lastSiteDay.Channels.ContainsKey(channel))
                            {
                                var prevChannel = lastSiteDay.Channels[channel];
                                if (prevChannel.Total != lastChannelDay.Total)
                                {
                                    result.LogMessages.Add(new FileLogMessage($"Duplicate Channel {channel} for site {nmi} date {dateOfRead:yyyy-MM-dd} " +
                                        $" Original Total Was {prevChannel.Total} , New Total was {lastChannelDay.Total} " +
                                        $"- original entries will be replaced", LogLevel.Warning, csvReader.Filename, csvReader.LineNumber, 0));
                                }
                                lastSiteDay.Channels.Remove(channel);
                            }

                            lastSiteDay.Channels.Add(channel, lastChannelDay);

                        }


                        break;
                    case 400:
                        if (skip300 || lastChannelDay == null)
                        {
                            break;
                        }
                        {
                            var startInterval = line.GetIntCol(1);
                            var endInterval = line.GetIntCol(2);
                            var qualityMethod = line.GetStringUpper(3);
                            string quality = qualityMethod.Length > 0 ? qualityMethod.Substring(0, 1) : string.Empty;
                            var qualityCode = QualityExtensions.ToQuality(quality);
                            if (qualityCode == Quality.Unknown)
                            {
                                result.LogMessages.Add(new FileLogMessage($"Invalid Quality Code {quality} for site {nmi} date {lastSiteDay?.Date:yyyy-MM-dd}  - quality will be ignored ", LogLevel.Warning, csvReader.Filename, csvReader.LineNumber, 3));
                                break;
                            }
                            if ((int)qualityCode > (int)lastChannelDay.OverallQuality)
                            {
                                lastChannelDay.OverallQuality = qualityCode;
                            }

                            var reasonCode = line.GetIntCol(4);
                            var reasonDesc = line.GetString(5);

                            if (startInterval == null || endInterval == null || startInterval < 1 || endInterval < 1 || startInterval > endInterval || endInterval > 60 * 24 / intervalLength)
                            {
                                result.LogMessages.Add(new FileLogMessage($"Invalid Interval {startInterval} - {endInterval} for site {nmi} date {lastSiteDay?.Date:yyyy-MM-dd}  - line will be ignored ", LogLevel.Warning, csvReader.Filename, csvReader.LineNumber, 1));
                                break;
                            }
                            if (lastChannelDay.ReadQualities == null)
                            {
                                lastChannelDay.ReadQualities = new Quality[lastChannelDay.Readings.Length];
                            }

                            for (int i = startInterval.Value - 1; i < endInterval.Value; i++)
                            {
                                if (i < lastChannelDay.Readings.Length)
                                {
                                    lastChannelDay.ReadQualities[i] = qualityCode;

                                }
                            }


                            if (reasonCode != null)
                            {

                                lastChannelDay.Metadata.Add(new MeterDataInfo(MetaDataName.ReasonCode, startInterval.Value - 1, endInterval.Value - 1, reasonCode.Value.ToString()));
                            }
                            if (string.IsNullOrWhiteSpace(reasonDesc) == false)
                            {

                                lastChannelDay.Metadata.Add(new MeterDataInfo(MetaDataName.ReasonDescription, startInterval.Value - 1, endInterval.Value - 1, reasonDesc));
                            }



                        }
                        break;
                    case 500:
                        if (skip300 || lastSiteDay == null || lastChannelDay == null)
                        {
                            break;
                        }
                        {
                            string transCode = line.GetStringUpper(1);
                            string serviceOrder = line.GetStringUpper(2);
                            DateTime? indexReadDateTime = line.GetDate(3, "yyyyMMddHHmmss");
                            string indexRead = line.GetStringUpper(4);

                            if (!string.IsNullOrEmpty(transCode))
                            {
                                lastChannelDay?.Metadata.Add(new MeterDataInfo(MetaDataName.TransactionCode, transCode));
                            }

                            if (!string.IsNullOrEmpty(serviceOrder))
                            {
                                lastChannelDay?.Metadata.Add(new MeterDataInfo(MetaDataName.ServiceOrder, serviceOrder));
                            }

                            if (indexReadDateTime != null && !string.IsNullOrEmpty(indexRead))
                            {
                                IndexRead read = new() { Quality = lastChannelDay!.OverallQuality, Multiplier = 1, TimeStamp = indexReadDateTime.Value };
                                if (read.SetFromString(indexRead))
                                {
                                    if (lastSiteDay.Date.AddHours(12) > indexReadDateTime.Value)
                                    {
                                        lastChannelDay.StartIndexRead = read;
                                    }
                                    else
                                    {
                                        lastChannelDay.EndIndexRead = read;
                                    }
                                }
                                else
                                {
                                    result.LogMessages.Add(new FileLogMessage($"Invalid Index Read {indexRead} for site {nmi} date {lastSiteDay?.Date:yyyy-MM-dd}  - line will be ignored ", LogLevel.Warning, csvReader.Filename, csvReader.LineNumber, 4));
                                    break;
                                }
                            }

                        }
                        break;
                    case 900:
                        skip300 = false;
                        lastChannelDay = null;
                        last300 = null;
                        lastSiteDay = null;

                        break;
                    default:
                        result.LogMessages.Add(new FileLogMessage($"Invalid Record Type {recordType} - line is ignored", LogLevel.Warning, csvReader.Filename, csvReader.LineNumber, 0));
                        break;

                }

                if (result.Errors > MAX_ERRORS)
                {
                    throw new Exception("Too many errors");
                }
                if (result.Warnings > MAX_WARNINGS)
                {
                    throw new Exception("Too many warnings ");
                }

            }
        }

        private static void CheckForNegativeReads(SimpleCsvReader csvReader, ParserResult result, ChannelDay lastChannelDay, string? nmi, string channel, ref bool negativeReadsDetected, DateTime? dateOfRead)
        {
            // check for negative reads
            if (lastChannelDay.Readings.Any(r => r < 0))
            {
                // check if all the reads are negative or ZERO 
                if (lastChannelDay.Readings.All(r => r <= 0))
                {
                    // for certain channels we can ignore this 
                    // test if the channel prefix is one of the known negative channels : B K 
                    if (channel.Length > 0 && (channel[0] == 'B' || channel[0] == 'K'))
                    {
                        if (!negativeReadsDetected)
                        {
                            result.LogMessages.Add(new FileLogMessage($"Negative read values detected for site {nmi} date {dateOfRead:yyyy-MM-dd} on line {csvReader.LineNumber}- negative reads will be accepted but are not valid NEM12. For this channel [{channel}] they will be converted to positive as this a 'import' channel.", LogLevel.Warning, csvReader.Filename, csvReader.LineNumber, 2));
                            negativeReadsDetected = true;
                        }
                        lastChannelDay.Readings = lastChannelDay.Readings.Select(r => r < 0 ? -r : r).ToArray();
                    }
                    else
                    {
                        if (!negativeReadsDetected)
                        {
                            result.LogMessages.Add(new FileLogMessage($"Negative read values detected for site {nmi} date {dateOfRead:yyyy-MM-dd} on line {csvReader.LineNumber} - negative reads will be accepted but are not valid NEM12. Future negatives will not be displayed in this log.", LogLevel.Warning, csvReader.Filename, csvReader.LineNumber, 2));
                            negativeReadsDetected = true;
                        }
                    }
                }
                else
                {
                    if (!negativeReadsDetected)
                    {
                        result.LogMessages.Add(new FileLogMessage($"Negative read values detected for site {nmi} date {dateOfRead:yyyy-MM-dd} on line {csvReader.LineNumber} - negative reads will be accepted but are not valid NEM12. There is a mix of negative and positive reads. Future negatives will not be displayed in this log.", LogLevel.Warning, csvReader.Filename, csvReader.LineNumber, 2));
                        negativeReadsDetected = true;
                    }
                }
            }
}
        bool IParser.CanParse( List<CsvLine> lines )
        {
            if (lines.Count < 2)
            {
                return false;
            }
            var line = lines.First();
            if (line.Eof)
            {
                return false;
            }
            if (line.GetIntCol(0) == 100)
            {
                if (line.GetStringUpper(1) == "NEM12")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (line.GetIntCol(0) == 200)
            {
                if (line.ColCount < 9)
                {
                    return false;
                }
                if (line.GetStringUpper(1) == string.Empty || line.GetIntCol(8) == null || line.GetStringUpper(4) == string.Empty)
                {
                    return false;
                }
                var line2 = lines.Skip(1).First();
                if (line2.Eof || line2.GetIntCol(0) != 300 || line2.GetDate(1, "yyyyMMdd") == null || line2.ColCount < 48 + 1)
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }



}
