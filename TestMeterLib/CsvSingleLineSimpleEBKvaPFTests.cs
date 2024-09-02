using FluentAssertions;
using MeterDataLib.Parsers;
using Xunit.Abstractions;

namespace TestMeterLib
{
    public class CsvSingleLineSimpleEBKvaPFTests(ITestOutputHelper Output)
    {
        [Theory]
        [InlineData("SampleCsv.csv", "text/csv" , 577 , 578263.04 )]

        public async Task  Parse1(string resource, string mimeType , int expectedDays , decimal expectedTotal)
        {
            Console.SetOut(new RedirectOutput(Output));
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
                Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
                Assert.True(result.Success);
                result.SitesDays.Count.Should().Be(expectedDays);
                decimal actualTotal = result.SitesDays.SelectMany(x => x.Channels.Values).Where(x => x.ChannelType == MeterDataLib.ChanelType.ActiveEnergyConsumption).Sum(x => x.Total);
                actualTotal.Should().Be(expectedTotal);
                Assert.True(result.ParserName == "SingleLineWith_E_B_KVA_PF");


            }
        }
    }

}