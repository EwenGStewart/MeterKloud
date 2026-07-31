using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MeterDataLib;
using MeterDataLib.Parsers;
using Xunit;

namespace TestMeterLib;

public class SolarAnalysis(ITestOutputHelper Output)
{
    [Theory]
    [InlineData("MeterDataReport.csv", "text/csv")]
    public async Task AnalyzeSolarExportPeriods(string resource, string mimeType)
    {
        Console.SetOut(new RedirectOutput(Output));

        // Parse the file
        using Stream stream = File.OpenRead(Path.Combine("Resources", resource));
        var result = await ParserFactory.ParseAsync(stream, resource, mimeType);

        result.Success.Should().BeTrue();
        result.ParserName.Should().Be("CsvMultiLine2");

        // Track periods where solar export exceeds consumption
        var solarExceedsPeriods = new List<ExportPeriod>();

        Console.WriteLine($"Analyzing {result.SitesDays.Count} site-days of data");

        // Iterate through all days and all intervals
        foreach (var siteDay in result.SitesDays)
        {
            var date = siteDay.Date;

            // Get channel days for B1, E1, E2
            if (!siteDay.Channels.ContainsKey("B1") || 
                !siteDay.Channels.ContainsKey("E1") || 
                !siteDay.Channels.ContainsKey("E2"))
                continue;

            var b1Day = siteDay.Channels["B1"];
            var e1Day = siteDay.Channels["E1"];
            var e2Day = siteDay.Channels["E2"];

            // Check each 5-minute interval
            int maxReadings = Math.Min(Math.Min(b1Day.Readings.Length, e1Day.Readings.Length), e2Day.Readings.Length);

            for (int i = 0; i < maxReadings; i++)
            {
                var solarReading = b1Day.Readings[i]; // This is NEGATIVE for generation
                var e1Reading = e1Day.Readings[i];
                var e2Reading = e2Day.Readings[i];

                // Solar generation is stored as negative, so convert to positive
                var solarGeneration = Math.Abs(solarReading);
                var totalConsumption = e1Reading + e2Reading;

                // Check if solar generation exceeds consumption
                if (solarGeneration > totalConsumption && solarGeneration > 0.001m) // Ignore tiny values
                {
                    var timestamp = date.AddMinutes(i * b1Day.IntervalMinutes);
                    var excess = solarGeneration - totalConsumption;

                    solarExceedsPeriods.Add(new ExportPeriod
                    {
                        Timestamp = timestamp,
                        SolarGen = solarGeneration,
                        E1 = e1Reading,
                        E2 = e2Reading,
                        TotalConsumption = totalConsumption,
                        Excess = excess
                    });
                }
            }
        }

        // Report findings
        Console.WriteLine($"\nFound {solarExceedsPeriods.Count} periods where solar generation exceeds consumption:");
        Console.WriteLine("");

        if (solarExceedsPeriods.Count > 0)
        {
            Console.WriteLine("Timestamp               | Solar Gen | E1 Cons | E2 Cons | Total Cons | Excess Export");
            Console.WriteLine("------------------------|-----------|---------|---------|------------|---------------");

            foreach (var period in solarExceedsPeriods.OrderByDescending(p => p.Excess).Take(50)) // Show top 50
            {
                Console.WriteLine($"{period.Timestamp:yyyy-MM-dd HH:mm} | {period.SolarGen,9:F3} | {period.E1,7:F3} | {period.E2,7:F3} | {period.TotalConsumption,10:F3} | {period.Excess,13:F3}");
            }

            if (solarExceedsPeriods.Count > 50)
            {
                Console.WriteLine($"... and {solarExceedsPeriods.Count - 50} more periods");
            }

            // Summary statistics
            var totalExcess = solarExceedsPeriods.Sum(p => p.Excess);
            var avgExcess = solarExceedsPeriods.Average(p => p.Excess);
            var maxExcess = solarExceedsPeriods.Max(p => p.Excess);

            Console.WriteLine("");
            Console.WriteLine("Summary:");
            Console.WriteLine($"  Total excess exported: {totalExcess:F3} kWh");
            Console.WriteLine($"  Average excess per period: {avgExcess:F3} kWh");
            Console.WriteLine($"  Maximum excess in a period: {maxExcess:F3} kWh");
        }
        else
        {
            Console.WriteLine("No periods found where solar generation exceeds consumption.");
        }
    }

    private class ExportPeriod
    {
        public DateTime Timestamp { get; set; }
        public decimal SolarGen { get; set; }
        public decimal E1 { get; set; }
        public decimal E2 { get; set; }
        public decimal TotalConsumption { get; set; }
        public decimal Excess { get; set; }
    }
}
