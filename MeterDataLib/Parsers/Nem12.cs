using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
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
        
        //public override ParserResult Parse(Stream stream, string filename)
        //{
        //    stream.Seek(0, SeekOrigin.Begin);
        //    var result = new ParserResult();
        //    result.FileName = filename;
        //    result.ParserName = Name;

 

        //    using var csvReader = new SimpleCsvReader(stream, filename);
        //    try
        //    {
        //        while (true)
        //        {
        //            var line =  bufferLine ?? csvReader.Read();
        //            bufferLine = null; 
        //            if (line.Eof)
        //            {
        //                break;
        //            }
        //            int recordType = line.GetIntCol(0) ?? 0;
        //            switch (recordType)
        //            {
        //                case 100:
        //                    skip300 = false;
        //                    last100 = line;
        //                    lastChannelDay = null;
        //                    last300 = null;
        //                    lastSiteDay = null;

        //                    if (line.GetStringUpper(1) != "NEM12")
        //                    {
        //                        result.LogMessages.Add(new FileLogMessage($"Invalid File type {line.GetStringUpper(1)} - File cannot be loaded ", LogLevel.Error, filename, csvReader.LineNumber, 1));
        //                        throw new Exception("Invalid File type in 100 Line");
        //                    }
        //                    sourceMDP = line.GetStringUpper(3);

        //                    break;
        //                case 200:
        //                    skip300 = false;
        //                    last200 = line;
        //                    lastChannelDay = null;
        //                    last300 = null; 
        //                    lastSiteDay = null;

        //                    nmi = line.GetStringUpper(1);
        //                    registerId = line.GetStringUpper(3);
        //                    meterSerial = line.GetStringUpper(6);
        //                    channel = line.GetStringUpper(4);
        //                    uom = line.GetStringUpper(7);
        //                    intervalLength = line.GetIntCol(8) ?? 0;

        //                    // validate Errors 
        //                    if (string.IsNullOrWhiteSpace(nmi))  
        //                    {
        //                        result.LogMessages.Add(new FileLogMessage($"Missing NMI - 200 Block will be ignored", LogLevel.Error, filename, csvReader.LineNumber, 1));
        //                        skip300 =  true; 
        //                        break;
        //                    }
        //                    if (string.IsNullOrWhiteSpace(channel))
        //                    {
        //                        result.LogMessages.Add(new FileLogMessage($"Missing channel - 200 Block will be ignored", LogLevel.Error, filename, csvReader.LineNumber, 4));
        //                        skip300 = true;
        //                        break;
        //                    }




        //                    if (!validIntervalLengths.Contains(intervalLength))
        //                    {
        //                        result.LogMessages.Add(new FileLogMessage($"Interval length  {intervalLength}  is invalid - expected : 5,15,30,60  200 block will be ignored", LogLevel.Error, filename, csvReader.LineNumber, 8));
        //                        skip300 = true;
        //                        break;
        //                    }

        //                    if (string.IsNullOrWhiteSpace(uom))
        //                    {
        //                        result.LogMessages.Add(new FileLogMessage($"Missing UOM - 200 Block will be ignored", LogLevel.Error, filename, csvReader.LineNumber, 7));
        //                        skip300 = true;
        //                        break;
        //                    }

        //                    uom ??= string.Empty;
        //                    var uomEnum = UnitOfMeasureExtensions.ToUom(uom);
        //                    if (uomEnum == UnitOfMeasure.Other)
        //                    {
        //                        result.LogMessages.Add(new FileLogMessage($"Invalid UOM {uom}  ", LogLevel.Warning, filename, last200.LineNumber, 7));
        //                        unitOfMeasure = uomEnum;
        //                        OriginalUnitOfMeasureSymbol = uom;
        //                        OriginalUnitOfMeasure =  UnitOfMeasure.Other;
        //                        uomConversionFactor = 1; 
        //                    }
        //                    else
        //                    {

        //                        var (stdUnitOfMeasure, stdConversionFactor) = uomEnum.ToStandardUnit();
        //                        if (stdUnitOfMeasure != uomEnum || stdConversionFactor != 1m)
        //                        {
        //                            unitOfMeasure = stdUnitOfMeasure;
        //                            uomConversionFactor = stdConversionFactor;
        //                            OriginalUnitOfMeasureSymbol = uom;
        //                            OriginalUnitOfMeasure = uomEnum;
        //                        }
        //                        else
        //                        {

        //                            unitOfMeasure = stdUnitOfMeasure;
        //                            uomConversionFactor = 1m;
        //                            OriginalUnitOfMeasureSymbol = null;
        //                            OriginalUnitOfMeasure = null;


        //                        }



        //                    }
                            





                            


        //                    // soft warnings - should not occur but can still process file 
        //                    if (string.IsNullOrWhiteSpace(registerId))
        //                    {
        //                        result.LogMessages.Add(new FileLogMessage($"Missing Register ID", LogLevel.Warning, filename, csvReader.LineNumber, 3));
        //                        break;
        //                    }

        //                    if (string.IsNullOrWhiteSpace(meterSerial))
        //                    {
        //                        result.LogMessages.Add(new FileLogMessage($"Missing meterSerial ID", LogLevel.Warning, filename, csvReader.LineNumber, 6));
        //                        break;
        //                    }

        //                    break;
        //                case 300:
        //                    if ( skip300 || string.IsNullOrWhiteSpace(nmi))
        //                    {
        //                        break;
        //                    }
        //                    {
        //                        last300 = line;
        //                        var dateOfRead = line.GetDate(1, "yyyyMMdd");
        //                        if (dateOfRead == null)
        //                        {
        //                            result.LogMessages.Add(new FileLogMessage($"Invalid Date {line.Columns[1]} - line is ignored", LogLevel.Warning, filename, csvReader.LineNumber, 1));
        //                            break;
        //                        }

        //                        lastSiteDay = result.SitesDays.Where(zz => zz.Site == nmi && zz.Date == dateOfRead).FirstOrDefault();
        //                        nmi ??= string.Empty;
        //                        if (lastSiteDay == null)
        //                        {
        //                            lastSiteDay = new SiteDay() { Site = nmi, Date = dateOfRead.Value, TimeZoneName= "E. Australia Standard Time" , UCT_Offset=10  };
        //                            result.SitesDays.Add(lastSiteDay);
        //                        }
        //                        channel ??= string.Empty;
        //                        if (lastSiteDay.Channels.ContainsKey(channel))
        //                        {
        //                            result.LogMessages.Add(new FileLogMessage($"Duplicate Channel {channel} for site {nmi} date {dateOfRead:yyyy-MM-dd} - original entries will be replaced", LogLevel.Warning, filename, csvReader.LineNumber, 0));
        //                            lastSiteDay.Channels.Remove(channel);
        //                        }
        //                        int expectedReadings = 60 * 24 / intervalLength;

        //                        lastChannelDay = new ChannelDay()
        //                        {
        //                            Channel = channel,
        //                            SourceFile = filename,
        //                            ChannelNumber = channel.Substring(1, 1),
        //                            Ignore = false,
        //                            IntervalMinutes = intervalLength,
        //                            RegisterId = registerId ?? string.Empty                                    ,
        //                            MeterId = meterSerial ?? string.Empty                                    ,
        //                            UnitOfMeasure = unitOfMeasure                                    ,
        //                            OriginalUnitOfMeasureSymbol = OriginalUnitOfMeasureSymbol                                    ,
        //                            OriginalUnitOfMeasure = OriginalUnitOfMeasure                                     ,
        //                            SourceProviderCode = sourceMDP ?? string.Empty                                     ,
        //                            Readings = new decimal[expectedReadings]                                     ,
        //                            Controlled = false
        //                        };

        //                        // load the data 
        //                        bool errorInReads = false;
        //                        for (int i = 0; i < expectedReadings; i++)
        //                        {
        //                            var reading = line.GetDecimalCol(2 + i);
        //                            if (reading == null || reading < 0)
        //                            {
        //                                result.LogMessages.Add(new FileLogMessage($"Invalid read value {line.GetStringUpper(2 + i)} for site {nmi} date {dateOfRead:yyyy-MM-dd}  day will be skipped", LogLevel.Error, filename, csvReader.LineNumber, 2 + i));
        //                                errorInReads = true;
        //                                break;
        //                            }
        //                            lastChannelDay.Readings[i] = reading.Value * uomConversionFactor;
        //                        }
        //                        if (errorInReads)
        //                        {
        //                            break;
        //                        }
        //                        var qualityMethod = line.GetStringUpper(1 + expectedReadings + 1);
        //                        string quality = qualityMethod.Length > 0 ? qualityMethod.Substring(0, 1) : string.Empty;
        //                        var reasonCode = line.GetIntCol(1 + expectedReadings + 2);
        //                        var reasonDesc = line.GetString(1 + expectedReadings + 3);
        //                        if (quality.Length > 1)
        //                        {
        //                            string estimationType = qualityMethod.Substring(1);
        //                            //lastChannelDay.Metadata.Add("EstimationType", estimationType);
        //                            lastChannelDay.Metadata.Add(new MeterDataInfo(MetaDataName.EstimationType, estimationType));
                                    
        //                        }
        //                        var qualityCode = QualityExtensions.ToQuality(quality);
        //                        if (qualityCode == Quality.Unknown)
        //                        {
        //                            result.LogMessages.Add(new FileLogMessage($"Invalid Quality Code {quality} for site {nmi} date {dateOfRead:yyyy-MM-dd}  - quality will be ignored ", LogLevel.Warning, filename, csvReader.LineNumber, 1 + expectedReadings + 1));
        //                            break;
        //                        }
        //                        else if (qualityCode == Quality.Variable)
        //                        {
        //                            lastChannelDay.ReadQualities = new Quality[expectedReadings];
        //                        }
        //                        else
        //                        {
        //                            lastChannelDay.OverallQuality = qualityCode;
        //                        }

        //                        if (reasonCode != null && reasonCode.Value > 0)
        //                        {
        //                            //lastChannelDay.Metadata.Add("ReasonCode", reasonCode.Value.ToString());
        //                            lastChannelDay.Metadata.Add(new MeterDataInfo(MetaDataName.ReasonCode, reasonCode.Value));
        //                        }

        //                        if (!string.IsNullOrWhiteSpace(reasonDesc))
        //                        {
        //                            //lastChannelDay.Metadata.Add("ReasonDesc", reasonDesc);
        //                            lastChannelDay.Metadata.Add(new MeterDataInfo(MetaDataName.ReasonDescription, reasonDesc));
        //                        }
 

        //                        DateTime updateDateTime = line.GetDate(1 + expectedReadings + 4, "yyyyMMddHHmmss") ?? dateOfRead ?? new DateTime(1901, 1, 1);

        //                        lastChannelDay.TimeStampUtc = updateDateTime.AddHours(-10);
        //                        lastChannelDay.OverallQuality = qualityCode;
        //                        lastChannelDay.Total = lastChannelDay.Readings.Sum();

                           
        //                        CsvParserLib.UpdateFromAemoChannel(lastChannelDay);



        //                        lastSiteDay.Channels.Add(channel, lastChannelDay);

        //                    }


        //                    break;
        //                case 400:
        //                    if (skip300 || lastChannelDay == null )
        //                    {
        //                        break;
        //                    }
        //                    {
        //                        var startInterval = line.GetIntCol(1);
        //                        var endInterval = line.GetIntCol(2);
        //                        var qualityMethod = line.GetStringUpper(3);
        //                        string quality = qualityMethod.Length > 0 ? qualityMethod.Substring(0, 1) : string.Empty;
        //                        var qualityCode = QualityExtensions.ToQuality(quality);
        //                        if (qualityCode == Quality.Unknown)
        //                        {
        //                            result.LogMessages.Add(new FileLogMessage($"Invalid Quality Code {quality} for site {nmi} date {lastSiteDay?.Date:yyyy-MM-dd}  - quality will be ignored ", LogLevel.Warning, filename, csvReader.LineNumber, 3));
        //                            break;
        //                        }
        //                        if ( (int)qualityCode > (int)lastChannelDay.OverallQuality)
        //                        {
        //                            lastChannelDay.OverallQuality = qualityCode;
        //                        }
                       
        //                        var reasonCode = line.GetIntCol(4);
        //                        var reasonDesc = line.GetString(5);

        //                        if( startInterval == null || endInterval == null || startInterval < 1 || endInterval < 1 || startInterval > endInterval || endInterval > 60 * 24 / intervalLength)
        //                        {
        //                            result.LogMessages.Add(new FileLogMessage($"Invalid Interval {startInterval} - {endInterval} for site {nmi} date {lastSiteDay?.Date:yyyy-MM-dd}  - line will be ignored ", LogLevel.Warning, filename, csvReader.LineNumber, 1));
        //                            break;
        //                        }
        //                        if ( lastChannelDay.ReadQualities==null )
        //                        {
        //                            lastChannelDay.ReadQualities = new Quality[lastChannelDay.Readings.Length];
        //                        }
                   
        //                        for( int i = startInterval.Value - 1; i < endInterval.Value; i++)
        //                        {
        //                            if (i < lastChannelDay.Readings.Length)
        //                            {
        //                                lastChannelDay.ReadQualities[i] = qualityCode;
                                       
        //                            }
        //                        }


        //                        if (reasonCode != null )
        //                        {

        //                            lastChannelDay.Metadata.Add(new MeterDataInfo(MetaDataName.ReasonCode, startInterval.Value - 1, endInterval.Value - 1, reasonCode.Value.ToString())); 
        //                        }
        //                        if (string.IsNullOrWhiteSpace(reasonDesc) == false ) 
        //                        {

        //                            lastChannelDay.Metadata.Add(new MeterDataInfo(MetaDataName.ReasonDescription, startInterval.Value - 1, endInterval.Value - 1, reasonDesc));
        //                        }



        //                    }
        //                    break;
        //                case 500:
        //                    if (skip300 || lastSiteDay == null || lastChannelDay == null )
        //                    {
        //                        break;
        //                    }
        //                    {
        //                        string transCode = line.GetStringUpper(1);
        //                        string serviceOrder = line.GetStringUpper(2);
        //                        DateTime? indexReadDateTime = line.GetDate(3, "yyyyMMddHHmmss");
        //                        string indexRead = line.GetStringUpper(4);

        //                        if ( ! string.IsNullOrEmpty(transCode))
        //                        {
        //                            lastChannelDay?.Metadata.Add( new MeterDataInfo(MetaDataName.TransactionCode, transCode));
        //                        }

        //                        if (!string.IsNullOrEmpty(serviceOrder))
        //                        {
        //                            lastChannelDay?.Metadata.Add( new MeterDataInfo(MetaDataName.ServiceOrder, serviceOrder));
        //                        }

        //                        if (indexReadDateTime != null && ! string.IsNullOrEmpty(indexRead))
        //                        {
        //                           IndexRead read  = new() { Quality = lastChannelDay!.OverallQuality , Multiplier=1 , TimeStamp = indexReadDateTime.Value };
        //                            if (read.SetFromString(indexRead))
        //                            {
        //                                if ( lastSiteDay.Date.AddHours(12) > indexReadDateTime.Value)
        //                                {
        //                                    lastChannelDay.StartIndexRead = read;
        //                                }
        //                                else
        //                                {
        //                                    lastChannelDay.EndIndexRead = read;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                result.LogMessages.Add(new FileLogMessage($"Invalid Index Read {indexRead} for site {nmi} date {lastSiteDay?.Date:yyyy-MM-dd}  - line will be ignored ", LogLevel.Warning, filename, csvReader.LineNumber, 4));
        //                                break;
        //                            }
        //                        }

        //                    }
        //                    break;
        //                case 900:
        //                    skip300 = false;
        //                    lastChannelDay = null;
        //                    last300 = null;
        //                    lastSiteDay = null;

        //                    break;
        //                default:
        //                    result.LogMessages.Add(new FileLogMessage($"Invalid Record Type {recordType} - line is ignored", LogLevel.Warning, filename, csvReader.LineNumber, 0));
        //                    break;

        //            }

        //            if (result.Errors > MAX_ERRORS)
        //            {
        //                throw new Exception("Too many errors");
        //            }
        //            if (result.Warnings > MAX_WARNINGS)
        //            {
        //                throw new Exception("Too many warnings ");
        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result.LogMessages.Add(new FileLogMessage($"Error processing file {ex.Message}", LogLevel.Critical, filename, csvReader.LineNumber, 0));
        //    }

        //  //  result.Success = result.SitesDays.Any() && result.Errors == 0  ;


        //    return result;


        //}

    
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
            int[] validIntervalLengths = new int[] { 5, 15, 30, 60 };
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
                            if (lastSiteDay.Channels.ContainsKey(channel))
                            {
                                result.LogMessages.Add(new FileLogMessage($"Duplicate Channel {channel} for site {nmi} date {dateOfRead:yyyy-MM-dd} - original entries will be replaced", LogLevel.Warning, csvReader.Filename, csvReader.LineNumber, 0));
                                lastSiteDay.Channels.Remove(channel);
                            }
                            int expectedReadings = 60 * 24 / intervalLength;

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
                                if (reading == null || reading < 0)
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
