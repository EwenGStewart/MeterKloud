namespace MeterDataLib.Query
{
    public record TimeInterval(TimeIntervalSize Interval = TimeIntervalSize.Minute, int Size = 1)
    {
        public const int DayMinutes = 60 * 24;
        // important must be a Sunday 
        static readonly DateTime BaseMinuteDate = new(2017, 1, 1, 0, 0, 0);
        static readonly int BaseYearMth = BaseMinuteDate.Year * 12;
        static readonly int BaseYear = BaseMinuteDate.Year;


        public bool IsValid => Size > 0 && Enum.IsDefined(Interval) &&
         (
             (Interval == TimeIntervalSize.Minute && Size <= DayMinutes && (DayMinutes % Size) == 0) ||
             (Interval == TimeIntervalSize.Hour && Size <= 24) ||
             (Interval == TimeIntervalSize.Day && Size <= 100) ||
             (Interval == TimeIntervalSize.Week && Size <= 50) ||
             (Interval == TimeIntervalSize.Month && Size <= 36) ||
             (Interval == TimeIntervalSize.Qtr && Size <= 12) ||
             (Interval == TimeIntervalSize.Year && Size <= 3)
         );



        public int OffSet(DateTime dateTime)
        {
            if (!IsValid)
            {
                throw new InvalidOperationException("Invalid TimeInterval");
            }
            switch (Interval)
            {
                case TimeIntervalSize.Minute:
                    {
                        return ((int)dateTime.Subtract(BaseMinuteDate).TotalMinutes) / Size;

                    }
                case TimeIntervalSize.Hour:
                    {
                        return ((int)dateTime.Subtract(BaseMinuteDate).TotalHours) / Size;
                    }
                case TimeIntervalSize.Day:
                    {
                        return ((int)dateTime.Subtract(BaseMinuteDate).TotalDays) / Size;

                    }
                case TimeIntervalSize.Week:
                    {
                        var weekSize = 7 * Size;
                        return ((int)dateTime.Subtract(BaseMinuteDate).TotalDays) / weekSize;
                    }
                case TimeIntervalSize.Month:
                    {
                        int mthYr = dateTime.Year * 12 + (dateTime.Month - 1);
                        return (mthYr - BaseYearMth) / Size;
                    }
                case TimeIntervalSize.Qtr:
                    {
                        var qtrSize = 3 * Size;
                        int mthYr = dateTime.Year * 12 + (dateTime.Month - 1);
                        return (mthYr - BaseYearMth) / qtrSize;

                    }

                case TimeIntervalSize.Year:
                    {
                        int yr = dateTime.Year;
                        return (yr - BaseYear) / Size;
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public int Index( DateTime start, DateTime target)
        {
            var startOffset = OffSet(start);
            var endOffset = OffSet(target);
            return endOffset - startOffset;
        }

        public DateTime Bucket(DateTime dateTime)
        {
            int offset = OffSet(dateTime);

            switch (Interval)
            {
                case TimeIntervalSize.Minute:
                    {

                        return BaseMinuteDate.AddMinutes(offset * Size);
                    }
                case TimeIntervalSize.Hour:
                    {

                        return BaseMinuteDate.AddHours(offset * Size);
                    }
                case TimeIntervalSize.Day:
                    {

                        return BaseMinuteDate.AddDays(offset * Size);
                    }
                case TimeIntervalSize.Week:
                    {
                        return BaseMinuteDate.AddDays(offset * Size * 7);
                    }
                case TimeIntervalSize.Month:
                    {
                        return BaseMinuteDate.AddMonths(offset * Size);
                    }
                case TimeIntervalSize.Qtr:
                    {

                        return BaseMinuteDate.AddMonths(offset * Size * 3);
                    }

                case TimeIntervalSize.Year:
                    {
                        return BaseMinuteDate.AddYears(offset * Size);
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public DateTime Add(DateTime from, int buckets = 1)
        {
            if (!IsValid)
            {
                throw new InvalidOperationException("Invalid TimeInterval");
            }
            if (buckets < 0)
            {
                throw new InvalidOperationException("Invalid Buckets");
            }
            from = Bucket(from);
            switch (Interval)
            {
                case TimeIntervalSize.Minute:
                    return from.AddMinutes(buckets * Size);
                case TimeIntervalSize.Hour:
                    return from.AddHours(buckets * Size);
                case TimeIntervalSize.Day:
                    return from.AddDays(buckets * Size);
                case TimeIntervalSize.Week:
                    return from.AddDays(buckets * 7 * Size);
                case TimeIntervalSize.Month:
                    return from.AddMonths(buckets * Size);
                case TimeIntervalSize.Qtr:
                    return from.AddMonths(buckets * 3 * Size);
                case TimeIntervalSize.Year:
                    return from.AddYears(buckets * Size);
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }


        public bool DayOrMore()
        {
            switch (Interval)
            {
                case TimeIntervalSize.Day:
                case TimeIntervalSize.Week:
                case TimeIntervalSize.Month:
                case TimeIntervalSize.Qtr:
                case TimeIntervalSize.Year:
                    return true;
                case TimeIntervalSize.Minute:
                    return Size >= 60 * 24;
                case TimeIntervalSize.Hour:
                    return Size >= 24;
                default:
                    return true;
            }
        }


        public int BucketsPerDay()
        {
            if (!IsValid)
            {
                throw new InvalidOperationException("Invalid TimeInterval");
            }
            switch (Interval)
            {
                case TimeIntervalSize.Minute:
                    return DayMinutes / Size;
                case TimeIntervalSize.Hour:
                    return 24 / Size;
                case TimeIntervalSize.Day:
                    return 1;
                case TimeIntervalSize.Week:
                    return 1;
                case TimeIntervalSize.Month:
                    return 1;
                case TimeIntervalSize.Qtr:
                    return 1;
                case TimeIntervalSize.Year:
                    return 1;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public int IntervalInMinutes => Interval switch
        {
            TimeIntervalSize.Minute => Size,
            TimeIntervalSize.Hour => Size * 60,
            TimeIntervalSize.Day => Size * 60 * 24,
            TimeIntervalSize.Week => Size * 60 * 24 * 7,
            TimeIntervalSize.Month => Size * 60 * 24 * 30,
            TimeIntervalSize.Qtr => Size * 60 * 24 * 30 * 3,
            TimeIntervalSize.Year => Size * 60 * 24 * 365,
            _ => throw new ArgumentOutOfRangeException()
        };


        public bool Equivalent(TimeInterval other)
        {
            if ( Interval == other.Interval && Size == other.Size) return true;
            int leftMins = -1;
            int rightMins = -2;
            switch (Interval)
            {
                case TimeIntervalSize.Minute:
                    leftMins = Size;
                    break;
                case TimeIntervalSize.Hour:
                    leftMins = Size * 60;
                    break;
                case TimeIntervalSize.Day:
                    leftMins = Size * 60 * 24;
                    break;
                default:
                    return false; 
            }
            switch (other.Interval)
            {
                case TimeIntervalSize.Minute:
                    rightMins = Size;
                    break;
                case TimeIntervalSize.Hour:
                    rightMins = Size * 60;
                    break;
                case TimeIntervalSize.Day:
                    rightMins = Size * 60 * 24;
                    break;
                default:
                    return false;
            }
            return leftMins == rightMins;
        }


        public int NumberOfBuckets(QueryDateRange queryDateRange)
        {
            if (!IsValid)
            {
                throw new InvalidOperationException("Invalid TimeInterval");
            }
            if (!queryDateRange.IsValid)
            {
                throw new InvalidOperationException("Invalid QueryDateRange");
            }
            var start = Bucket(queryDateRange.FromDate);
            var end = Add(Bucket(queryDateRange.InclusiveToDate), 1);
            var offsetStart = OffSet(start);
            var offsetEnd = OffSet(end);
            return offsetEnd - offsetStart;
        }


        public DateTime[] Range(QueryDateRange queryDateRange)
        {
            if (!IsValid)
            {
                throw new InvalidOperationException("Invalid TimeInterval");
            }
            if (!queryDateRange.IsValid)
            {
                throw new InvalidOperationException("Invalid QueryDateRange");
            }
            var start = Bucket(queryDateRange.FromDate);
            var range = new DateTime[NumberOfBuckets(queryDateRange)];
            for (int i = 0; i < range.Length; i++)
            {
                range[i] = Add(start, i);
            }
            return range;
        }



        public override string ToString()
        {
            bool single = Size == 1;
            var unit =  Interval switch
            {
                TimeIntervalSize.Minute => single ? "Minute" : "Minutes",
                TimeIntervalSize.Hour => single ? "Hour" : "Hours",
                TimeIntervalSize.Day => single ? "Day" : "Days",
                TimeIntervalSize.Week => single ? "Week" : "Weeks",
                TimeIntervalSize.Month => single ? "Month" : "Months",
                TimeIntervalSize.Qtr => single ? "Qtr" : "Qtrs",
                TimeIntervalSize.Year => single ? "Year" : "Years",
                _ => throw new ArgumentOutOfRangeException()
            };
            if (single)
            {
                return unit;
            }
            return $"{Size} {unit}";
        }




    }

}
