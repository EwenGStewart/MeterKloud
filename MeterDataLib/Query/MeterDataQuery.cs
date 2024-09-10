using Microsoft.VisualBasic;
using System.Runtime.InteropServices.Marshalling;
using System.Security.Cryptography.X509Certificates;
using static MeterDataLib.Query.MeterDataQuery;
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
                        _z = [];
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


            public IList<object> XReverse => X.Reverse().ToList();
            public IList<object> YReverse => Y.Reverse().ToList();
            public IList<object> ZReverse { get
                {
                    var z = Z.Reverse().ToList();
                    foreach (var item in z)
                    {
                        if (item is decimal[] values)
                        {
                            Array.Reverse(values);
                        }
                    }
                    return z;
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







        public class DailyDemandResult(QueryDateRange queryDateRange)
        {
            public QueryDateRange RequestedDateRange { get; set; } = queryDateRange;
            public QueryDateRange ActualDateRange { get; set; } = queryDateRange;

            public int Days => ActualDateRange.TotalDays;

            public TimeInterval Interval { get; set; } = new TimeInterval(TimeIntervalSize.Day, 1);

            public DateTime[] Dates => Interval.Range(ActualDateRange);


            public int Points => Interval.NumberOfBuckets(ActualDateRange);

            public decimal?[] Demand_kW{ get; set; } = [];
            public decimal?[] Demand_kW_atMax_kVA { get; set; } = [];
            public decimal?[] Demand_kVA { get; set; } = [];
            public decimal?[] Demand_Pf { get; set; } = [];
            public DateTime?[] TimeOfMaxKva { get; set; } = [];

            public bool AnyKva()
            {
                for (int i = 0; i < Demand_kVA.Length; i++)
                {
                    if (Demand_kVA[i].HasValue && Math.Round(Demand_kVA[i] ?? 0, 3) > Math.Round(Demand_kW[i] ?? 0, 3))
                    {
                        return true;
                    }
                }
                return false;
            }



            public int Missing => Demand_kW.Where(x => !x.HasValue).Count();

            public IList<object> X => Array.ConvertAll<DateTime, object>(Dates, v => (object)v);
            public IList<object> Y_kW => Array.ConvertAll<decimal?, object>(Demand_kW, v => (object)v!);
            public IList<object> Y_kVA_Diff()
            {
                var result = new List<object>();
                for (int i = 0; i < Demand_kVA.Length; i++)
                {
                    if (Demand_kVA[i].HasValue && Demand_kW[i].HasValue)
                    {
                        result.Add( Math.Round( Demand_kVA[i]!.Value - Demand_kW[i]!.Value,3));
                    }
                    else
                    {
                        result.Add(0);
                    }
                }
                return result;
            }

            public IList<object> Y_Pf => Array.ConvertAll<decimal?, object>(Demand_Pf, v => (object)v!); 
            public string[] DemandLabels()
            {
                var result = new string[Points];
                if (AnyKva())
                {
                    for (int i = 0; i < Points; i++)
                    {
                        if (Demand_kVA.Length > i &&  Demand_kVA[i].HasValue )
                        {
                            decimal kva = Math.Round(Demand_kVA[i] ?? 0, 3);
                            decimal kw = Math.Round(Demand_kW[i] ?? 0, 3);
                            decimal kwAtMax = Math.Round(Demand_kW_atMax_kVA[i] ?? 0, 3);
                            decimal pf = Math.Round(Demand_Pf[i] ?? 1, 3);
                            DateTime dateAndTime = TimeOfMaxKva[i] ?? DateTime.MinValue;
                            if (kwAtMax != kw)
                            {
                                result[i] = $"kVA:{kva:0.000}<br>kW:{kw:0.000}<br>kW at Max kVA:{kwAtMax:0.000}<br>PF:{pf:0.000}<br>{dateAndTime:dd-MMM-yy}<br>Time:{dateAndTime:HH:mm}";
                            }
                            else
                            {
                                result[i] = $"kVA:{kva:0.000}<br>kW:{kw:0.000}<br>PF:{pf:0.000}<br>{dateAndTime:dd-MMM-yy}<br>Time:{dateAndTime:HH:mm}";
                            }
                        }
                        else
                        {
                            result[i] = "No Data";
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < Points; i++)
                    {
                        if( Demand_kW.Length > i &&  Demand_kW[i].HasValue )
                        {
                            decimal kw = Math.Round(Demand_kW[i] ?? 0, 3);
                            DateTime dateAndTime = TimeOfMaxKva[i] ?? DateTime.MinValue;
                            result[i] = $"kW:{kw:0.000}<br>{dateAndTime:dd-MMM-yy}<br>Time:{dateAndTime:HH:mm}";
                        }
                        else
                        {
                            result[i] = "No Data";
                        }
                    }

                }
                return result;
            }



         
            public bool IsEmpty => Demand_kW.Length == 0 || Demand_kW.All(x => !x.HasValue);

            public decimal Max_kVA_Value => Demand_kVA.Where(x => x.HasValue).Select(x => x!.Value).Max();
            public DateTime Max_kVA_DateTime { 
                get 
                {
                    if (Demand_kVA.Length <= 0) return ActualDateRange.FromDate;
                    var top = Demand_kVA.Select((value, i) => (value, i)).OrderByDescending(x => x.value).FirstOrDefault();
                    if (top.value.HasValue)
                    {
                        if (top.i < 0 || top.i >= Dates.Length)
                        {
                            return ActualDateRange.FromDate;
                        }
                        return Dates[top.i];
                    }
                    return ActualDateRange.FromDate;
                } 
            }

            public decimal Max_kW_Value => Demand_kW.Where(x => x.HasValue).Select(x => x!.Value).Max();
            public DateTime Max_kW_DateTime => Dates[Demand_kVA.Select((value, i) => (value, i)).Where(x => x.value.HasValue).OrderByDescending(x => x.value).First().i];




        }

        public class MonthlyDemandResult
        {
            private readonly DailyDemandResult _dailyBaseData   ;
            public MonthlyDemandResult(DailyDemandResult dailyDemandResult)
            {

                _dailyBaseData = dailyDemandResult;
                RequestedDateRange = dailyDemandResult.RequestedDateRange;
                ActualDateRange = dailyDemandResult.ActualDateRange;
                Demand_kVA = new decimal[Points];
                Demand_kW = new decimal[Points];
                Demand_Pf = new decimal[Points];
                TimeOfMaxKva = new DateTime[Points];
                Demand_kW_atMax_kVA = new decimal[Points];

          

                foreach (var date in Dates)
                {
                    DateTime endDate = date.AddMonths(1).AddDays(-1);
                    var offset = Interval.Index(ActualDateRange.FromDate, date);
                    int startDay = dailyDemandResult.Interval.Index(ActualDateRange.FromDate, date);
                    int endDay = dailyDemandResult.Interval.Index(ActualDateRange.FromDate, endDate);

               
                    if (offset < 0 || offset >= Points)
                    {
                        continue;
                    }
                    if (startDay < 0)
                    {
                        startDay = 0;
                    }
                    if (endDay >= dailyDemandResult.Points)
                    {
                        endDay = dailyDemandResult.Points - 1;
                    }
                    for ( int i = startDay; i <= endDay; i++)
                    {
                        if (dailyDemandResult.Demand_kVA[i].HasValue)
                        {
                            if (Demand_kVA[offset] == 0 || dailyDemandResult.Demand_kVA[i] > Demand_kVA[offset])
                            {
                                Demand_kVA[offset] = dailyDemandResult.Demand_kVA[i] ?? 0;
                                Demand_kW_atMax_kVA[offset] = dailyDemandResult.Demand_kW_atMax_kVA[i] ?? 0;
                                Demand_Pf[offset] = dailyDemandResult.Demand_Pf[i] ?? 0;
                                TimeOfMaxKva[offset] = dailyDemandResult.TimeOfMaxKva[i] ?? date;
                            }
                        }
                        if (dailyDemandResult.Demand_kW[i].HasValue)
                        {
                            if (Demand_kW[offset] == 0 || dailyDemandResult.Demand_kW[i] > Demand_kW[offset])
                            {
                                Demand_kW[offset] = dailyDemandResult.Demand_kW[i] ?? 0;
                            }
                        }
                    }
                }
            }
            public QueryDateRange RequestedDateRange { get; init; }
            public QueryDateRange ActualDateRange { get; init; }

            

            public TimeInterval Interval { get; init; } = new TimeInterval(TimeIntervalSize.Month, 1);

            public int Points => Interval.NumberOfBuckets(ActualDateRange);

            public DateTime[] Dates => Interval.Range(ActualDateRange);
            public decimal[] Demand_kW { get; set; } = [];
            public decimal[] Demand_kW_atMax_kVA { get; set; } = [];
            public decimal[] Demand_kVA { get; set; } = [];
            public decimal[] Demand_Pf { get; set; } = [];
            public DateTime[] TimeOfMaxKva { get; set; } = [];

            public bool AnyKva()
            {
                for (int i = 0; i < Demand_kVA.Length; i++)
                {
                    if (Math.Round(Demand_kVA[i] , 3) > Math.Round(Demand_kW[i] , 3))
                    {
                        return true;
                    }
                }
                return false;
            }

            public IList<object> X => Array.ConvertAll<DateTime, object>(Dates, v => (object)v.ToString("MMM-yy")) ;
            public IList<object> Y_kW => Array.ConvertAll<decimal, object>(Demand_kW, v => (object)v!);
            public IList<object> Y_kVA_Diff()
            {
                var result = new List<object>();
                for (int i = 0; i < Demand_kVA.Length; i++)
                {
                        result.Add(Math.Round(Demand_kVA[i] - Demand_kW[i], 3));
                }
                return result;
            }

            public IList<object> Y_Pf => Array.ConvertAll<decimal, object>(Demand_Pf, v => (object)v!);
            public string[] DemandLabels()
            {
                var result = new string[Points];
                if (AnyKva())
                {
                    for (int i = 0; i < Points; i++)
                    {
                       
                            decimal kva = Math.Round(Demand_kVA[i]  , 3);
                            decimal kw = Math.Round(Demand_kW[i]  , 3);
                            decimal kwAtMax = Math.Round(Demand_kW_atMax_kVA[i] , 3);
                            decimal pf = Math.Round(Demand_Pf[i]  , 3);
                            DateTime dateAndTime = TimeOfMaxKva[i];
                            if (TimeOfMaxKva[i] == DateTime.MinValue)
                            {
                                    result[i] = $"No Data";
                            }
                            else  if (kwAtMax != kw)
                            {
                                result[i] = $"kVA:{kva:0.000}<br>kW:{kw:0.000}<br>kW at Max kVA:{kwAtMax:0.000}<br>PF:{pf:0.000}<br>{dateAndTime:dd-MMM-yy}<br>Time:{dateAndTime:HH:mm}";
                            }
                            else
                            {
                                result[i] = $"kVA:{kva:0.000}<br>kW:{kw:0.000}<br>PF:{pf:0.000}<br>Day:{dateAndTime:dd-MMM-yy}<br>Time:{dateAndTime:HH:mm}";
                            }
                    }
                }
                else
                {
                    for (int i = 0; i < Points; i++)
                    {
                     
                            decimal kw = Math.Round(Demand_kW[i]  , 3);
                            DateTime dateAndTime = TimeOfMaxKva[i]  ;
                            result[i] = $"kW:{kw:0.000}<br>Day:{dateAndTime:dd-MMM-yy}<br>Time:{dateAndTime:HH:mm}";
                            if (TimeOfMaxKva[i] == DateTime.MinValue)
                            {
                                result[i] = $"No Data";
                            }


                    }

                }
                return result;
            }


            public DateTime MonthWithMax {  get
                {
                    if ( Dates.Length <= 0|| Demand_kVA.Length <=  0 ) return ActualDateRange.FromDate;
                    var topIndex = Demand_kVA.Select((value, i) => (value, i)).OrderByDescending(x => x.value).FirstOrDefault();
                    var index = topIndex.i;
                    if (index < 0 || index >= Dates.Length)
                    {
                        return ActualDateRange.FromDate;
                    }
                    return Dates[index];
                } 
            } 


            public DailyDemandResult GetDaily ( DateTime month)
            {
                var offset = Interval.Index(ActualDateRange.FromDate, month);
                if (offset < 0 || offset >= Points)
                {
                    throw new ArgumentException("Invalid Date");
                }

                DateTime date = Dates[offset];
                DateTime endDate = date.AddMonths(1).AddDays(-1);

                if ( date < _dailyBaseData.ActualDateRange.FromDate)
                {
                    date = _dailyBaseData.ActualDateRange.FromDate;
                }
                if (endDate > _dailyBaseData.ActualDateRange.ToDate)
                {
                    endDate = _dailyBaseData.ActualDateRange.ToDate;
                }

                var dateRange = new QueryDateRange(date, endDate);
                var result = new DailyDemandResult(dateRange);
                int startDay = _dailyBaseData.Interval.Index(ActualDateRange.FromDate, date);
                int endDay = _dailyBaseData.Interval.Index(ActualDateRange.FromDate, endDate);
                if (offset < 0 || offset >= Points || startDay < 0 || endDay >= _dailyBaseData.Points)
                {
                    return result;
                }
                result.Interval = _dailyBaseData.Interval;
                result.Demand_kW = new decimal?[result.Points];
                result.Demand_kW_atMax_kVA = new decimal?[result.Points];
                result.Demand_kVA = new decimal?[result.Points];
                result.Demand_Pf = new decimal?[result.Points];
                result.TimeOfMaxKva = new DateTime?[result.Points];
                for (int i = startDay; i <= endDay; i++)
                {
                    int targetIndex = result.Interval.Index(date, _dailyBaseData.Dates[i]);
                    if (targetIndex < 0 || targetIndex >= result.Points || i < 0 || i >= _dailyBaseData.Points)
                    {
                        continue;
                    }
                    result.Demand_kVA[targetIndex] = _dailyBaseData.Demand_kVA[i];
                    result.Demand_kW_atMax_kVA[targetIndex] = _dailyBaseData.Demand_kW_atMax_kVA[i];
                    result.Demand_Pf[targetIndex] = _dailyBaseData.Demand_Pf[i];
                    result.TimeOfMaxKva[targetIndex] = _dailyBaseData.TimeOfMaxKva[i];
                    result.Demand_kW[targetIndex] = _dailyBaseData.Demand_kW[i];
                }
                return result;
            }


            public DailyDemandResult GetMaxMonth()
            {
                return GetDaily(MonthWithMax);
            }


        }

        public class QuadrantResult (QueryDateRange queryDateRange)
        {
            public QueryDateRange RequestedDateRange { get; set; } = queryDateRange;
            public QueryDateRange ActualDateRange { get; set; } = queryDateRange;

            public int Days => ActualDateRange.TotalDays;

            public TimeInterval Interval { get; set; } = new TimeInterval(TimeIntervalSize.Minute, 30);
            public DateTime[] Dates => Interval.Range(ActualDateRange);

            public int Points => Interval.NumberOfBuckets(ActualDateRange);

            public EnergyQuadrant[] Quadrants { get; set; } = [];


            public IList<object> X => Array.ConvertAll<DateTime, object>(Dates, v => (object)v);

            public IList<object> Y_kW => Array.ConvertAll<EnergyQuadrant, object>(Quadrants, v => (object)v.RealPowerConsumption_kW);

            public IList<object> Y_kVA => Array.ConvertAll<EnergyQuadrant, object>(Quadrants, v => (object)v.ApparentPower_kVA);


            public IList<object> Y_kVA_Diff()
            {
                var result = new List<object>();
                for (int i = 0; i < Points; i++)
                {
                    var v = Math.Round(Quadrants[i].ApparentPower_kVA - Quadrants[i].RealPowerConsumption_kW, 3);
                    if ( v < 0 ) { v = 0; }
                    result.Add((object)v);
                }
                return result;
            }


            public bool AnyKva()
            {
                for (int i = 0; i < Quadrants.Length; i++)
                {
                    if ( Math.Round( Quadrants[i].NetReactiveEnergy_kVArh,3)  !=0 )  
                    {
                        return true;
                    }
                }
                return false;
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
                        result.Profile[offset] += quad.NetActiveEnergy_kWh;
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
                    result.Profile[timeOffset, dayOffset] += quad.NetActiveEnergy_kWh;
                }
            }
            return result;
        }



        public static DailyDemandResult GetDailyDemand(DateTime fromDate, DateTime toDate, IEnumerable<SiteDay> siteDays, int demandIntervalMinutes = 30 )
        {

            var dr = new QueryDateRange(fromDate, toDate).TrimYears(5).TrimFuture();
            if (!dr.IsValid)
            {
                throw new ArgumentException("Invalid Date Range");
            }
            if (demandIntervalMinutes < 1 || demandIntervalMinutes > 60)
            {
                throw new ArgumentException("Invalid Demand Interval - must be from 1-60 minutes");
            }

            var result = new DailyDemandResult(dr);
            var resultInterval = new TimeInterval(TimeIntervalSize.Day, 1);
            result.Interval = resultInterval;
            var demandInterval = new TimeInterval(TimeIntervalSize.Minute, demandIntervalMinutes);
            var days = siteDays.Where(x => x.Date >= fromDate && x.Date <= toDate).ToList();
            DateTime minDate = days.Min(x => x.Date);
            DateTime maxDate = days.Max(x => x.Date);

            // no data = return an empty time series
            if (days.Count == 0)
            {
                return result;
            }
            
            if ( minDate > dr.FromDate)
            {
                dr = new QueryDateRange(minDate, dr.ToDate);
            }
            if (maxDate < dr.ToDate)
            {
                dr = new QueryDateRange(dr.FromDate, maxDate);
            }
            if (!dr.IsValid)
            {
                throw new ArgumentException("Invalid Date Range - after trimming for available data ");
            }
            result.ActualDateRange = dr; 

            int points = result.Points;

            result.Demand_kW = new decimal?[points];
            result.Demand_kVA = new decimal?[points];
            result.Demand_Pf = new decimal?[points];
            result.TimeOfMaxKva = new DateTime?[points];
            result.Demand_kW_atMax_kVA = new decimal?[points];
            var options = new EnergyDailySummaryOptions() { CalculateKvaAtMaxKw = false, Interval = demandInterval.IntervalInMinutes };


            foreach (var siteDay in siteDays.Where(x => x.Date >= fromDate && x.Date <= toDate))
            {
                var dayOffset = resultInterval.Index(dr.FromDate, siteDay.Date);
                if (dayOffset < 0 || dayOffset >= dr.TotalDays)
                {
                    continue;
                }
                var siteDayDemand = siteDay.GetDailySummary(options);
                result.Demand_kW[dayOffset] = siteDayDemand.Max_kW;
                result.Demand_kVA[dayOffset] = siteDayDemand.Max_kVA;
                result.Demand_Pf[dayOffset] = siteDayDemand.PowerFactor;
                result.TimeOfMaxKva[dayOffset] = siteDayDemand.TimeOfMax;
                result.Demand_kW_atMax_kVA[dayOffset] = siteDayDemand.Max_Kw_atMax_kVa;
            }

            return result; 

        }


        public static QuadrantResult GetQuadrants(DateTime fromDate, DateTime toDate, IEnumerable<SiteDay> siteDays, int demandIntervalMinutes = 30)
        {
            var dr = new QueryDateRange(fromDate, toDate).TrimYears(5).TrimFuture();
            if (!dr.IsValid)
            {
                throw new ArgumentException("Invalid Date Range");
            }
            if (demandIntervalMinutes < 1 || demandIntervalMinutes > SiteDay.MinutesPerDay)
            {
                throw new ArgumentException("Invalid Demand Interval");
            }
            var result = new QuadrantResult(dr);
            var demandInterval = new TimeInterval(TimeIntervalSize.Minute, demandIntervalMinutes);
            var days = siteDays.Where(x => x.Date >= fromDate && x.Date <= toDate).ToList();
            DateTime minDate = days.Min(x => x.Date);
            DateTime maxDate = days.Max(x => x.Date);

            // no data = return an empty time series
            if (days.Count == 0)
            {

                return result;
            }

            if (minDate > dr.FromDate)
            {
                dr = new QueryDateRange(minDate, dr.ToDate);
            }
            if (maxDate < dr.ToDate)
            {
                dr = new QueryDateRange(dr.FromDate, maxDate);
            }
            if (!dr.IsValid)
            {
                throw new ArgumentException("Invalid Date Range - after trimming for available data ");
            }
            result.ActualDateRange = dr;
            int points = result.Points;
            result.Quadrants = new EnergyQuadrant[points];
            foreach (var siteDay in days)
            {
                var quads = siteDay.GetEnergyQuadrants(new QuadrantOptions() { Interval = demandInterval.IntervalInMinutes, UseSimpleIntervalCorrection = true });
                foreach (var quad in quads)
                {
                    var offset = demandInterval.Index(dr.FromDate, quad.ReadingDateTime);
                    if (offset < 0 || offset >= points)
                    {
                        continue;
                    }
                    result.Quadrants[offset] = quad;
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
