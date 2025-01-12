using Microsoft.Extensions.Logging;
using MeterDataLib;


namespace MeterDataLib.Parsers
{


    public class CsvRedEnergyFormat : IParser
    {
        public string Name => "Csv Red Energy";

        private static readonly int[] AllowedIntervals = [1, 5, 15, 30, 60];

        public bool CanParse(List<CsvLine> lines)
        {
            if (lines.Count < 2) return false;

            const string hdrCheck = "NMI        ,INTERVAL_DATETIME   ,INTERVAL ,INTERVAL_LENGTH ,SERIAL  ,QUALITY_METHOD ,SUFFIX_E ,VALUE_E ,UOM_E ,SUFFIX_K ,VALUE_K ,UOM_K ,SUFFIX_Q ,VALUE_Q ,UOM_Q ,SUFFIX_B ,VALUE_B ,UOM_B";
            string[] hdrArray = hdrCheck.Split(',', StringSplitOptions.TrimEntries);

            // CHECK HEADER ROW
            if (lines[0].ColCount != hdrArray.Length) return false;
            for (int i = 0; i < hdrArray.Length; i++)
            {
                if (lines[0].GetStringUpper(i) != hdrArray[i]) return false;
            }
            return true;
        }
        async Task IParser.Parse(SimpleCsvReader reader, ParserResult result, Func<ParserResult, Task>? callBack, CancellationToken? cancellationToken)
        {
            string filename = reader.Filename;
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            var records = new List<DataLine>();
            try
            {


                {
                    CsvLine line;
                    // skip the first line 
                    await reader.ReadAsync();

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

                            //NMI        ,MM   ,INTERVAL ,INTERVAL_LENGTH ,SERIAL  ,QUALITY_METHOD ,SUFFIX_E ,VALUE_E ,UOM_E ,SUFFIX_K ,VALUE_K ,UOM_K ,SUFFIX_Q ,VALUE_Q ,UOM_Q ,SUFFIX_B ,VALUE_B ,UOM_B
                            string nmi = line.GetStringUpperMandatory(0);
                            DateTime localTime = line.GetDateMandatory(1, ["dd/MM/yyyy HH:mm:ss"]);
                            int period = line.GetIntMandatory(2, 1, 24 * 60);  // period can be 1 to 1440 minutes
                            int intervalDurationInMinutes = line.GetIntMandatory(3, 1, 60);  // interval length can be 1 to 60 minutes
                            string serial = line.GetStringUpperMandatory(4);
                            string qualityMethod = line.GetStringUpperMandatory(5);
                            string suffixE = line.GetString(6);
                            decimal e = line.GetDecimalCol(7) ?? 0;
                            string uomE = line.GetString(8);
                            string suffixK = line.GetString(9);
                            decimal k = line.GetDecimalCol(10) ?? 0;
                            string uomK = line.GetString(11);
                            string suffixQ = line.GetString(12);
                            decimal q = line.GetDecimalCol(13) ?? 0;
                            string uomQ = line.GetString(14);
                            string suffixB = line.GetString(15);
                            decimal b = line.GetDecimalCol(16) ?? 0;
                            string uomB = line.GetString(17);

                            if (!AllowedIntervals.Contains(intervalDurationInMinutes))
                            {
                                result.LogMessages.Add(new FileLogMessage($"Invalid interval length [{intervalDurationInMinutes}] between reads found ", Microsoft.Extensions.Logging.LogLevel.Error, filename, 0, 2));
                                continue;
                            }

                            if (!string.IsNullOrWhiteSpace(suffixE))
                            {

                                var record = new CsvRedEnergyFormat.DataLine
                               (
                               Nmi: nmi,
                               IntervalEndTime: localTime,
                               Period: period,
                               IntervalDurationInMinutes: intervalDurationInMinutes,
                               Serial: serial,
                               QualityMethod: qualityMethod,
                               Suffix: suffixE,
                               Value: e,
                               Uom: uomE,
                               LineNumber: line.LineNumber
                               );
                                records.Add(record);
                            }
                            if (!string.IsNullOrWhiteSpace(suffixK))
                            {
                                var record = new CsvRedEnergyFormat.DataLine
                               (
                               Nmi: nmi,
                               IntervalEndTime: localTime,
                               Period: period,
                               IntervalDurationInMinutes: intervalDurationInMinutes,
                               Serial: serial,
                               QualityMethod: qualityMethod,
                               Suffix: suffixK,
                               Value: k,
                               Uom: uomK,
                               LineNumber: line.LineNumber
                               );
                                records.Add(record);
                            }
                            if (!string.IsNullOrWhiteSpace(suffixQ))
                            {
                                var record = new CsvRedEnergyFormat.DataLine
                               (
                               Nmi: nmi,
                               IntervalEndTime: localTime,
                               Period: period,
                               IntervalDurationInMinutes: intervalDurationInMinutes,
                               Serial: serial,
                               QualityMethod: qualityMethod,
                               Suffix: suffixQ,
                               Value: q,
                               Uom: uomQ,
                               LineNumber: line.LineNumber
                               );
                                records.Add(record);
                            }
                            if (!string.IsNullOrWhiteSpace(suffixB))
                            {

                                var record = new CsvRedEnergyFormat.DataLine
                               (
                               Nmi: nmi,
                               IntervalEndTime: localTime,
                               Period: period,
                               IntervalDurationInMinutes: intervalDurationInMinutes,
                               Serial: serial,
                               QualityMethod: qualityMethod,
                               Suffix: suffixB,
                               Value: b,
                               Uom: uomB,
                               LineNumber: line.LineNumber
                               );
                                records.Add(record);
                            }


                        }
                        catch (Exception ex)
                        {
                            result.LogMessages.Add(new FileLogMessage(ex.Message, Microsoft.Extensions.Logging.LogLevel.Critical, filename, reader.LineNumber, 0));
                        }
                    }
                }


                var list = records.GroupBy(x => new { x.Nmi, x.ReadDate })
                                  .Where(x => x.Count() > 1)
                                  .OrderBy(x => x.Key.Nmi)
                                  .ThenBy(x => x.Key.ReadDate)
                                  .ToList();

                int counterTotal = list.Count;
                int counter = 0;

                foreach (var siteDayGroup in list)
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    counter++;
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

                    // Transform the data into a dictionary of channels 
                    var byChannel = siteDayGroup.GroupBy(x => x.Suffix);
                    foreach (var channelSet in byChannel)
                    {

                        int intervalDuration = channelSet.First().IntervalDurationInMinutes;
                        string serial = channelSet.First().Serial;
                        int expectedPeriods = 60 * 24 / intervalDuration;


                        ChannelDay chlDy = new()
                        {
                            Channel = channelSet.Key,
                            IntervalMinutes = intervalDuration,
                            Readings = new decimal[expectedPeriods],
                            TimeStampUtc = DateTime.UtcNow,
                            MeterId = serial,
                            RegisterId = channelSet.Key,
                            SourceFile = filename,
                            Total = 0,
                            IsAvg = false,
                            IsNet = false,
                            IsCheck = false
                        };
                        siteDay.Channels.Add(chlDy.Channel, chlDy);
                        UnitOfMeasure uom = channelSet.First().Uom.ToUom();
                        (UnitOfMeasure stdUom, decimal mult) = uom.ToStandardUnit();
                        chlDy.UnitOfMeasure = stdUom;
                        chlDy.OriginalUnitOfMeasure = uom;
                        chlDy.OriginalUnitOfMeasureSymbol = channelSet.First().Uom;
                        chlDy.Ignore = false;
                        CsvParserLib.UpdateFromAemoChannel(chlDy);

                        var qualityCodes = channelSet.Select(x => x.QualityMethod).Distinct().ToList();
                        if (qualityCodes.Count == 1)
                        {
                            var qualityCode = qualityCodes.First();
                            CsvParserLib.UpdateQuality(chlDy, qualityCode);
                            chlDy.ReadQualities = null;
                        }
                        else
                        {
                            chlDy.OverallQuality = Quality.Variable;
                            chlDy.ReadQualities = new Quality[expectedPeriods];
                        }

                        foreach (var dataPoint in channelSet)
                        {
                            int index = dataPoint.Period - 1;
                            DateTime expectedStartTime = siteDay.Date.AddMinutes(index * intervalDuration);
                            DateTime recordedStartTime = dataPoint.StartTime;
                            if (expectedStartTime != recordedStartTime)
                            {
                                result.LogMessages.Add(new FileLogMessage($"The interval period {dataPoint.Period} indicates a start time {expectedStartTime:HH:mm:ss} but found {recordedStartTime:HH:mm:ss} ", LogLevel.Error, filename, dataPoint.LineNumber));
                                continue;
                            }
                            if (index < 0 || index >= chlDy.Readings.Length)
                            {
                                result.LogMessages.Add(new FileLogMessage($"Invalid index period found  {index} ", LogLevel.Error, filename, dataPoint.LineNumber));
                                continue;
                            }
                            chlDy.Readings[index] = dataPoint.Value * mult;
                            if (chlDy.OverallQuality == Quality.Variable && chlDy.ReadQualities != null)
                            {
                                string qualityMethod = dataPoint.QualityMethod.Trim();
                                if (qualityMethod.Length == 0) { qualityMethod = "A"; }
                                string quality = qualityMethod[0].ToString().ToUpper();
                                chlDy.ReadQualities[index] = quality.ToQuality();
                                if (qualityMethod.Length > 1)
                                {
                                    string estimationType = qualityMethod.Substring(1);
                                    //lastChannelDay.Metadata.Add("EstimationType", estimationType);
                                    chlDy.Metadata.Add(new MeterDataInfo(MetaDataName.EstimationType, index, index, estimationType));
                                }
                            }

                        }
                        chlDy.Total = chlDy.Readings.Sum();
                    }







                }
            }
            catch (Exception ex)
            {
                result.LogMessages.Add(new FileLogMessage(ex.Message, Microsoft.Extensions.Logging.LogLevel.Critical, filename, 0, 0));
            }

            return;
        }

        record DataLine(string Nmi, DateTime IntervalEndTime, int Period, int IntervalDurationInMinutes,
                      string Serial,
                      string QualityMethod,
                      string Suffix, decimal Value, string Uom,
                      int LineNumber)
        {
            public DateTime StartTime => IntervalEndTime.AddMinutes(-1 * IntervalDurationInMinutes);
            public DateTime ReadDate => StartTime.Date;
        }



    }
}
