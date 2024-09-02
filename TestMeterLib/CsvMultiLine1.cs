using FluentAssertions;
using MeterDataLib.Parsers;
using Xunit.Abstractions;

namespace TestMeterLib
{
    public class CsvMultiLine1(ITestOutputHelper Output)
    {
        [Theory]
        [InlineData("MultiLineCsv1.csv", "text/csv")]

        public async Task CsvMultiLine1Test1(string resource, string mimeType)
        {
            Console.SetOut(new RedirectOutput(Output));
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
                Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
                Assert.True(result.Success);
                result.ParserName.Should().Be( "MultiLineCSV1");

            }
        }
    }

}