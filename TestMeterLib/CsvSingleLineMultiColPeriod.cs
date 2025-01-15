using MeterDataLib.Parsers;
using Xunit.Abstractions;

namespace TestMeterLib
{
    public class CsvSingleLineMultiColPeriod(ITestOutputHelper Output)
    {
        [Theory]
        [InlineData("61029293722_meter_usage_data_cipc1_05_2015.csv", "text/csv")]

        public async Task CsvSingleLineMultiColPeriod_test1(string resource, string mimeType)
        {
            Console.SetOut(new RedirectOutput(Output));
            using Stream stream = File.OpenRead(Path.Combine("Resources", resource));
            var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
            Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
            Assert.True(result.Success);
            Assert.True(result.ParserName == "SingleLineByPeriod");
        }


        [Theory]
        [InlineData("SampleCsv8.csv", "text/csv")]

        public async Task CsvSingleLineMultiColPeriod2_test1(string resource, string mimeType)
        {
            Console.SetOut(new RedirectOutput(Output));
            using Stream stream = File.OpenRead(Path.Combine("Resources", resource));
            var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
            Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
            Assert.True(result.Success);
            Assert.True(result.ParserName == "SingleLineByPeriod2");
        }


    }

}