using FluentAssertions;
using MeterDataLib.Query;
using Xunit.Abstractions;

namespace TestMeterLib
{

    public class RangeAndInterValTests(ITestOutputHelper Output)
    {



        [Fact]
        public void DateRangeNormal()
        {
            var range = QueryDateRange.FromText("2021-01-01", "2021-01-31");
            range.TotalDays.Should().Be(31);
            range.FromDate.Should().Be(new DateTime(2021, 1, 1));
            range.ToDate.Should().Be(new DateTime(2021, 1, 31));
            range.ExclusiveToDate.Should().Be(new DateTime(2021, 2, 1));
            range.InclusiveToDate.Should().Be(new DateTime(2021, 1, 31, 23 ,59, 0));
        }


        [Fact]
        public void DateRangeMonthRelative()
        {
            var lastMonth= QueryDateRange.RelativeRange(RelativeDateRanges.LastMonth);
            var last3Months = QueryDateRange.RelativeRange(RelativeDateRanges.Last3Months);
            var last12Months = QueryDateRange.RelativeRange(RelativeDateRanges.Last12Months);
            var last13Months = QueryDateRange.RelativeRange(RelativeDateRanges.Last13Months);
            var thisMonth = QueryDateRange.RelativeRange(RelativeDateRanges.ThisMonth);

            lastMonth.FromDate.Day.Should().Be(1);
            lastMonth.FromDate.Month.Should().Be(DateTime.Now.AddMonths(-1).Month);
            lastMonth.FromDate.Year.Should().Be(DateTime.Now.AddMonths(-1).Year);
            lastMonth.ExclusiveToDate.Day.Should().Be(1);
            lastMonth.ExclusiveToDate.Month.Should().Be(DateTime.Now.Month);
            lastMonth.ExclusiveToDate.Year.Should().Be(DateTime.Now.Year);

            thisMonth.FromDate.Day.Should().Be(1);
            thisMonth.FromDate.Month.Should().Be(DateTime.Now.Month);
            thisMonth.FromDate.Year.Should().Be(DateTime.Now.Year);
            thisMonth.ExclusiveToDate.Day.Should().Be(1);
            thisMonth.ExclusiveToDate.Month.Should().Be(DateTime.Now.AddMonths(1).Month);
            thisMonth.ExclusiveToDate.Year.Should().Be(DateTime.Now.AddMonths(1).Year);

            last3Months.TotalDays.Should().BeGreaterThan(lastMonth.TotalDays);
            last3Months.TotalDays.Should().BeInRange(28*3 , 31*3);
            last12Months.TotalDays.Should().BeInRange(365, 366);
            last13Months.TotalDays.Should().BeInRange(365+28, 366+31);


            lastMonth.ToDate.Should().Be(thisMonth.FromDate.AddDays(-1));
            last3Months.ToDate.Should().Be(thisMonth.FromDate.AddDays(-1));
            last12Months.ToDate.Should().Be(thisMonth.FromDate.AddDays(-1));
            last13Months.ToDate.Should().Be(thisMonth.FromDate.AddDays(-1));

            lastMonth.FromDate.Should().BeBefore(thisMonth.FromDate);
            last3Months.FromDate.Should().BeBefore(lastMonth.FromDate);
            last12Months.FromDate.Should().BeBefore(lastMonth.FromDate);
            last13Months.FromDate.Should().BeBefore(last12Months.FromDate);

        }



        [Fact]
        public void DateRangeInvalid()
        {

            var range = QueryDateRange.FromText("2021-01-31", "2021-01-01");
            range.IsEmpty.Should().BeFalse();

            range.IsValid.Should().BeFalse();

        }


        [Fact]
        public void DateRangeIsEmpty()
        { 

            var range = QueryDateRange.Empty;
            range.IsValid.Should().BeTrue();
            range.IsEmpty.Should().BeTrue();

        }



        [Fact]
        public void IntervalMinute()
        {
            var interval = new TimeInterval(TimeIntervalSize.Minute, 30);
            interval.IsValid.Should().BeTrue();
            interval.Size.Should().Be(30);
            interval.Interval.Should().Be(TimeIntervalSize.Minute);
            var d = new DateTime(2021, 1, 1, 0, 0, 0);
            var range = new QueryDateRange(d, d);
            interval.NumberOfBuckets(range).Should().Be(48);
            interval.Bucket( d).Should().Be(d);
            interval.Bucket(d.AddMinutes(14)).Should().Be(d);
            interval.Bucket(d.AddMinutes(14).AddSeconds(37)).Should().Be(d);
            interval.Bucket(range.InclusiveToDate).Should().Be(d.AddMinutes(47*30));
            var periodValues  = interval.NumberOfBuckets(range);
            var periods = interval.Range(range);
            periodValues.Should().Be(48);
            periods.Length.Should().Be(48);
            periods.First().Should().Be(d);
            periods.Last().Should().Be(d.AddMinutes(47 * 30));

        }

        [Fact]
        public void IntervalValidation()
        {
            var badInterval = new TimeInterval(TimeIntervalSize.Minute, 28);
            badInterval.IsValid.Should().BeFalse();
            badInterval = new TimeInterval(TimeIntervalSize.Minute, 60);
            badInterval.IsValid.Should().BeTrue();
            badInterval = new TimeInterval(TimeIntervalSize.Hour, -1);
            badInterval.IsValid.Should().BeFalse();
            badInterval = new TimeInterval(TimeIntervalSize.Hour, 0);
            badInterval.IsValid.Should().BeFalse();
            badInterval = new TimeInterval(TimeIntervalSize.Hour, 26);
            badInterval.IsValid.Should().BeFalse();


        }

        [Fact]
        public void IntervalWeeks()
        {
            var interval = new TimeInterval(TimeIntervalSize.Week, 1);
            interval.IsValid.Should().BeTrue();
            interval.Size.Should().Be(1);
            interval.Interval.Should().Be(TimeIntervalSize.Week);
            
            var d = new DateTime(2024, 1, 1, 0, 0, 0);
            var d2 = new DateTime(2024, 1, 31, 0, 0, 0);
            var range = new QueryDateRange(d, d2);
            
            interval.NumberOfBuckets(range).Should().Be(5);
            interval.Bucket(d).Should().Be(d.AddDays(-1 * (int)d.DayOfWeek));
            interval.Bucket(d2).Should().Be(d2.AddDays(-1 * (int)d2.DayOfWeek));

            var periodValues = interval.NumberOfBuckets(range);
            var periods = interval.Range(range);
            periodValues.Should().Be(5);
            periods.Length.Should().Be(5);
            periods.First().Should().Be(d.AddDays(-1 * (int)d.DayOfWeek));
            periods.Last().Should().Be(d2.AddDays(-1 * (int)d2.DayOfWeek));

        }


        [Fact]
        public void IntervalMonths()
        {
            var interval = new TimeInterval(TimeIntervalSize.Month, 1);
            interval.IsValid.Should().BeTrue();
            interval.Size.Should().Be(1);
            interval.Interval.Should().Be(TimeIntervalSize.Month);

            var d = new DateTime(2024, 1, 1, 0, 0, 0);
            var d2 = new DateTime(2024, 12, 31, 0, 0, 0);
            var range = new QueryDateRange(d, d2);

            interval.NumberOfBuckets(range).Should().Be(12);
            interval.Bucket(d).Should().Be(d);
            interval.Bucket(d2).Should().Be(d.AddMonths(11));

            var periodValues = interval.NumberOfBuckets(range);
            var periods = interval.Range(range);
            periodValues.Should().Be(12);
            periods.Length.Should().Be(12);
            periods.First().Should().Be(d);
            periods.Last().Should().Be(interval.Bucket(d2));
        }

        [Fact]
        public void IntervalQtr()
        {
            var interval = new TimeInterval(TimeIntervalSize.Qtr, 1);
            interval.IsValid.Should().BeTrue();
            interval.Size.Should().Be(1);
            interval.Interval.Should().Be(TimeIntervalSize.Qtr);

            var d = new DateTime(2024, 1, 1, 0, 0, 0);
            var d2 = new DateTime(2024, 12, 31, 0, 0, 0);
            var range = new QueryDateRange(d, d2);

            interval.NumberOfBuckets(range).Should().Be(4);
            interval.Bucket(d).Should().Be(d);
            interval.Bucket(d2).Should().Be(d.AddMonths(9));

            var periodValues = interval.NumberOfBuckets(range);
            var periods = interval.Range(range);
            periodValues.Should().Be(4);
            periods.Length.Should().Be(4);
            periods.First().Should().Be(d);
            periods.Last().Should().Be(interval.Bucket(d2));
        }

        [Fact]
        public void IntervalYear()
        {
            var interval = new TimeInterval(TimeIntervalSize.Year, 1);
            interval.IsValid.Should().BeTrue();
            interval.Size.Should().Be(1);
            interval.Interval.Should().Be(TimeIntervalSize.Year);

            var d = new DateTime(2024, 1, 1, 0, 0, 0);
            var d2 = new DateTime(2024, 12, 31, 0, 0, 0);
            var range = new QueryDateRange(d, d2);

            interval.NumberOfBuckets(range).Should().Be(1);
            interval.Bucket(d).Should().Be(d);
            interval.Bucket(d2).Should().Be(d);

            var periodValues = interval.NumberOfBuckets(range);
            var periods = interval.Range(range);
            periodValues.Should().Be(1);
            periods.Length.Should().Be(1);
            periods.First().Should().Be(d);
            periods.Last().Should().Be(d);
        }


    }

}
 