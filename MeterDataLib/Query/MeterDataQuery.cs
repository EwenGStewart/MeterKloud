using System.Runtime.InteropServices.Marshalling;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MeterDataLib.Query
{
    public static class MeterDataQuery 
    {


        public class DailyConsumptionProfileResult ( QueryDateRange queryDateRange)
        {
            public QueryDateRange RequestedDateRange { get; set; } = queryDateRange;
            public QueryDateRange ActualDateRange { get; set; } = queryDateRange;

            public int Days => ActualDateRange.TotalDays;
 
            public int Missing => Profile.Where(x => !x.HasValue).Count();


            public bool IsEmpty => Profile.Length == 0 || Profile.All(x => !x.HasValue);


            public TimeInterval Interval { get; set; } = new TimeInterval(TimeIntervalSize.Day, 1);

            public DateTime[] Dates => Interval.Range(ActualDateRange);

            public IList<object> X  =>  Array.ConvertAll<DateTime, object>(Dates, v=> (object) v);
            public decimal?[] Profile { get; set; } = [];


            public IList<object> Y => Array.ConvertAll<decimal?, object>(Profile, v => (object)v!  );

            public Quality[] Quality { get; set; } = [];

            public decimal MaxValue => Profile.Where(x=>x.HasValue).Select(x=>x!.Value).Max();

            public DateTime MaxDay => Profile.Select( (x,i) => new { Value = x, Date = Dates[i] }).Where(x => x.Value.HasValue).OrderByDescending(x => x.Value).FirstOrDefault()?.Date??ActualDateRange.FromDate;
       

            public decimal MinValue => Profile.Where(x => x.HasValue).Select(x => x!.Value).Min();
            public DateTime MinDay => Dates[Profile.Select((value, i) => (value, i)).Where(x => x.value.HasValue).OrderBy(x => x.value).First().i];


            public decimal AvgDay => Profile.Where(x => x.HasValue).Select(x => x!.Value).Average();
            
            public decimal Median => CalculateMedian( Profile.Where(x => x.HasValue).Select(x => x!.Value).ToArray()) ;

        }



        public class DetailedConsumptionProfileResult(QueryDateRange queryDateRange)
        {
            public QueryDateRange RequestedDateRange { get; set; } = queryDateRange;
            public QueryDateRange ActualDateRange { get; set; } = queryDateRange;

            public int Days => ActualDateRange.TotalDays;

            public int Points => Interval.NumberOfBuckets(ActualDateRange);

       
            public int Missing  => Profile.Where(x => !x.HasValue).Count();

            public TimeInterval Interval { get; set; } = new TimeInterval(TimeIntervalSize.Day, 1);

            public DateTime[] Dates => Interval.Range(ActualDateRange);

            public IList<object> X => Array.ConvertAll<DateTime, object>(Dates, v => (object)v);

            public decimal?[] Profile { get; set; } = [];

            public IList<object> Y => Array.ConvertAll<decimal?, object>(Profile, v => (object)v!);



            public decimal MaxValue => Profile.Where(x => x.HasValue).Select(x => x!.Value).Max();

            public DateTime MaxDateTime => Dates[ Profile.Select( (value, i)=> (value,i)).Where(x => x.value.HasValue).OrderByDescending(x=>x.value).First().i];

            public decimal MinValue => Profile.Where(x => x.HasValue).Select(x => x!.Value).Min();

            public DateTime MinDateTime => Dates[Profile.Select((value, i) => (value, i)).Where(x => x.value.HasValue).OrderBy(x => x.value).First().i];
            public decimal AvgDay => Profile.Where(x => x.HasValue).Select(x => x!.Value).Average();

            public decimal Median => CalculateMedian(Profile.Where(x => x.HasValue).Select(x => x!.Value).ToArray());

        }






        const int IDEAL_POINTS = 500;
        const int MAX_POINTS = 100 * 288;


        public static DailyConsumptionProfileResult GetDailyNetConsumption(DateTime fromDate, DateTime toDate , IEnumerable<SiteDay> siteDays)
        {
            var dr = new QueryDateRange(fromDate, toDate).TrimYears(5).TrimFuture();
            if ( ! dr.IsValid)
            {
                throw new ArgumentException("Invalid Date Range");
            }
            var result = new DailyConsumptionProfileResult(dr);


            var interval = new TimeInterval(TimeIntervalSize.Day, 1);
            int days = result.Days;


            result.Profile  = new decimal?[days];
            result.Quality = new Quality[days];
            Array.Fill(result.Quality, Quality.Missing);

            result.Interval = interval;
            result.ActualDateRange = new QueryDateRange(dr.FromDate, dr.ToDate);
            
            
            foreach (var siteDay in siteDays.Where(x=>x.Date >= fromDate && x.Date <= toDate))
            {
                
                var offset = interval.Index( dr.FromDate , siteDay.Date);
                if ( offset < 0 || offset >= days)
                {
                    continue;
                }

                result.Profile[offset] ??= 0;
                result.Profile[offset] += siteDay.EnergyDailySummary.TotalNetActivePower_kWh;
                result.Quality[offset] =  siteDay.EnergyDailySummary.Quality;
            }
            return result; 
        }


        public static DetailedConsumptionProfileResult GetDetailedNetConsumption(DateTime fromDate, DateTime toDate, IEnumerable<SiteDay> siteDays,  TimeInterval? requestedInterval=null)
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
            if (points > MAX_POINTS  )
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

                    var quads  = siteDay.GetEnergyQuadrants( new QuadrantOptions() {  Interval = interval.IntervalInMinutes, UseSimpleIntervalCorrection = false });    
                    foreach( var quad in quads  )
                    {
                        var offset = interval.Index(dr.FromDate , quad.ReadingDateTime);
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
