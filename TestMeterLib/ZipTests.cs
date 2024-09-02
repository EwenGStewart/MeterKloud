using MeterDataLib.Parsers;
using Xunit.Abstractions;

namespace TestMeterLib
{
    public class ZipTests(ITestOutputHelper Output)
    {
        [Theory]
        [InlineData("SampleNem12Zip.zip", "application/x-zip-compressed")]
        
        public async Task Parse1(string resource, string mimeType)
        {
            Console.SetOut(new RedirectOutput(Output));
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
                Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
                Assert.True(result.Success);
                Assert.True(result.ParserName == "NEM12");



            }
        }
    }

}