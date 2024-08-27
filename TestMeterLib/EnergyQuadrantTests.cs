using FluentAssertions;
using MeterDataLib.Parsers;
using Xunit.Abstractions;

namespace TestMeterLib
{

    public class EnergyQuadrantTests(ITestOutputHelper Output)
    {

        [Fact]
        public async Task ValidateNem12Sample()
        {
            var resource = "SampleNem12-2.csv";
            var mimeType = "text/csv";
            Console.SetOut(new RedirectOutput(Output));
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 

                var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
                Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
                Assert.True(result.Success);
                Assert.True(result.ParserName == "NEM12");
                Assert.True(result.Sites > 0);
                Assert.True(result.Errors == 0);
                Assert.True(result.TotalSiteDays > 0);
                var firstDay = result.SitesDays.First();
                var interval = firstDay.Channels.Values.First().IntervalMinutes;
                Assert.True(interval == 30);
                var quads = firstDay.GetEnergyQuadrants(new MeterDataLib.QuadrantOptions());
                quads.Length.Should().Be(48);
                foreach (var quad in quads)
                {
                    Console.WriteLine($"Quad:{quad}");
                }
                Console.WriteLine("-----------------------------------------------");
                foreach (var quad in quads)
                {
                    Console.WriteLine(quad.ToCsvString());
                }

                quads.Select(x => x.NetActivePower_kWh).Sum().Should().BeApproximately(8823.7m, 0.001m);
                quads.Select(x => x.RealPowerConsumption_kW).Max().Should().BeApproximately(474.8m, 0.001m);
                quads.Select(x => x.ApparentPower_kVA).Max().Should().BeApproximately(476.672m, 0.005m);


            }
        }

        [Fact]
        public async Task ValidateNem12SampleForce5min()
        {
            var resource = "SampleNem12-2.csv";
            var mimeType = "text/csv";
            Console.SetOut(new RedirectOutput(Output));
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 

                var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
                Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
 
                var firstDay = result.SitesDays.First();
                var interval = firstDay.Channels.Values.First().IntervalMinutes;
                Assert.True(interval == 30);
                var quads = firstDay.GetEnergyQuadrants(new MeterDataLib.QuadrantOptions() { Interval=5});
                quads.Length.Should().Be(288);
                foreach (var quad in quads)    
                {
                    Console.WriteLine($"Quad:{quad}");
                }
                Console.WriteLine("-----------------------------------------------");
                foreach (var quad in quads)
                {
                    Console.WriteLine(quad.ToCsvString());
                }

                quads.Select(x => x.NetActivePower_kWh).Sum().Should().BeApproximately(8823.7m, 0.001m);
                //quads.Select(x => x.RealPowerConsumption_Kw).Max().Should().BeApproximately(474.8m, 0.001m);
                //quads.Select(x => x.ApparentPower_kVA).Max().Should().BeApproximately(476.672m, 0.005m);


            }
        }


        [Fact]
        public async Task ValidateNem12SampleFilterEOnly()
        {
            var resource = "SampleNem12-2.csv";
            var mimeType = "text/csv";
            Console.SetOut(new RedirectOutput(Output));
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 

                var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
                Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");

                var firstDay = result.SitesDays.First();
                var interval = firstDay.Channels.Values.First().IntervalMinutes;
                Assert.True(interval == 30);
                var quads = firstDay.GetEnergyQuadrants(new MeterDataLib.QuadrantOptions() { OnlyIncludeChannelFilter=new string[] { "E1" } });
                quads.Length.Should().Be(48);
                foreach (var quad in quads)
                {
                    Console.WriteLine($"Quad:{quad}");
                }
                Console.WriteLine("-----------------------------------------------");
                foreach (var quad in quads)
                {
                    Console.WriteLine(quad.ToCsvString());
                }

                quads.Select(x => x.NetActivePower_kWh).Sum().Should().BeApproximately(8823.7m, 0.001m);
                quads.Select(x => x.RealPowerConsumption_kW).Max().Should().BeApproximately(474.8m, 0.001m);
                quads.Select(x => x.ApparentPower_kVA).Max().Should().BeApproximately(474.8m, 0.005m);


            }
        }

        [Fact]
        public async Task ValidateNem12SampleExcludeQOnly()
        {
            var resource = "SampleNem12-2.csv";
            var mimeType = "text/csv";
            Console.SetOut(new RedirectOutput(Output));
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 

                var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
                Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");

                var firstDay = result.SitesDays.First();
                var interval = firstDay.Channels.Values.First().IntervalMinutes;
                Assert.True(interval == 30);
                var quads = firstDay.GetEnergyQuadrants(new MeterDataLib.QuadrantOptions() { AlwaysExcludeChannelFilter = new string[] { "Q1" } });
                quads.Length.Should().Be(48);
                foreach (var quad in quads)
                {
                    Console.WriteLine($"Quad:{quad}");
                }
                Console.WriteLine("-----------------------------------------------");
                foreach (var quad in quads)
                {
                    Console.WriteLine(quad.ToCsvString());
                }

                quads.Select(x => x.NetActivePower_kWh).Sum().Should().BeApproximately(8823.7m, 0.001m);
                quads.Select(x => x.RealPowerConsumption_kW).Max().Should().BeApproximately(474.8m, 0.001m);
                quads.Select(x => x.ApparentPower_kVA).Max().Should().BeApproximately(474.8m, 0.005m);


            }
        }

        [Fact]
        public async Task ValidateNem12SampleReactiveOptions()
        {
            var resource = "SampleNem12-2.csv";
            var mimeType = "text/csv";
            Console.SetOut(new RedirectOutput(Output));
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 

                var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
                Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");

                var firstDay = result.SitesDays.First();
                var interval = firstDay.Channels.Values.First().IntervalMinutes;
                Assert.True(interval == 30);
                var quads = firstDay.GetEnergyQuadrants(new MeterDataLib.QuadrantOptions() { ReactivePowerGenerationHandling = MeterDataLib.GenerationHandlingOption.IgnoreConsumption });
                quads.Length.Should().Be(48);
                foreach (var quad in quads)
                {
                    Console.WriteLine($"Quad:{quad}");
                }
                Console.WriteLine("-----------------------------------------------");
                foreach (var quad in quads)
                {
                    Console.WriteLine(quad.ToCsvString());
                }

                quads.Select(x => x.NetActivePower_kWh).Sum().Should().BeApproximately(8823.7m, 0.001m);
                quads.Select(x => x.RealPowerConsumption_kW).Max().Should().BeApproximately(474.8m, 0.001m);
                quads.Select(x => x.ApparentPower_kVA).Max().Should().BeApproximately(474.8m, 0.005m);


            }
        }


        [Fact]
        public async Task ValidateNem12SampleReactiveOptionsIgnoreBoth()
        {
            var resource = "SampleNem12-2.csv";
            var mimeType = "text/csv";
            Console.SetOut(new RedirectOutput(Output));
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 

                var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
                Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");

                var firstDay = result.SitesDays.First();
                var interval = firstDay.Channels.Values.First().IntervalMinutes;
                Assert.True(interval == 30);
                var quads = firstDay.GetEnergyQuadrants(new MeterDataLib.QuadrantOptions() { ReactivePowerGenerationHandling = MeterDataLib.GenerationHandlingOption.IgnoreBoth });
                quads.Length.Should().Be(48);
                foreach (var quad in quads)
                {
                    Console.WriteLine($"Quad:{quad}");
                }
                Console.WriteLine("-----------------------------------------------");
                foreach (var quad in quads)
                {
                    Console.WriteLine(quad.ToCsvString());
                }

                quads.Select(x => x.NetActivePower_kWh).Sum().Should().BeApproximately(8823.7m, 0.001m);
                quads.Select(x => x.RealPowerConsumption_kW).Max().Should().BeApproximately(474.8m, 0.001m);
                quads.Select(x => x.ApparentPower_kVA).Max().Should().BeApproximately(474.8m, 0.005m);


            }
        }

    }

}
