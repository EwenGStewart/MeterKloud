using MeterDataLib.Parsers;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Xunit.Abstractions;

namespace TestMeterLib
{
    public class ParserNEM12(ITestOutputHelper Output)
    {
        [Theory]
        [InlineData("4103354306_20160604_20170605_20170605000000_EnergyAustralia_DETAILED (1).csv", "text/csv")]
        [InlineData("6407276797_20150803_20160517_20160518160200_UNITEDENERGY_DETAILED.csv", "text/csv")]
        [InlineData("SampleNem", "")]
        [InlineData("SampleNem12.csv", "text/csv")]
        [InlineData("SampleNem12-2.csv", "text/csv")]
        [InlineData("SampleNem12-3.csv", "text/csv")]
        [InlineData("SampleNem12-4.CSV", "text/csv")]

        public async Task Nem12(string resource, string mimeType)
        {
            Console.SetOut(new RedirectOutput(Output));
            using Stream stream = File.OpenRead(Path.Combine("Resources", resource));
            // Use the stream here
            //test 

            var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
            Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
            Assert.True(result.Success);
            Assert.True(result.ParserName == "NEM12");
            Assert.True(result.Sites > 0);
            Assert.True(result.Errors == 0);
            Assert.True(result.TotalSiteDays > 0);
        }



        [Fact]
        public async Task Nem12_duplicate()
        {
            string resource = "All_Detailed_as_at_20250211.csv";
            string mimeType = "text/csv";
            Console.SetOut(new RedirectOutput(Output));
            using Stream stream = File.OpenRead(Path.Combine("Resources", resource));
            // Use the stream here
            //test 

            var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
            Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
            Assert.True(result.Success);
            Assert.True(result.ParserName == "NEM12");
            Assert.True(result.Sites > 0);
            Assert.True(result.Errors == 0);
            Assert.True(result.Warnings == 0);
            Assert.True(result.TotalSiteDays > 0);
        }
        [Fact]
        public async Task Nem12_duplicate_totalChanged()
        {
            string resource = "All_Detailed_as_at_20250211 AlteredDuplicate.csv";
            string mimeType = "text/csv";
            Console.SetOut(new RedirectOutput(Output));
            using Stream stream = File.OpenRead(Path.Combine("Resources", resource));
            // Use the stream here
            //test 

            var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
            Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
            Assert.True(result.Success);
            Assert.True(result.ParserName == "NEM12");
            Assert.True(result.Sites > 0);
            Assert.True(result.Errors == 0);
            Assert.True(result.Warnings == 1);
            Assert.True(result.TotalSiteDays > 0);
        }

    }

}