using MeterDataLib.Parsers;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace MeterDataLib
{
    public class SiteDay
    {

        public string? Id { get; set; }



        private string siteCode = ParserResult.UNKNOWN;
        public string SiteCode
        {
            get => siteCode;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    siteCode = ParserResult.UNKNOWN;
                }
                else
                {
                    siteCode = value.Trim().ToUpperInvariant();
                }
            }
        }



        public DateTime Date { get; set; }
        public Dictionary<string, ChannelDay> Channels { get; set; } = [];

        public string TimeZoneName { get; set; } = string.Empty;
        public decimal? UCT_Offset { get; set; } = null;

        public const int MinutesPerDay = 24 * 60;

        static ChanelType[] QuadrantTypes = new ChanelType[] { ChanelType.ActivePowerConsumption, ChanelType.ActivePowerGeneration, ChanelType.ReactivePowerConsumption, ChanelType.ReactivePowerGeneration };

        EnergyQuadrant[] EmptyQuadrant(int? interval)
        {
            if (interval == null || interval <= 0 || interval > MinutesPerDay || MinutesPerDay % interval != 0 )
            {
                interval = Channels.Values.Where(x=>x.IntervalMinutes > 0).Select(x => x.IntervalMinutes).Min();
                if (interval == null || interval <= 0 || interval > MinutesPerDay || MinutesPerDay % interval != 0)
                {
                    interval = 30; 
                }

            }
            var periods = MinutesPerDay / interval.Value;
            List<EnergyQuadrant> quadrants = new List<EnergyQuadrant>();
            for (int i = 0; i < periods; i++)
            {
                quadrants.Add(new EnergyQuadrant(Date.AddMinutes(i * interval.Value), null, null, null, interval.Value, Quality.Unknown, 0, 0, 0, 0));
            }
            return quadrants.ToArray();

        }

        public EnergyQuadrant[] GetEnergyQuadrants(QuadrantOptions? quadrantOptions)
        {

            quadrantOptions ??= new QuadrantOptions();
            var keyChannels = Channels.Values.Where(x => !x.Ignore && QuadrantTypes.Contains(x.ChannelType) && x.IntervalMinutes > 0);
            if (quadrantOptions.OnlyIncludeMeterFilter.Length > 0)
            {
                keyChannels = keyChannels.Where(x => quadrantOptions.OnlyIncludeMeterFilter.Contains(x.MeterId));
            }
            if (quadrantOptions.OnlyIncludeChannelFilter.Length > 0)
            {
                keyChannels = keyChannels.Where(x => quadrantOptions.OnlyIncludeChannelFilter.Contains(x.Channel));
            }
            if (quadrantOptions.OnlyIncludeChanelNumberFilter.Length > 0)
            {
                keyChannels = keyChannels.Where(x => quadrantOptions.OnlyIncludeChanelNumberFilter.Contains(x.ChannelNumber));
            }
            if (quadrantOptions.AlwaysExcludeMeterFilter.Length > 0)
            {
                keyChannels = keyChannels.Where(x => !quadrantOptions.AlwaysExcludeMeterFilter.Contains(x.MeterId));
            }
            if (quadrantOptions.AlwaysExcludeChannelFilter.Length > 0)
            {
                keyChannels = keyChannels.Where(x => !quadrantOptions.AlwaysExcludeChannelFilter.Contains(x.Channel));
            }
            if (quadrantOptions.AlwaysExcludeChanelNumberFilter.Length > 0)
            {
                keyChannels = keyChannels.Where(x => !quadrantOptions.AlwaysExcludeChanelNumberFilter.Contains(x.ChannelNumber));
            }
            if (quadrantOptions.ControlledLoad == FlagFilter.Include)
            {
                keyChannels = keyChannels.Where(x => x.Controlled);
            }
            else if (quadrantOptions.ControlledLoad == FlagFilter.Exclude)
            {
                keyChannels = keyChannels.Where(x => !x.Controlled);
            }


            if (!keyChannels.Any())
            {
                return EmptyQuadrant(quadrantOptions.Interval);
            }
            var intervals = keyChannels.Select(x => x.IntervalMinutes).Distinct();
            if (!intervals.Any())
            {
                return EmptyQuadrant(quadrantOptions.Interval);
            }

            int targetInterval = quadrantOptions.Interval ?? intervals.Min();
            if (targetInterval <= 0)
            {
                targetInterval = intervals.Min();
            }
            // interval must be a factor of 24 * 60 = 1440
            if (MinutesPerDay % targetInterval != 0)
            {
                throw new ArgumentException("Interval must be a factor of 1440 - 1min, 5min, 15min, 30min , 60min  ");
            }

            int expectedReads = MinutesPerDay / targetInterval;
            if (expectedReads == 0)
            {
                return EmptyQuadrant(quadrantOptions.Interval);
            }

            List<EnergyQuadrant> quadrants = new List<EnergyQuadrant>();
            foreach (var channelDay in keyChannels)
            {


                string? meter = channelDay.MeterId;
                string? channelNumber = channelDay.ChannelNumberOrMeterName;
                string? channelList = channelDay.Channel;
                var readings = channelDay.Readings;
                if (readings.Length != expectedReads)
                {
                    readings = ProfileHelpers.UnifyLength(readings, expectedReads, quadrantOptions.UseSimpleIntervalCorrection);
                }
                for (int i = 0; i < expectedReads; i++)
                {

                    var quality = channelDay.ReadQualities?[i] ?? channelDay.OverallQuality;
                    decimal activePowerConsumption = channelDay.ChannelType == ChanelType.ActivePowerConsumption ? readings[i] : 0;
                    decimal activePowerGeneration = channelDay.ChannelType == ChanelType.ActivePowerGeneration ? readings[i] : 0;
                    decimal reactivePowerConsumption = channelDay.ChannelType == ChanelType.ReactivePowerConsumption ? readings[i] : 0;
                    decimal reactivePowerGeneration = channelDay.ChannelType == ChanelType.ReactivePowerGeneration ? readings[i] : 0;

                    if (activePowerConsumption < 0)
                    {
                        activePowerGeneration -= activePowerConsumption;
                        activePowerConsumption = 0;
                    }
                    if (activePowerGeneration < 0)
                    {
                        activePowerConsumption -= activePowerGeneration;
                        activePowerGeneration = 0;
                    }
                    if (reactivePowerConsumption < 0)
                    {
                        reactivePowerGeneration -= reactivePowerConsumption;
                        reactivePowerConsumption = 0;
                    }
                    if (reactivePowerGeneration < 0)
                    {
                        reactivePowerConsumption -= reactivePowerGeneration;
                        reactivePowerGeneration = 0;
                    }


                    EnergyQuadrant energyQuadrant = new EnergyQuadrant(Date.AddMinutes(i * targetInterval), meter, channelNumber, channelList, targetInterval, quality,
                        activePowerConsumption, activePowerGeneration, reactivePowerConsumption, reactivePowerGeneration);
                    quadrants.Add(energyQuadrant);
                }

            }


            EnergyQuadrant[] result;

            if (quadrantOptions.ByMeter) // group by meter
            {
                result = quadrants.GroupBy(x => new { x.ReadingDateTime, x.Meter })
                    .Select(x => new EnergyQuadrant(x.Key.ReadingDateTime, x.Key.Meter, x.Max(y => y.ChannelNumber), string.Join('|', x.Select(y => y.ChannelList).Distinct().Order()), x.First().IntervalMinutes,
                        x.Max(y => y.Quality),
                        x.Sum(y => y.ActivePowerConsumption_kWh),
                        x.Sum(y => y.ActivePowerGeneration_kWh),
                        x.Sum(y => y.ReactivePowerConsumption_kVArh),
                        x.Sum(y => y.ReactivePowerGeneration_kVArh)
                        )).OrderBy(x => x.ReadingDateTime).ThenBy(x => x.Meter).ToArray();
            }
            else
            {

                result = quadrants.GroupBy(x => x.ReadingDateTime)
                    .Select(x => new EnergyQuadrant(x.Key, null, null, null, x.First().IntervalMinutes,
                        x.Max(y => y.Quality),
                        x.Sum(y => y.ActivePowerConsumption_kWh),
                        x.Sum(y => y.ActivePowerGeneration_kWh),
                        x.Sum(y => y.ReactivePowerConsumption_kVArh),
                        x.Sum(y => y.ReactivePowerGeneration_kVArh)
                        )).OrderBy(x => x.ReadingDateTime).ToArray();
            }

            ProcessRealPowerGenerationOptions(quadrantOptions, result);

            ProcessReactivePowerGenerationOptions(quadrantOptions, result);

            return result;

        }



        public EnergyDailySummary GetDailySummary ( EnergyDailySummaryOptions?  options  = null )
        {
            options ??= new EnergyDailySummaryOptions();
            var demandInterval = options.Interval ;
            if (demandInterval < 1 || demandInterval > MinutesPerDay) { demandInterval = 30;  }
            var keyChannels = Channels.Values.Where(x => !x.Ignore && QuadrantTypes.Contains(x.ChannelType) && x.IntervalMinutes > 0);
            var quadrants = GetEnergyQuadrants( new QuadrantOptions() { Interval = demandInterval });
            var readInterval = keyChannels.Where( x=>x.IntervalMinutes > 0 && x.IntervalMinutes <= MinutesPerDay).Select(x => x.IntervalMinutes).Min();
            var meters =  keyChannels.Select(x=>x.MeterId).Distinct().Count();
            var channels = keyChannels.Count();
            var readings = keyChannels.Select(x => x.Readings.Length).Sum();
            var quality = quadrants.Select(x => x.Quality).Max();   
            var totalActivePowerConsumption = quadrants.Select(x => x.ActivePowerConsumption_kWh).Sum();
            var totalActivePowerGeneration = quadrants.Select(x => x.ActivePowerGeneration_kWh).Sum();
            var totalReactivePowerConsumption = quadrants.Select(x => x.ReactivePowerConsumption_kVArh).Sum();
            var totalReactivePowerGeneration = quadrants.Select(x => x.ReactivePowerGeneration_kVArh).Sum();
            decimal maxKw = quadrants.OrderByDescending(x => x.RealPowerConsumption_kW).First().RealPowerConsumption_kW;
            var maxQuad = quadrants.OrderByDescending(x => x.CalculateKva(setKvaToZeroWhenActiveGenerationExceedsConsumption: true)).ThenByDescending(x=>x.ReadingDateTime).First();
            var maxKva = maxQuad.ApparentPower_kVA;
            var maxKvaTime = maxQuad.ReadingDateTime;
            var pf  = maxQuad.PowerFactor;
            var result = new EnergyDailySummary(
                    Date, meters, channels, readInterval, quality, totalActivePowerConsumption, totalActivePowerGeneration, totalReactivePowerConsumption, totalReactivePowerGeneration
                , maxKw, maxKva, pf, maxKvaTime
                );
            return result; 
        }

        private EnergyDailySummary? energyDailySummary = null;
        public EnergyDailySummary EnergyDailySummary
        {
            get
            {
                if (energyDailySummary == null)
                {
                    energyDailySummary = GetDailySummary();
                }
                return energyDailySummary.Value;
            }
        }







        private static void ProcessRealPowerGenerationOptions(QuadrantOptions quadrantOptions, EnergyQuadrant[] result)
        {
            switch (quadrantOptions.RealPowerGenerationHandling)
            {
                case GenerationHandlingOption.IncludeGenerationAndConsumption:
                    break;
                case GenerationHandlingOption.NetGeneration:
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (result[i].ActivePowerGeneration_kWh > 0 && result[i].ActivePowerConsumption_kWh > 0)
                        {
                            if (result[i].ActivePowerGeneration_kWh > result[i].ActivePowerConsumption_kWh)
                            {
                                result[i] = result[i] with { ActivePowerGeneration_kWh = result[i].ActivePowerGeneration_kWh - result[i].ActivePowerConsumption_kWh, ActivePowerConsumption_kWh = 0 };
                            }
                            else
                            {
                                result[i] = result[i] with { ActivePowerConsumption_kWh = result[i].ActivePowerConsumption_kWh - result[i].ActivePowerGeneration_kWh, ActivePowerGeneration_kWh = 0 };
                            }
                        }
                    }
                    break;
                case GenerationHandlingOption.IgnoreGeneration:
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (result[i].ActivePowerGeneration_kWh > 0)
                        {
                            result[i].ActivePowerGeneration_kWh = 0;
                        }
                    }

                    break;
                case GenerationHandlingOption.NetAndIgnoreGeneration:
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (result[i].ActivePowerGeneration_kWh > 0 && result[i].ActivePowerConsumption_kWh > 0)
                        {
                            if (result[i].ActivePowerGeneration_kWh > result[i].ActivePowerConsumption_kWh)
                            {
                                result[i] = result[i] with { ActivePowerGeneration_kWh = 0, ActivePowerConsumption_kWh = 0 };
                            }
                            else
                            {
                                result[i] = result[i] with { ActivePowerConsumption_kWh = result[i].ActivePowerConsumption_kWh - result[i].ActivePowerGeneration_kWh, ActivePowerGeneration_kWh = 0 };
                            }
                        }


                    }
                    break;


                case GenerationHandlingOption.IgnoreConsumption:
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (result[i].ActivePowerConsumption_kWh > 0)
                        {
                            result[i].ActivePowerConsumption_kWh = 0;
                        }
                    }
                    break;
                case GenerationHandlingOption.NetAndIgnoreConsumption:
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (result[i].ActivePowerGeneration_kWh > 0 && result[i].ActivePowerConsumption_kWh > 0)
                        {
                            if (result[i].ActivePowerGeneration_kWh > result[i].ActivePowerConsumption_kWh)
                            {
                                result[i] = result[i] with { ActivePowerGeneration_kWh = result[i].ActivePowerGeneration_kWh - result[i].ActivePowerConsumption_kWh, ActivePowerConsumption_kWh = 0 };
                            }
                            else
                            {
                                result[i] = result[i] with { ActivePowerConsumption_kWh = 0, ActivePowerGeneration_kWh = 0 };
                            }
                        }
                    }
                    break;
                case GenerationHandlingOption.IgnoreBoth:
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (result[i].ActivePowerGeneration_kWh > 0 || result[i].ActivePowerConsumption_kWh > 0)
                        {
                                result[i] = result[i] with { ActivePowerConsumption_kWh = 0, ActivePowerGeneration_kWh = 0 };
                        }
                    }
                    break;
            }
        }

        private static void ProcessReactivePowerGenerationOptions(QuadrantOptions quadrantOptions, EnergyQuadrant[] result)
        {
            switch (quadrantOptions.ReactivePowerGenerationHandling)
            {
                case GenerationHandlingOption.IncludeGenerationAndConsumption:
                    break;
                case GenerationHandlingOption.NetGeneration:
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (result[i].ReactivePowerGeneration_kVArh > 0 && result[i].ReactivePowerConsumption_kVArh > 0)
                        {
                            if (result[i].ReactivePowerGeneration_kVArh > result[i].ActivePowerConsumption_kWh)
                            {
                                result[i] = result[i] with { ReactivePowerGeneration_kVArh = result[i].ReactivePowerGeneration_kVArh - result[i].ReactivePowerConsumption_kVArh, ReactivePowerConsumption_kVArh = 0 };
                            }
                            else
                            {
                                result[i] = result[i] with { ReactivePowerConsumption_kVArh = result[i].ReactivePowerConsumption_kVArh - result[i].ReactivePowerGeneration_kVArh, ReactivePowerGeneration_kVArh = 0 };
                            }
                        }
                    }
                    break;
                case GenerationHandlingOption.IgnoreGeneration:
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (result[i].ReactivePowerGeneration_kVArh > 0)
                        {
                            result[i].ReactivePowerGeneration_kVArh = 0;
                        }
                    }

                    break;
                case GenerationHandlingOption.NetAndIgnoreGeneration:
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (result[i].ReactivePowerGeneration_kVArh > 0 && result[i].ReactivePowerConsumption_kVArh > 0)
                        {
                            if (result[i].ReactivePowerGeneration_kVArh > result[i].ReactivePowerConsumption_kVArh)
                            {
                                result[i] = result[i] with { ReactivePowerGeneration_kVArh = 0, ReactivePowerConsumption_kVArh = 0 };
                            }
                            else
                            {
                                result[i] = result[i] with { ReactivePowerConsumption_kVArh = result[i].ReactivePowerConsumption_kVArh - result[i].ReactivePowerGeneration_kVArh, ReactivePowerGeneration_kVArh = 0 };
                            }
                        }


                    }
                    break;


                case GenerationHandlingOption.IgnoreConsumption:
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (result[i].ReactivePowerConsumption_kVArh > 0)
                        {
                            result[i].ReactivePowerConsumption_kVArh = 0;
                        }
                    }
                    break;
                case GenerationHandlingOption.NetAndIgnoreConsumption:
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (result[i].ReactivePowerGeneration_kVArh > 0 && result[i].ReactivePowerConsumption_kVArh > 0)
                        {
                            if (result[i].ReactivePowerGeneration_kVArh > result[i].ReactivePowerConsumption_kVArh)
                            {
                                result[i] = result[i] with { ReactivePowerConsumption_kVArh = result[i].ReactivePowerGeneration_kVArh - result[i].ActivePowerConsumption_kWh, ActivePowerConsumption_kWh = 0 };
                            }
                            else
                            {
                                result[i] = result[i] with { ReactivePowerConsumption_kVArh = 0, ReactivePowerGeneration_kVArh = 0 };
                            }
                        }


                    }
                    break;
                case GenerationHandlingOption.IgnoreBoth:
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (result[i].ReactivePowerGeneration_kVArh > 0 || result[i].ReactivePowerConsumption_kVArh > 0)
                        {
                            result[i] = result[i] with { ReactivePowerConsumption_kVArh = 0, ReactivePowerGeneration_kVArh = 0 };
                        }
                    }
                    break;
            }
        }
    }






}