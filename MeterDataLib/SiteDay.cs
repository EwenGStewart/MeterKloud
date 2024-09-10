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

        static readonly ChanelType[] QuadrantTypes = [ChanelType.ActiveEnergyConsumption, ChanelType.ActiveEnergyGeneration, ChanelType.ReactiveEnergyConsumption, ChanelType.ReactiveEnergyGeneration];

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
            List<EnergyQuadrant> quadrants = [];
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

            List<EnergyQuadrant> quadrants = [];
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
                    decimal activeEnergyConsumption = channelDay.ChannelType == ChanelType.ActiveEnergyConsumption ? readings[i] : 0;
                    decimal activeEnergyGeneration = channelDay.ChannelType == ChanelType.ActiveEnergyGeneration ? readings[i] : 0;
                    decimal reactiveEnergyConsumption = channelDay.ChannelType == ChanelType.ReactiveEnergyConsumption ? readings[i] : 0;
                    decimal reactivePowerGeneration = channelDay.ChannelType == ChanelType.ReactiveEnergyGeneration ? readings[i] : 0;

                    if (activeEnergyConsumption < 0)
                    {
                        activeEnergyGeneration -= activeEnergyConsumption;
                        activeEnergyConsumption = 0;
                    }
                    if (activeEnergyGeneration < 0)
                    {
                        activeEnergyConsumption -= activeEnergyGeneration;
                        activeEnergyGeneration = 0;
                    }
                    if (reactiveEnergyConsumption < 0)
                    {
                        reactivePowerGeneration -= reactiveEnergyConsumption;
                        reactiveEnergyConsumption = 0;
                    }
                    if (reactivePowerGeneration < 0)
                    {
                        reactiveEnergyConsumption -= reactivePowerGeneration;
                        reactivePowerGeneration = 0;
                    }


                    EnergyQuadrant energyQuadrant = new(Date.AddMinutes(i * targetInterval), meter, channelNumber, channelList, targetInterval, quality,
                        activeEnergyConsumption, activeEnergyGeneration, reactiveEnergyConsumption, reactivePowerGeneration);
                    quadrants.Add(energyQuadrant);
                }

            }


            EnergyQuadrant[] result;

            if (quadrantOptions.ByMeter) // group by meter
            {
                result = quadrants.GroupBy(x => new { x.ReadingDateTime, x.Meter })
                    .Select(x => new EnergyQuadrant(x.Key.ReadingDateTime, x.Key.Meter, x.Max(y => y.ChannelNumber), string.Join('|', x.Select(y => y.ChannelList).Distinct().Order()), x.First().IntervalMinutes,
                        x.Max(y => y.Quality),
                        x.Sum(y => y.ActiveEnergyConsumption_kWh),
                        x.Sum(y => y.ActiveEnergyGeneration_kWh),
                        x.Sum(y => y.ReactiveEnergyConsumption_kVArh),
                        x.Sum(y => y.ReactiveEnergyGeneration_kVArh)
                        )).OrderBy(x => x.ReadingDateTime).ThenBy(x => x.Meter).ToArray();
            }
            else
            {

                result = quadrants.GroupBy(x => x.ReadingDateTime)
                    .Select(x => new EnergyQuadrant(x.Key, null, null, null, x.First().IntervalMinutes,
                        x.Max(y => y.Quality),
                        x.Sum(y => y.ActiveEnergyConsumption_kWh),
                        x.Sum(y => y.ActiveEnergyGeneration_kWh),
                        x.Sum(y => y.ReactiveEnergyConsumption_kVArh),
                        x.Sum(y => y.ReactiveEnergyGeneration_kVArh)
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
            var totalActivePowerConsumption = quadrants.Select(x => x.ActiveEnergyConsumption_kWh).Sum();
            var totalActivePowerGeneration = quadrants.Select(x => x.ActiveEnergyGeneration_kWh).Sum();
            var totalReactiveEnergyConsumption = quadrants.Select(x => x.ReactiveEnergyConsumption_kVArh).Sum();
            var totalReactivePowerGeneration = quadrants.Select(x => x.ReactiveEnergyGeneration_kVArh).Sum();
            decimal maxKw = quadrants.OrderByDescending(x => x.RealPowerConsumption_kW).First().RealPowerConsumption_kW;
            var maxQuad = quadrants.OrderByDescending(x => x.CalculateKva(setKvaToZeroWhenActiveGenerationExceedsConsumption: true)).ThenByDescending(x=>x.ReadingDateTime).First();
            var maxKva = maxQuad.ApparentPower_kVA;
            var maxKvaTime = maxQuad.ReadingDateTime;
            var pf  = maxQuad.PowerFactor;
            var result = new EnergyDailySummary(
                    Date, meters, channels, readInterval, quality, totalActivePowerConsumption, totalActivePowerGeneration, totalReactiveEnergyConsumption, totalReactivePowerGeneration
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
                        if (result[i].ActiveEnergyGeneration_kWh > 0 && result[i].ActiveEnergyConsumption_kWh > 0)
                        {
                            if (result[i].ActiveEnergyGeneration_kWh > result[i].ActiveEnergyConsumption_kWh)
                            {
                                result[i] = result[i] with { ActiveEnergyGeneration_kWh = result[i].ActiveEnergyGeneration_kWh - result[i].ActiveEnergyConsumption_kWh, ActiveEnergyConsumption_kWh = 0 };
                            }
                            else
                            {
                                result[i] = result[i] with { ActiveEnergyConsumption_kWh = result[i].ActiveEnergyConsumption_kWh - result[i].ActiveEnergyGeneration_kWh, ActiveEnergyGeneration_kWh = 0 };
                            }
                        }
                    }
                    break;
                case GenerationHandlingOption.IgnoreGeneration:
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (result[i].ActiveEnergyGeneration_kWh > 0)
                        {
                            result[i].ActiveEnergyGeneration_kWh = 0;
                        }
                    }

                    break;
                case GenerationHandlingOption.NetAndIgnoreGeneration:
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (result[i].ActiveEnergyGeneration_kWh > 0 && result[i].ActiveEnergyConsumption_kWh > 0)
                        {
                            if (result[i].ActiveEnergyGeneration_kWh > result[i].ActiveEnergyConsumption_kWh)
                            {
                                result[i] = result[i] with { ActiveEnergyGeneration_kWh = 0, ActiveEnergyConsumption_kWh = 0 };
                            }
                            else
                            {
                                result[i] = result[i] with { ActiveEnergyConsumption_kWh = result[i].ActiveEnergyConsumption_kWh - result[i].ActiveEnergyGeneration_kWh, ActiveEnergyGeneration_kWh = 0 };
                            }
                        }


                    }
                    break;


                case GenerationHandlingOption.IgnoreConsumption:
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (result[i].ActiveEnergyConsumption_kWh > 0)
                        {
                            result[i].ActiveEnergyConsumption_kWh = 0;
                        }
                    }
                    break;
                case GenerationHandlingOption.NetAndIgnoreConsumption:
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (result[i].ActiveEnergyGeneration_kWh > 0 && result[i].ActiveEnergyConsumption_kWh > 0)
                        {
                            if (result[i].ActiveEnergyGeneration_kWh > result[i].ActiveEnergyConsumption_kWh)
                            {
                                result[i] = result[i] with { ActiveEnergyGeneration_kWh = result[i].ActiveEnergyGeneration_kWh - result[i].ActiveEnergyConsumption_kWh, ActiveEnergyConsumption_kWh = 0 };
                            }
                            else
                            {
                                result[i] = result[i] with { ActiveEnergyConsumption_kWh = 0, ActiveEnergyGeneration_kWh = 0 };
                            }
                        }
                    }
                    break;
                case GenerationHandlingOption.IgnoreBoth:
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (result[i].ActiveEnergyGeneration_kWh > 0 || result[i].ActiveEnergyConsumption_kWh > 0)
                        {
                                result[i] = result[i] with { ActiveEnergyConsumption_kWh = 0, ActiveEnergyGeneration_kWh = 0 };
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
                        if (result[i].ReactiveEnergyGeneration_kVArh > 0 && result[i].ReactiveEnergyConsumption_kVArh > 0)
                        {
                            if (result[i].ReactiveEnergyGeneration_kVArh > result[i].ActiveEnergyConsumption_kWh)
                            {
                                result[i] = result[i] with { ReactiveEnergyGeneration_kVArh = result[i].ReactiveEnergyGeneration_kVArh - result[i].ReactiveEnergyConsumption_kVArh, ReactiveEnergyConsumption_kVArh = 0 };
                            }
                            else
                            {
                                result[i] = result[i] with { ReactiveEnergyConsumption_kVArh = result[i].ReactiveEnergyConsumption_kVArh - result[i].ReactiveEnergyGeneration_kVArh, ReactiveEnergyGeneration_kVArh = 0 };
                            }
                        }
                    }
                    break;
                case GenerationHandlingOption.IgnoreGeneration:
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (result[i].ReactiveEnergyGeneration_kVArh > 0)
                        {
                            result[i].ReactiveEnergyGeneration_kVArh = 0;
                        }
                    }

                    break;
                case GenerationHandlingOption.NetAndIgnoreGeneration:
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (result[i].ReactiveEnergyGeneration_kVArh > 0 && result[i].ReactiveEnergyConsumption_kVArh > 0)
                        {
                            if (result[i].ReactiveEnergyGeneration_kVArh > result[i].ReactiveEnergyConsumption_kVArh)
                            {
                                result[i] = result[i] with { ReactiveEnergyGeneration_kVArh = 0, ReactiveEnergyConsumption_kVArh = 0 };
                            }
                            else
                            {
                                result[i] = result[i] with { ReactiveEnergyConsumption_kVArh = result[i].ReactiveEnergyConsumption_kVArh - result[i].ReactiveEnergyGeneration_kVArh, ReactiveEnergyGeneration_kVArh = 0 };
                            }
                        }


                    }
                    break;


                case GenerationHandlingOption.IgnoreConsumption:
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (result[i].ReactiveEnergyConsumption_kVArh > 0)
                        {
                            result[i].ReactiveEnergyConsumption_kVArh = 0;
                        }
                    }
                    break;
                case GenerationHandlingOption.NetAndIgnoreConsumption:
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (result[i].ReactiveEnergyGeneration_kVArh > 0 && result[i].ReactiveEnergyConsumption_kVArh > 0)
                        {
                            if (result[i].ReactiveEnergyGeneration_kVArh > result[i].ReactiveEnergyConsumption_kVArh)
                            {
                                result[i] = result[i] with { ReactiveEnergyConsumption_kVArh = result[i].ReactiveEnergyGeneration_kVArh - result[i].ActiveEnergyConsumption_kWh, ActiveEnergyConsumption_kWh = 0 };
                            }
                            else
                            {
                                result[i] = result[i] with { ReactiveEnergyConsumption_kVArh = 0, ReactiveEnergyGeneration_kVArh = 0 };
                            }
                        }


                    }
                    break;
                case GenerationHandlingOption.IgnoreBoth:
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (result[i].ReactiveEnergyGeneration_kVArh > 0 || result[i].ReactiveEnergyConsumption_kVArh > 0)
                        {
                            result[i] = result[i] with { ReactiveEnergyConsumption_kVArh = 0, ReactiveEnergyGeneration_kVArh = 0 };
                        }
                    }
                    break;
            }
        }
    }






}