using FluentAssertions;
using MeterDataLib.Parsers;

namespace TestMeterLib
{
    public class MeterDataReport(ITestOutputHelper Output)
    {
        [Theory]
        [InlineData("MeterDataReport.csv", "text/csv")]

        public async Task MeterDataReportTest1(string resource, string mimeType)
        {
            Console.SetOut(new RedirectOutput(Output));
            using Stream stream = File.OpenRead(Path.Combine("Resources", resource));
            var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
            Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
            Assert.True(result.Success);
            result.ParserName.Should().Be("CsvMultiLine2");
            result.Sites.Should().Be(1);
            result.SitesDays.First().Channels.Count.Should().Be(3);

            // Validate E1 channel
            result.SitesDays.First().Channels["E1"].IntervalMinutes.Should().Be(5);
            result.SitesDays.First().Channels["E1"].ChannelType.Should().Be(MeterDataLib.ChanelType.ActiveEnergyConsumption);
            result.SitesDays.First().Channels["E1"].Readings.Sum().Should().BeGreaterThan(0);

            // Validate E2 channel
            result.SitesDays.First().Channels["E2"].IntervalMinutes.Should().Be(5);
            result.SitesDays.First().Channels["E2"].ChannelType.Should().Be(MeterDataLib.ChanelType.ActiveEnergyConsumption);
            result.SitesDays.First().Channels["E2"].Readings.Sum().Should().BeGreaterThan(0);

            // Validate B1 channel (Solar/Generation - should be positive like NEM12)
            result.SitesDays.First().Channels["B1"].IntervalMinutes.Should().Be(5);
            result.SitesDays.First().Channels["B1"].ChannelType.Should().Be(MeterDataLib.ChanelType.ActiveEnergyGeneration);
            result.SitesDays.First().Channels["B1"].Readings.Sum().Should().BeGreaterThan(0);

            // Validate totals across all days
            var b1Total = result.SitesDays.Sum(sd => sd.Channels.ContainsKey("B1") ? sd.Channels["B1"].Total : 0);
            var e1Total = result.SitesDays.Sum(sd => sd.Channels.ContainsKey("E1") ? sd.Channels["E1"].Total : 0);
            var e2Total = result.SitesDays.Sum(sd => sd.Channels.ContainsKey("E2") ? sd.Channels["E2"].Total : 0);

            Console.WriteLine($"B1 Total: {b1Total:F3} kWh (expected: 3234.064)");
            Console.WriteLine($"E1 Total: {e1Total:F3} kWh (expected: 3091.248)");
            Console.WriteLine($"E2 Total: {e2Total:F3} kWh (expected: 3.480)");

            // Validate totals match expected values (allow small rounding differences)
            b1Total.Should().BeApproximately(3234.064m, 0.01m);
            e1Total.Should().BeApproximately(3091.248m, 0.01m);
            e2Total.Should().BeApproximately(3.480m, 0.01m);
        }

        [Theory]
        [InlineData("MeterDataReport.csv", "text/csv")]
        public async Task MeterDataReport_1PmTotalEnergyUsed_2025_11_07(string resource, string mimeType)
        {
            Console.SetOut(new RedirectOutput(Output));
            using Stream stream = File.OpenRead(Path.Combine("Resources", resource));
            var result = await ParserFactory.ParseAsync(stream, resource, mimeType);

            Assert.True(result.Success);
            result.ParserName.Should().Be("CsvMultiLine2");

            var targetDate = new DateTime(2025, 11, 7);
            const int periodIndexAt1Pm = (13 * 60) / 5; // 13:00 at 5-minute intervals

            var siteDay = result.SitesDays.Single(sd => sd.Date.Date == targetDate.Date);

            var e1 = siteDay.Channels["E1"].Readings[periodIndexAt1Pm];
            var e2 = siteDay.Channels["E2"].Readings[periodIndexAt1Pm];
            var b1 = siteDay.Channels["B1"].Readings[periodIndexAt1Pm];

            var totalEnergyUsedAt1Pm = e1 + e2 - b1;

            Console.WriteLine($"2025-11-07 13:00 E1: {e1:F3}, E2: {e2:F3}, B1: {b1:F3}, NetEnergy(E1+E2-B1): {totalEnergyUsedAt1Pm:F3}");

            // Negative means net generation/export for the interval
            totalEnergyUsedAt1Pm.Should().BeLessThan(0);
        }

        [Theory]
        [InlineData("MeterDataReport.csv", "text/csv")]
        public async Task MeterDataReport_Net15MinEnergy_2025_11_01_1230(string resource, string mimeType)
        {
            Console.SetOut(new RedirectOutput(Output));
            using Stream stream = File.OpenRead(Path.Combine("Resources", resource));
            var result = await ParserFactory.ParseAsync(stream, resource, mimeType);

            Assert.True(result.Success);
            result.ParserName.Should().Be("CsvMultiLine2");

            var targetDate = new DateTime(2025, 11, 1);
            const int startIndex1230 = (12 * 60 + 30) / 5; // 12:30

            var siteDay = result.SitesDays.Single(sd => sd.Date.Date == targetDate.Date);

            decimal net15Min = 0;
            for (int i = startIndex1230; i < startIndex1230 + 3; i++)
            {
                var e1 = siteDay.Channels["E1"].Readings[i];
                var e2 = siteDay.Channels["E2"].Readings[i];
                var b1 = siteDay.Channels["B1"].Readings[i];
                net15Min += e1 + e2 - b1;
            }

            Console.WriteLine($"2025-11-01 12:30-12:44 Net15Min(E1+E2-B1): {net15Min:F3}");
            net15Min.Should().BeApproximately(-1.241m, 0.001m);
        }
    }

}
