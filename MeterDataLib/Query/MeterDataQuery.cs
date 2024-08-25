using System.Runtime.InteropServices.Marshalling;
using System.Security.Cryptography.X509Certificates;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MeterDataLib.Query
{
    public static class MeterDataQuery
    {



        public abstract class TimeBasedResult(QueryDateRange queryDateRange)
        {
            public QueryDateRange RequestedDateRange { get; set; } = queryDateRange;
            public QueryDateRange ActualDateRange { get; set; } = queryDateRange;

            public int Days => ActualDateRange.TotalDays;

            public int Points => Interval.NumberOfBuckets(ActualDateRange);


            public int Missing => Profile.Where(x => !x.HasValue).Count();

            public TimeInterval Interval { get; set; } = new TimeInterval(TimeIntervalSize.Day, 1);

            public DateTime[] Dates => Interval.Range(ActualDateRange);


            public IList<object> X => Array.ConvertAll<DateTime, object>(Dates, v => (object)v);

            public decimal?[] Profile { get; set; } = [];

            public IList<object> Y => Array.ConvertAll<decimal?, object>(Profile, v => (object)v!);

            public bool IsEmpty => Profile.Length == 0 || Profile.All(x => !x.HasValue);

            public decimal MaxValue => Profile.Where(x => x.HasValue).Select(x => x!.Value).Max();

            public DateTime MaxDateTime => Dates[Profile.Select((value, i) => (value, i)).Where(x => x.value.HasValue).OrderByDescending(x => x.value).First().i];

            public decimal MinValue => Profile.Where(x => x.HasValue).Select(x => x!.Value).Min();

            public DateTime MinDateTime => Dates[Profile.Select((value, i) => (value, i)).Where(x => x.value.HasValue).OrderBy(x => x.value).First().i];
            public decimal AvgDay => Profile.Where(x => x.HasValue).Select(x => x!.Value).Average();

            public decimal Median => CalculateMedian(Profile.Where(x => x.HasValue).Select(x => x!.Value).ToArray());


        }


        public class DailyConsumptionProfileResult(QueryDateRange queryDateRange) : TimeBasedResult(queryDateRange)
        {
            public Quality[] Quality { get; set; } = [];

            public DateTime MaxDay => Profile.Select((x, i) => new { Value = x, Date = Dates[i] }).Where(x => x.Value.HasValue).OrderByDescending(x => x.Value).FirstOrDefault()?.Date ?? ActualDateRange.FromDate;
        }


        public class DetailedConsumptionProfileResult(QueryDateRange queryDateRange) : TimeBasedResult(queryDateRange)
        {
        }



        public class HeatMapResult(QueryDateRange queryDateRange)
        {
            public QueryDateRange RequestedDateRange { get; set; } = queryDateRange;
            public QueryDateRange ActualDateRange { get; set; } = queryDateRange;

            public int Days => ActualDateRange.TotalDays;

            public TimeInterval Interval { get; set; } = new TimeInterval(TimeIntervalSize.Day, 1);
            public TimeOnly[] Times { get; set; } = [];
            public DateOnly[] Dates { get; set; } = [];
            public decimal[,] Profile { get; set; } = new decimal[0, 0];

            public int DateIndex(DateTime date)
            {
                var result = (int)date.Date.Subtract(ActualDateRange.FromDate.Date).TotalDays;
                if (result < 0 || result >= Dates.Length)
                {
                    throw new ArgumentException("Invalid Date");
                }
                return result;

            }





            // X = Time 
            public IList<object> X => Array.ConvertAll<TimeOnly, object>(Times, v => (object)(v.ToString("HH:mm")));
            // Y = Dates reversed 
            public IList<object> Y => Array.ConvertAll<DateOnly, object>(Dates, v => (object)(v.ToString("ddd dd-MMM-yy"))).Reverse().ToList();

            private IList<object>? _z = null;
            public IList<object> Z
            {
                get
                {
                    if (_z == null)
                    {
                        _z = new List<object>();
                        decimal[][] zValues = new decimal[Dates.Length][];

                        for (int dayIndex = Dates.Length - 1; dayIndex >= 0; dayIndex--)
                        {
                            decimal[] values = new decimal[Times.Length];
                            for (int timeIndex = 0; timeIndex < Times.Length; timeIndex++)
                            {
                                values[timeIndex] = Profile[timeIndex, dayIndex];
                            }
                            zValues[dayIndex] = values;
                        }
                        _z = zValues.Cast<object>().ToList();
                    }

                    return _z;
                }
            }


        }




        public class ProfileResult
        {
            readonly HeatMapResult _HeatMapResult;
            public ProfileResult(HeatMapResult heatMapResult)
            {
                _HeatMapResult = heatMapResult;
            }

            public TimeOnly[] Times => _HeatMapResult.Times;
            public IList<object> X => _HeatMapResult.X;




            public decimal[] GetProfile
                (
                   DayOfWeek? dayOfWeek = null,
                   bool? working = null,
                   Season? season = null,
                   int? month = null,
                   int? year = null
                )
            {
                var result = new decimal[Times.Length];
                var counts = new int[Times.Length];
                var sum = new decimal[Times.Length];
                for (int dayIndex = 0; dayIndex < _HeatMapResult.Dates.Length; dayIndex++)
                {
                    var date = _HeatMapResult.Dates[dayIndex];
                    if (dayOfWeek.HasValue && date.DayOfWeek != dayOfWeek.Value)
                    {
                        continue;
                    }
                    if (working.HasValue
                            && !dayOfWeek.HasValue &&
                                 (
                                        (working.Value == true
                                            && (
                                                    date.DayOfWeek == DayOfWeek.Saturday
                                                || date.DayOfWeek == DayOfWeek.Sunday
                                                )
                                         )
                                || (working.Value == false
                                            && (
                                                    date.DayOfWeek != DayOfWeek.Saturday
                                                 && date.DayOfWeek != DayOfWeek.Sunday
                                                 )
                                        )
                                )
                        )
                    {
                        continue;
                    }
                    if (season.HasValue && season.Value != GetSeason(date))
                    {
                        continue;
                    }
                    if (month.HasValue && month.Value != date.Month)
                    {
                        continue;
                    }
                    if (year.HasValue && year.Value != date.Year)
                    {
                        continue;
                    }

                    for (int timeIndex = 0; timeIndex < _HeatMapResult.Times.Length; timeIndex++)
                    {
                        var value = _HeatMapResult.Profile[timeIndex, dayIndex];
                        if (value != 0)
                        {
                            counts[timeIndex]++;
                            sum[timeIndex] += value;
                        }
                    }




                }


                for (int timeIndex = 0; timeIndex < _HeatMapResult.Times.Length; timeIndex++)
                {
                    if (counts[timeIndex] > 0)
                    {
                        result[timeIndex] = sum[timeIndex] / counts[timeIndex];
                    }
                }
                return result;


            }



            public static Season GetSeason(DateOnly date, bool southernHemisphere = true)
            {
                // Southern Hemisphere (Australia, South Africa, South America, New Zealand, etc.
                if (southernHemisphere)
                {
                    switch (date.Month)
                    {
                        case 12:
                        case 1:
                        case 2:
                            return Season.Summer;
                        case 3:
                        case 4:
                        case 5:
                            return Season.Autumn;
                        case 6:
                        case 7:
                        case 8:
                            return Season.Winter;
                        case 9:
                        case 10:
                        case 11:
                            return Season.Spring;
                        default:
                            throw new ArgumentException("Invalid Date");
                    }
                }
                else
                {

                    switch (date.Month)
                    {
                        case 12:
                        case 1:
                        case 2:
                            return Season.Winter;
                        case 3:
                        case 4:
                        case 5:
                            return Season.Spring;
                        case 6:
                        case 7:
                        case 8:
                            return Season.Summer;
                        case 9:
                        case 10:
                        case 11:
                            return Season.Autumn;
                        default:
                            throw new ArgumentException("Invalid Date");
                    }
                }



            }

        }


        const int IDEAL_POINTS = 500;
        const int MAX_POINTS = 100 * 288;


        public static DailyConsumptionProfileResult GetDailyNetConsumption(DateTime fromDate, DateTime toDate, IEnumerable<SiteDay> siteDays)
        {
            var dr = new QueryDateRange(fromDate, toDate).TrimYears(5).TrimFuture();
            if (!dr.IsValid)
            {
                throw new ArgumentException("Invalid Date Range");
            }
            var result = new DailyConsumptionProfileResult(dr);


            var interval = new TimeInterval(TimeIntervalSize.Day, 1);
            int days = result.Days;


            result.Profile = new decimal?[days];
            result.Quality = new Quality[days];
            Array.Fill(result.Quality, Quality.Missing);

            result.Interval = interval;
            result.ActualDateRange = new QueryDateRange(dr.FromDate, dr.ToDate);




            foreach (var siteDay in siteDays.Where(x => x.Date >= fromDate && x.Date <= toDate))
            {

                var offset = interval.Index(dr.FromDate, siteDay.Date);
                if (offset < 0 || offset >= days)
                {
                    continue;
                }

                result.Profile[offset] ??= 0;
                result.Profile[offset] += siteDay.EnergyDailySummary.TotalNetActivePower_kWh;
                result.Quality[offset] = siteDay.EnergyDailySummary.Quality;
            }
            return result;
        }


        public static DetailedConsumptionProfileResult GetDetailedNetConsumption(DateTime fromDate, DateTime toDate, IEnumerable<SiteDay> siteDays, TimeInterval? requestedInterval = null)
        {
            // ensure no more than 5 years of data can be requested  
            var dr = new QueryDateRange(fromDate, toDate).TrimFuture().TrimYears(5);
            if (!dr.IsValid)
            {
                throw new ArgumentException("Invalid Date Range");
            }
            var result = new DetailedConsumptionProfileResult(dr);
            var days = siteDays.Where(x => x.Date >= fromDate && x.Date <= toDate).ToList();
            var minInterval = days.Count == 0 ? 30 : days.SelectMany(x => x.Channels.Values).Select(x => x.IntervalMinutes).Where(x => x >= 1 && x <= 24 * 60).Min();
            if (minInterval < 1)
            {
                minInterval = 30;
            }

            var interval = requestedInterval ?? new TimeInterval(TimeIntervalSize.Minute, minInterval);
            var points = interval.NumberOfBuckets(dr);


            DateTime minDate = days.Min(x => x.Date);
            DateTime maxDate = days.Max(x => x.Date);






            // no data = return an empty time series
            if (days.Count == 0)
            {


                if (points > MAX_POINTS)
                {
                    interval = dr.DefaultInterval(MAX_POINTS);
                    points = interval.NumberOfBuckets(dr);
                }
                result.Interval = interval;
                result.Profile = new decimal?[points];
                return result;
            }

            // trim the request range to available data if there are too many points  
            if (points > IDEAL_POINTS && minDate > dr.FromDate)
            {
                dr = new QueryDateRange(minDate, dr.ToDate);
                points = interval.NumberOfBuckets(dr);
            }
            if (points > IDEAL_POINTS && maxDate < dr.ToDate)
            {
                dr = new QueryDateRange(dr.FromDate, maxDate);
                points = interval.NumberOfBuckets(dr);
            }

            // if super big  - then reduce the interval so that it does not exceed the max points  
            if (points > MAX_POINTS)
            {
                interval = dr.DefaultInterval(MAX_POINTS);
                points = interval.NumberOfBuckets(dr);
            }

            result.Interval = interval;
            result.ActualDateRange = new QueryDateRange(dr.FromDate, dr.ToDate);
            result.Profile = new decimal?[points];

            var bpd = interval.BucketsPerDay();

            foreach (var siteDay in siteDays.Where(x => x.Date >= fromDate && x.Date <= toDate))
            {

                if (bpd <= 1)
                {
                    var offset = interval.Index(dr.FromDate, siteDay.Date);
                    if (offset < 0 || offset >= points)
                    {
                        continue;
                    }

                    result.Profile[offset] ??= 0;
                    result.Profile[offset] += siteDay.EnergyDailySummary.TotalNetActivePower_kWh;
                }
                else
                {

                    var quads = siteDay.GetEnergyQuadrants(new QuadrantOptions() { Interval = interval.IntervalInMinutes, UseSimpleIntervalCorrection = false });
                    foreach (var quad in quads)
                    {
                        var offset = interval.Index(dr.FromDate, quad.ReadingDateTime);
                        if (offset < 0 || offset >= points)
                        {
                            continue;
                        }
                        result.Profile[offset] ??= 0;
                        result.Profile[offset] += quad.NetActivePower_kWh;
                    }

                }
            }
            return result;
        }



        public static HeatMapResult GetHeatMapConsumption(DateTime fromDate, DateTime toDate, IEnumerable<SiteDay> siteDays, TimeInterval? requestedInterval = null)
        {
            // ensure no more than 5 years of data can be requested  
            var dr = new QueryDateRange(fromDate, toDate).TrimFuture().TrimYears(5);
            if (!dr.IsValid)
            {
                throw new ArgumentException("Invalid Date Range");
            }
            var result = new HeatMapResult(dr);
            var days = siteDays.Where(x => x.Date >= fromDate && x.Date <= toDate).ToList();
            var minInterval = days.Count == 0 ? 30 : days.SelectMany(x => x.Channels.Values).Select(x => x.IntervalMinutes).Where(x => x >= 1 && x <= 24 * 60).Min();
            if (minInterval < 1)
            {
                minInterval = 30;
            }

            var interval = requestedInterval ?? new TimeInterval(TimeIntervalSize.Minute, minInterval);
            if (interval.IntervalInMinutes > 30)
            {
                interval = new TimeInterval(TimeIntervalSize.Minute, 30);
            }

            var points = interval.NumberOfBuckets(dr);


            DateTime minDate = days.Min(x => x.Date);
            DateTime maxDate = days.Max(x => x.Date);

            // no data = return an empty time series
            if (days.Count == 0)
            {
                return result;

            }
            // trim the request range to available data if there are too many points  
            if (minDate > dr.FromDate)
            {
                dr = new QueryDateRange(minDate, dr.ToDate);
                points = interval.NumberOfBuckets(dr);
            }
            if (maxDate < dr.ToDate)
            {
                dr = new QueryDateRange(dr.FromDate, maxDate);
                points = interval.NumberOfBuckets(dr);
            }

            result.Interval = interval;
            result.ActualDateRange = new QueryDateRange(dr.FromDate, dr.ToDate);


            int numberOfPeriods = interval.BucketsPerDay();

            result.Times = new TimeOnly[numberOfPeriods];
            result.Dates = new DateOnly[dr.TotalDays];
            result.Profile = new decimal[numberOfPeriods, dr.TotalDays];
            for (int i = 0; i < numberOfPeriods; i++)
            {
                result.Times[i] = new TimeOnly(0, 0).AddMinutes(i * interval.IntervalInMinutes);
            }
            for (int i = 0; i < dr.TotalDays; i++)
            {
                var dt = dr.FromDate.AddDays(i);

                result.Dates[i] = new DateOnly(dt.Year, dt.Month, dt.Day);
            }

            var dayInterval = new TimeInterval(TimeIntervalSize.Day, 1);

            var bpd = interval.BucketsPerDay();

            foreach (var siteDay in siteDays.Where(x => x.Date >= fromDate && x.Date <= toDate))
            {
                var dayOffset = dayInterval.Index(dr.FromDate, siteDay.Date);
                if (dayOffset < 0 || dayOffset >= dr.TotalDays)
                {
                    continue;
                }
                var quads = siteDay.GetEnergyQuadrants(new QuadrantOptions() { Interval = interval.IntervalInMinutes, UseSimpleIntervalCorrection = true });


                foreach (var quad in quads)
                {
                    var timeOffset = interval.Index(quad.ReadingDateTime.Date, quad.ReadingDateTime);
                    if (timeOffset < 0 || timeOffset >= numberOfPeriods)
                    {
                        continue;
                    }
                    result.Profile[timeOffset, dayOffset] += quad.NetActivePower_kWh;
                }
            }
            return result;
        }





        public static decimal CalculateMedian(decimal[] array)
        {
            if (array.Length == 0) return 0;
            Array.Sort(array);
            int length = array.Length;
            int middleIndex = length / 2;

            if (length % 2 == 0)
            {
                decimal median = (array[middleIndex - 1] + array[middleIndex]) / 2;
                return median;
            }
            else
            {
                decimal median = array[middleIndex];
                return median;
            }
        }






    }




}
