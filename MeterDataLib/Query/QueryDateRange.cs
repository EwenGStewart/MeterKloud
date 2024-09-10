namespace MeterDataLib.Query
{
    public record QueryDateRange(DateTime FromDate, DateTime ToDate)
    {
        public bool IsEmpty => FromDate == DateTime.MinValue.Date && ToDate == DateTime.MaxValue.Date;
        public bool IsValid => FromDate <= ToDate && FromDate.Date == FromDate && ToDate.Date == ToDate;

        public DateTime ExclusiveToDate => ToDate.Date < DateTime.MaxValue.Date ? ToDate.AddDays(1) : DateTime.MaxValue;
        public DateTime InclusiveToDate => ExclusiveToDate.AddMinutes(-1);

        public int TotalDays => (int)ToDate.Subtract(FromDate).TotalDays+1;

        public int TotalYears => ToDate.Year - FromDate.Year + 1;


        public override string ToString()
        {
            return $"{FromDate:yyyy-MM-dd} to {ToDate:yyyy-MM-dd}";
        }


        public static QueryDateRange Empty => new(DateTime.MinValue.Date, DateTime.MaxValue.Date);
        public static QueryDateRange FromText(string fromdate , string todate ) => new( DateTime.Parse(fromdate), DateTime.Parse(todate));

        public static QueryDateRange RelativeRange( RelativeDateRanges relativeDateRange)
        {
            switch (relativeDateRange)
            {
                case RelativeDateRanges.Yesterday:
                    return new QueryDateRange(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(-1));

                case RelativeDateRanges.Last7days:
                    return new QueryDateRange(DateTime.Today.AddDays(-7), DateTime.Today.AddDays(-1));


                case RelativeDateRanges.Last30days:
                    return new QueryDateRange(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(-1));
                case RelativeDateRanges.Last90days:
                    return new QueryDateRange(DateTime.Today.AddDays(-90), DateTime.Today.AddDays(-1));





                case RelativeDateRanges.LastWeek:
                    {
                        var start = DateTime.Today.AddDays(-1 * (int)DateTime.Today.DayOfWeek).AddDays(-7);
                        var end = start.AddDays(6);
                        return new QueryDateRange(start, end);
                    }
                case RelativeDateRanges.LastMonth:
                    {
                        var start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1);
                        var end = start.AddMonths(1).AddDays(-1);
                        return new QueryDateRange(start, end);
                    }

                case RelativeDateRanges.Last3Months:
                    {
                        var start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-3);
                        var end = start.AddMonths(3).AddDays(-1);
                        return new QueryDateRange(start, end);
                    }
                case RelativeDateRanges.Last6Months:
                    {
                        var start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-6);
                        var end = start.AddMonths(6).AddDays(-1);
                        return new QueryDateRange(start, end);
                    }

                case RelativeDateRanges.Last12Months:
                    {
                        var start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-12);
                        var end = start.AddMonths(12).AddDays(-1);
                        return new QueryDateRange(start, end);
                    }



                case RelativeDateRanges.LastQuarter:
                    {
                        var mth = (DateTime.Today.Month - 1) / 3 * 3 + 1;
                        var start = new DateTime(DateTime.Today.Year, mth, 1).AddMonths(-3);
                        var end = start.AddMonths(3).AddDays(-1);
                        return new QueryDateRange(start, end);
                    }
                case RelativeDateRanges.LastYear:
                    {
                        var year = (DateTime.Today.Year - 1);
                        var start = new DateTime(year, 1, 1);
                        var end = start.AddYears(1).AddDays(-1);
                        return new QueryDateRange(start, end);
                    }
                case RelativeDateRanges.Today:
                    return new QueryDateRange(DateTime.Today, DateTime.Today);
                case RelativeDateRanges.ThisWeek:
                    {
                        var start = DateTime.Today.AddDays(-1 * (int)DateTime.Today.DayOfWeek);
                        var end = start.AddDays(6);
                        return new QueryDateRange(start, end);
                    }

                case RelativeDateRanges.ThisMonth:
                    {
                        var start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                        var end = start.AddMonths(1).AddDays(-1);
                        return new QueryDateRange(start, end);
                    }

                case RelativeDateRanges.ThisQuarter:
                    {
                        var mth = (DateTime.Today.Month - 1) / 3 * 3 + 1;
                        var start = new DateTime(DateTime.Today.Year, mth, 1);
                        var end = start.AddMonths(3).AddDays(-1);
                        return new QueryDateRange(start, end);
                    }
                case RelativeDateRanges.ThisYear:
                    {
                        var year = (DateTime.Today.Year);
                        var start = new DateTime(year, 1, 1);
                        var end = start.AddYears(1).AddDays(-1);
                        return new QueryDateRange(start, end);
                    }

                case RelativeDateRanges.AllTime:
                    return new QueryDateRange(DateTime.MinValue, DateTime.MaxValue);


                case RelativeDateRanges.LastYearDays:
                    {
                        var start = new DateTime(DateTime.Today.Year - 1, DateTime.Today.Month, DateTime.Today.Day).AddDays(-1);
                        var end = DateTime.Today.AddDays(-1);
                        return new QueryDateRange(start, end);
                    }

                case RelativeDateRanges.Last5YearsDays:
                    {
                        var start = new DateTime(DateTime.Today.Year - 5, DateTime.Today.Month, DateTime.Today.Day).AddDays(-1);
                        var end = DateTime.Today.AddDays(-1);
                        return new QueryDateRange(start, end);
                    }
                case RelativeDateRanges.Last13Months:
                    {
                        var start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-13);
                        var end = start.AddMonths(13).AddDays(-1);
                        return new QueryDateRange(start, end);
                    }

                default:
                    throw new InvalidOperationException();
            }
        }


        public QueryDateRange TrimYears(int maxYears) 
        {
            if ( maxYears < 1) { throw new ArgumentException("maxYears must be greater than 0"); }
            if (TotalYears <= maxYears)
                return this;
            DateTime from = FromDate; 
            if ( FromDate.Year < DateTime.Today.Year - maxYears +  1)
            {
                return new QueryDateRange(DateTime.Today.AddYears(-maxYears), ToDate);
            }
            else
            {
                return new QueryDateRange(from , from.AddYears(maxYears).AddDays(-1));
            }
        }

        public QueryDateRange TrimFuture()
        {
            
            if (ToDate <= DateTime.Today)
                return this;
            DateTime from = FromDate;
            if (FromDate > DateTime.Today)
            {
                return new QueryDateRange(DateTime.Today, DateTime.Today);
            }
            else
            {
                return new QueryDateRange(from, DateTime.Today);
            }
        }





        static readonly TimeInterval[] defaultIntervals =
        [
            
            new TimeInterval(TimeIntervalSize.Minute, 5),
            new TimeInterval(TimeIntervalSize.Minute, 15),
            new TimeInterval(TimeIntervalSize.Minute, 30),
            new TimeInterval(TimeIntervalSize.Hour, 1),
            new TimeInterval(TimeIntervalSize.Hour, 4),
            new TimeInterval(TimeIntervalSize.Day, 1),
            new TimeInterval(TimeIntervalSize.Week, 1),
            new TimeInterval(TimeIntervalSize.Month, 1),
            new TimeInterval(TimeIntervalSize.Month, 3),
            new TimeInterval(TimeIntervalSize.Year, 1),
            new TimeInterval(TimeIntervalSize.Year, 10),
        ];


        public TimeInterval DefaultInterval( int maxPoints )
        {
            if (maxPoints < 1) { throw new ArgumentException("maxPoints must be greater than 0"); } 
            for ( int i = 0; i < defaultIntervals.Length; i++)
            {
                if (defaultIntervals[i].NumberOfBuckets(this) <= maxPoints)
                    return defaultIntervals[i];
            }
            return defaultIntervals[defaultIntervals.Length - 1];
        }

    }

}
