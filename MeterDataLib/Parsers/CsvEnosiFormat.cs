using Microsoft.Extensions.Logging;
using MeterDataLib;
using System.Diagnostics.Metrics;
using System.Drawing;


namespace MeterDataLib.Parsers
{


    public class CsvEnosiFormat : IParser
    {
        public string Name => "Csv Enosi Format";

        private static readonly int[] AllowedIntervals = [1, 5, 15, 30, 60];

        public bool CanParse(List<CsvLine> lines)
        {
            if (lines.Count < 2) return false;

            const string hdrCheck = "Property title,Primary billing point identifier,Meter title,Meter identifier,Metric,Start,Finish,Value,Units,Flags,Carbon emissionality,Carbon emissions,Carbon emissions units";
            string[] hdrArray = hdrCheck.Split(',', StringSplitOptions.TrimEntries);

            // CHECK HEADER ROW
            if (lines[0].ColCount != hdrArray.Length) return false;
  
            for (int i = 0; i < hdrArray.Length; i++)
            {
                if (!string.Equals(lines[0].Columns[i], hdrArray[i], StringComparison.InvariantCultureIgnoreCase)) return false;
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

                        //Property title,Primary billing point identifier,Meter title,Meter identifier,Metric,Start,Finish,Value,Units,Flags,Carbon emissionality,Carbon emissions,Carbon emissions units
                        //0              1                                2           3                4      5     6      7     8     9     10                   11               12
                        /* 
                         * NSW AGRI - MOONACRES HARVEST KITCHEN,4310777609,Meter 1,4310777609-1,elec_energy_consumed,2024-12-12 00:00:00,2024-12-12 00:30:00,1314,Wh,AEMO:QUALITY:A,0.73,959.22,g-CO2•e
                        NSW AGRI - MOONACRES HARVEST KITCHEN,4310777609,Meter 1,4310777609-1,elec_energy_consumed,2024-12-12 00:30:00,2024-12-12 01:00:00,1158,Wh,AEMO:QUALITY:A,0.73,845.34,g-CO2•e
                        NSW AGRI - MOONACRES HARVEST KITCHEN,4310777609,Meter 1,4310777609-1,elec_energy_consumed,2024-12-12 01:00:00,2024-12-12 01:30:00,1061,Wh,AEMO:QUALITY:A,0.73,774.53,g-CO2•e
                        NSW AGRI - MOONACRES HARVEST KITCHEN,4310777609,Meter 1,4310777609-1,elec_energy_consumed,2024-12-12 01:30:00,2024-12-12 02:00:00,1155,Wh,AEMO:QUALITY:A,0.73,843.15,g-CO2•e
                        NSW AGRI - MOONACRES HARVEST KITCHEN,4310777609,Meter 1,4310777609-1,elec_energy_consumed,2024-12-12 02:00:00,2024-12-12 02:30:00,1243,Wh,AEMO:QUALITY:A,0.73,907.39,g-CO2•e
                        NSW AGRI - MOONACRES HARVEST KITCHEN,4310777609,Meter 1,4310777609-1,elec_energy_consumed,2024-12-12 02:30:00,2024-12-12 03:00:00,1035,Wh,AEMO:QUALITY:A,0.73,755.55,g-CO2•e
                        */

                        var record = new CsvEnosiFormat.DataLine
                       (
                             PropertyTitle: line.GetString(0),
                             PrimaryBillingPointIdentifier: line.GetStringUpper(1),
                             MeterTitle: line.GetString(2),
                             MeterIdentifier: line.GetStringUpper(3),
                             Metric: line.GetStringUpper(4),
                             Start: line.GetDateMandatory(5, "yyyy-MM-dd HH:mm:ss"),
                             Finish: line.GetDateMandatory(6, "yyyy-MM-dd HH:mm:ss"),
                             Value: line.GetDecimalMandatory(7),
                             Units: line.GetString(8),
                             Flags: line.GetString(9),
                             CarbonEmissionality: line.GetDecimalCol(10) ?? 0,
                             CarbonEmissions: line.GetDecimalCol(11) ?? 0,
                             CarbonEmissionsUnits: line.GetString(12),


                             LineNumber: line.LineNumber
                       );
                        records.Add(record);



                    }
                    catch (Exception ex)
                    {
                        result.LogMessages.Add(new FileLogMessage(ex.Message, Microsoft.Extensions.Logging.LogLevel.Critical, filename, reader.LineNumber, 0));
                    }

                }


                var list = records.GroupBy(x => new { x.PrimaryBillingPointIdentifier, x.ReadDate })
                      .Where(x => x.Count() > 1)
                      .OrderBy(x => x.Key.PrimaryBillingPointIdentifier)
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
                        SiteCode = siteDayGroup.Key.PrimaryBillingPointIdentifier,
                        Date = siteDayGroup.Key.ReadDate,
                        Channels = [],
                    };
                    result.SitesDays.Add(siteDay);

                    // Transform the data into a dictionary of channels 
                    var byChannel = siteDayGroup.GroupBy(x => x.Suffix);
                    foreach (var channelSet in byChannel)
                    {


                        int intervalDuration = channelSet.First().IntervalDurationInMinutes;
                        string serial = channelSet.First().MeterIdentifier;
                        int expectedPeriods = 60 * 24 / intervalDuration;


                        ChannelDay chlDy = new()
                        {
                            Channel = channelSet.Key,
                            IntervalMinutes = intervalDuration,
                            Readings = new decimal[expectedPeriods],
                            TimeStampUtc = DateTime.UtcNow,
                            Metadata = [new MeterDataInfo(MetaDataName.Name, channelSet.First().MeterTitle)],
                            MeterId = serial,
                            RegisterId = channelSet.Key,
                            SourceFile = filename,
                            Total = 0,
                            IsAvg = false,
                            IsNet = false,
                            IsCheck = false
                        };
                        siteDay.Channels.Add(chlDy.Channel, chlDy);
                        UnitOfMeasure uom = channelSet.First().Units.ToUom();
                        (UnitOfMeasure stdUom, decimal multiplier) = uom.ToStandardUnit();
                        chlDy.UnitOfMeasure = stdUom;
                        chlDy.OriginalUnitOfMeasure = uom;
                        chlDy.OriginalUnitOfMeasureSymbol = channelSet.First().Units;
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

                        int period = 0;
                        foreach (var dataPoint in channelSet.OrderBy(x => x.Start))
                        {
                            period++;
                            int index = period - 1;
                            DateTime expectedStartTime = siteDay.Date.AddMinutes(index * intervalDuration);
                            DateTime recordedStartTime = dataPoint.Start;
                            if (expectedStartTime != recordedStartTime)
                            {
                                result.LogMessages.Add(new FileLogMessage($"The interval period {period} indicates a start time {expectedStartTime:HH:mm:ss} but found {recordedStartTime:HH:mm:ss} ", LogLevel.Error, filename, dataPoint.LineNumber));
                                continue;
                            }
                            if (index < 0 || index >= chlDy.Readings.Length)
                            {
                                result.LogMessages.Add(new FileLogMessage($"Invalid index period found  {period} for interval {intervalDuration} ", LogLevel.Error, filename, dataPoint.LineNumber));
                                continue;
                            }
                            chlDy.Readings[index] = dataPoint.Value * multiplier;
                            if (chlDy.OverallQuality == Quality.Variable && chlDy.ReadQualities != null)
                            {
                                string qualityMethod = dataPoint.QualityMethod.Trim();
                                if (qualityMethod.Length == 0) { qualityMethod = "A"; }
                                string quality = qualityMethod[0].ToString().ToUpper();
                                chlDy.ReadQualities[index] = quality.ToQuality();
                                if (qualityMethod.Length > 1)
                                {
                                    string estimationType = qualityMethod[1..];
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
        record DataLine(
            string PropertyTitle, string PrimaryBillingPointIdentifier,
            string MeterTitle, string MeterIdentifier, string Metric,
            DateTime Start, DateTime Finish,
            decimal Value, string Units, string Flags,
            decimal CarbonEmissionality, decimal CarbonEmissions, string CarbonEmissionsUnits,
            int LineNumber
        )
        {

            public DateTime ReadDate => Start.Date;

            public string RegisterId => MeterIdentifier.Last().ToString();

            public ChanelType ChanelType
            {
                get
                {
                    return Metric.ToUpperInvariant() switch
                    {
                        "ELEC_ENERGY_CONSUMED" => ChanelType.ActiveEnergyConsumption,
                        "ELEC_ENERGY_GENERATED" => ChanelType.ActiveEnergyGeneration,
                        //"ELEC_REACTIVE_ENERGY_CONSUMED" => ChanelType.ReactiveEnergyConsumption,
                        //"ELEC_REACTIVE_ENERGY_DELIVERED" => ChanelType.ReactiveEnergyGeneration,
                        //"ELEC_APPARENT_ENERGY_CONSUMED" => ChanelType.ApparentPowerConsumption,
                        //"ELEC_APPARENT_ENERGY_DELIVERED" => ChanelType.ApparentPowerGeneration,
                        //"ELEC_POWER_FACTOR" => ChanelType.PowerFactor,
                        //"ELEC_VOLTAGE" => ChanelType.Volts,
                        //"ELEC_CURRENT" => ChanelType.Current,
                        //"ELEC_FREQUENCY" => ChanelType.Other,
                        //"ELEC_POWER" => ChanelType.RealPower,
                        //"ELEC_REACTIVE_POWER" => ChanelType.ReactivePower,
                        //"ELEC_APPARENT_POWER" => ChanelType.ApparentPower,
                        _ => ChanelType.Unknown,
                    };
                }
            }

            public string ChannelPrefix => ChanelType switch
            {
                ChanelType.ActiveEnergyConsumption => "E",
                ChanelType.ActiveEnergyGeneration => "B",
                ChanelType.ReactiveEnergyConsumption => "Q",
                ChanelType.ReactiveEnergyGeneration => "K",
                _ => "X",
            };

            public string Suffix => ChannelPrefix + RegisterId;

            public int IntervalDurationInMinutes => (int)(Finish - Start).TotalMinutes;

            public string QualityMethod
            {
                get
                {
                    var qIndex = Flags.IndexOf("AEMO:QUALITY:", StringComparison.InvariantCultureIgnoreCase);
                    if (qIndex < 0) return "A";
                    if (qIndex + 13 >= Flags.Length) return "A";
                    var q = Flags.Substring(qIndex + 13, 1).ToUpperInvariant();
                    return q;
                }
            }




        }
    }
}
