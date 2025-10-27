using MeterDataLib.Parsers;
using Xunit.Abstractions;

namespace TestMeterLib
{
    public class EspHome(ITestOutputHelper Output)
    {
        [Theory]
        [InlineData("esphome.csv" )]

        public async Task Parse1(string resource )
        {
            Console.SetOut(new RedirectOutput(Output));
            using Stream stream = File.OpenRead(Path.Combine("Resources", resource));
            var result = await ParserFactory.ParseAsync(stream, resource, "text/csv");
            Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
            Assert.True(result.Success);
            Assert.True(result.ParserName == "CsvEspHome");
        }


        [Fact]
        public void RecordTests()
        {

            var ts = new DateTime(2024, 1, 2, 3, 4, 5);
            var local = ts.AddHours(10);  //  AEST
            var record = new MeterDataLib.Parsers.CsvEspHome.DataLine
                (
                    ts , local , 1234 , 100 , 1
                );
            Assert.Equal(ts, record.ReadDateUtc);
            Assert.Equal(local, record.ReadDateLocal);
            Assert.Equal(1234, record.WattsIn);
            Assert.Equal(100, record.WattsOut);
            Assert.Equal(0, record.IntervalMinutes);
            Assert.True(record.InitialRead);
            Assert.False(record.Valid);
            Assert.Equal(10m, record.UtcOffset);
            Assert.Equal(new DateTime(2024, 1, 2, 3, 4, 0), record.PeriodEndUtcExclusive);
            Assert.Equal(new DateTime(2024, 1, 2, 13, 4, 0), record.PeriodEndLocalExclusive);
            var intervals = record.CreateOneMinuteIntervals();
            Assert.Empty(intervals);
        }


        [Fact]
        public void RecordTestsWithDelta1()
        {

            var ts = new DateTime(2024, 1, 2, 3, 4, 5);
            var local = ts.AddHours(10);  //  AEST
            var previous = new DateTime(2024, 1, 2, 3, 3, 15);

            int  inWatts = 1234;
            int  outWatts = 100;
            int deltaIn = 100;
            int deltaOut = 5;
            int  previousInWats = inWatts - deltaIn;
            int  previousOutWats = outWatts - deltaOut;

            var record = new MeterDataLib.Parsers.CsvEspHome.DataLine
                (
                    ts, local, inWatts , outWatts , 1
                )
            {
                PreviousReadDateUtc = previous,
                PreviousWattsIn = previousInWats,
                PreviousWattsOut = previousOutWats
            };

            Assert.Equal(ts, record.ReadDateUtc);
            Assert.Equal(local, record.ReadDateLocal);
            Assert.Equal(inWatts, record.WattsIn);
            Assert.Equal(outWatts, record.WattsOut);
            Assert.Equal(1, record.IntervalMinutes);
            Assert.False(record.InitialRead);
            Assert.False(record.IsRollOverOrReset);
            Assert.True(record.Valid);
            Assert.Equal(10m, record.UtcOffset);
            Assert.Equal(new DateTime(2024, 1, 2, 3, 4, 0), record.PeriodEndUtcExclusive);
            Assert.Equal(new DateTime(2024, 1, 2, 13, 4, 0), record.PeriodEndLocalExclusive);

            Assert.Equal(new DateTime(2024, 1, 2, 3, 3, 0), record.PeriodStartUtc);
            Assert.Equal(new DateTime(2024, 1, 2, 13, 3, 0), record.PeriodStartLocal);
            Assert.Equal((decimal)deltaIn/1000m  , record.kWhIn );
            Assert.Equal((decimal)deltaOut / 1000m, record.kWhOut);
            var intervals = record.CreateOneMinuteIntervals();
            Assert.Single(intervals);
            var first = intervals[0];

            // every property int record should be equal to first
            Assert.Equal(first.ReadDateUtc , record.ReadDateUtc) ;
            Assert.Equal(first.ReadDateLocal, record.ReadDateLocal);
            Assert.Equal(first.PreviousReadDateUtc, record.PreviousReadDateUtc); 
            Assert.Equal(first.WattsIn, record.WattsIn);
            Assert.Equal(first.WattsOut, record.WattsOut);
            Assert.Equal(first.IntervalMinutes, record.IntervalMinutes);
            Assert.Equal(first.InitialRead, record.InitialRead);
            Assert.Equal(first.IsRollOverOrReset, record.IsRollOverOrReset);
            Assert.Equal(first.Valid, record.Valid);
            Assert.Equal(first.UtcOffset, record.UtcOffset);
            Assert.Equal(first.PeriodEndUtcExclusive, record.PeriodEndUtcExclusive);
            Assert.Equal(first.PeriodEndLocalExclusive, record.PeriodEndLocalExclusive);
            Assert.Equal(first.PeriodStartUtc, record.PeriodStartUtc);
            Assert.Equal(first.PeriodStartLocal, record.PeriodStartLocal);
            Assert.Equal(first.kWhIn, record.kWhIn);
            Assert.Equal(first.kWhOut, record.kWhOut);
        }




        [Fact]
        public void RecordTestsWithDelta5()
        {

            var ts = new DateTime(2024, 1, 2, 3, 4, 5);
            var local = ts.AddHours(10);  //  AEST
            var previous = new DateTime(2024, 1, 2, 3, 0, 15);

            int inWatts = 1234;
            int outWatts = 100;
            int deltaIn = 100;
            int deltaOut = 5;
            int previousInWats = inWatts - deltaIn;
            int previousOutWats = outWatts - deltaOut;

            var record = new MeterDataLib.Parsers.CsvEspHome.DataLine
                (
                    ts, local, inWatts, outWatts, 1
                )
            {
                PreviousReadDateUtc = previous,
                PreviousWattsIn = previousInWats,
                PreviousWattsOut = previousOutWats
            };

            Assert.Equal(ts, record.ReadDateUtc);
            Assert.Equal(local, record.ReadDateLocal);
            Assert.Equal(inWatts, record.WattsIn);
            Assert.Equal(outWatts, record.WattsOut);
            Assert.Equal(4, record.IntervalMinutes);
            Assert.False(record.InitialRead);
            Assert.False(record.IsRollOverOrReset);
            Assert.True(record.Valid);
            Assert.Equal(10m, record.UtcOffset);
            Assert.Equal(new DateTime(2024, 1, 2, 3, 4, 0), record.PeriodEndUtcExclusive);
            Assert.Equal(new DateTime(2024, 1, 2, 13, 4, 0), record.PeriodEndLocalExclusive);

            Assert.Equal(new DateTime(2024, 1, 2, 3, 0, 0), record.PeriodStartUtc);
            Assert.Equal(new DateTime(2024, 1, 2, 13, 0, 0), record.PeriodStartLocal);
            Assert.Equal((decimal)deltaIn / 1000m, record.kWhIn);
            Assert.Equal((decimal)deltaOut / 1000m, record.kWhOut);
            var intervals = record.CreateOneMinuteIntervals();
            Assert.Equal(4, intervals.Count);

            Assert.Equal(record.kWhIn , intervals.Sum(r=> r.kWhIn));
            Assert.Equal(record.kWhOut, intervals.Sum(r => r.kWhOut));


            for ( int i = 0; i < intervals.Count; i++)
            {
                var x = intervals[i];
                var expectedUtc = record.PeriodStartUtc.AddMinutes(i + 1);
                var expectedLocal = record.PeriodStartLocal.AddMinutes(i + 1);
                Assert.Equal(expectedUtc, x.ReadDateUtc);
                Assert.Equal(expectedLocal, x.ReadDateLocal);
                Assert.Equal(1, x.IntervalMinutes);
                Assert.True(x.Valid);
                if ( i > 0 ) 
                {
                    Assert.True(x.WattsIn >= intervals[i - 1].WattsIn);
                    Assert.True(x.WattsOut >= intervals[i - 1].WattsOut);
                    Assert.True(x.ReadDateUtc >= intervals[i - 1].ReadDateUtc);
                    Assert.True(x.ReadDateLocal >= intervals[i - 1].ReadDateLocal);
                }

            }


        }

    }

}