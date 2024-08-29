using FluentAssertions;
using MeterDataLib.Export;
using MeterDataLib.Parsers;
using System.Text;
using Xunit.Abstractions;

namespace TestMeterLib
{
    public class ExportTests(ITestOutputHelper Output)
    {
        [Fact]
        public async Task ExportToText()
        {
            var options = new ExportOptions
            {
                ExportType = ExportFormat.NEM12,
            };
            Console.SetOut(new RedirectOutput(Output));
            var filename = "powerpal_data_0000f596 (2).csv";
            ParserResult? parserResult = null;
            using (Stream stream = File.OpenRead(Path.Combine("Resources", filename)))
            {
                // Use the stream here
                //test 
                parserResult = await ParserFactory.ParseAsync(stream, filename, "text/csv", null);
                bool actualResult = parserResult.Success;
                Assert.True(parserResult.Success);
            }
            var siteDays = parserResult.SitesDays;
            options.SiteDays = siteDays;
            var nem12 = ExportData.Export(options);
            nem12.Should().NotBeNullOrEmpty();

        }




        [Theory]
        [InlineData("4103354306_20160604_20170605_20170605000000_EnergyAustralia_DETAILED (1).csv", "text/csv", true)]
        [InlineData("61029293722_meter_usage_data_cipc1_05_2015.csv", "text/csv", true)]
        [InlineData("6407276797_20150803_20160517_20160518160200_UNITEDENERGY_DETAILED.csv", "text/csv", true)]
        [InlineData("SampleNem", "", true)]
        [InlineData("SampleNem12.csv", "text/csv", true)]
        [InlineData("SampleNem12-2.csv", "text/csv", true)]
        [InlineData("SampleNem12-3.csv", "text/csv", true)]
        [InlineData("SampleNem12-4.CSV", "text/csv", true)]
        [InlineData("MultiLineCsv1.csv", "text/csv", true)]
        [InlineData("SampleCsv.csv", "text/csv", true)]
        [InlineData("SampleCsv3.csv", "text/csv", true)]
        [InlineData("SampleCsv4.csv", "text/csv", true)]
        [InlineData("SampleCsv5.csv", "text/csv", true)]
        [InlineData("SampleCsv8.csv", "text/csv", true)]
        [InlineData("SampleCsvFormat2.csv", "text/csv", true)]
        [InlineData("SampleExcelByChanel.XLSX", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", true)]
        [InlineData("SampleMultiLineCsv7.csv", "text/csv", true)]
        [InlineData("SampleNem12Excel.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", true)]
        [InlineData("SampleNem12Excel2.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", true)]
        [InlineData("SampleNem12Excel3.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", true)]
        [InlineData("SampleNem12Zip.zip", "application/x-zip-compressed", true)]
        [InlineData("powerpal_data_0000f596 (1).csv", "text/csv", true)]
        [InlineData("powerpal_data_0000f596 (2).csv", "text/csv", true)]
        [InlineData("powerpal_data_0000f596 (3).csv", "text/csv", true)]
        [InlineData("powerpal_data_0000f596 (4).csv", "text/csv", true)]
        [InlineData("SampleCsvByChanel.csv", "text/csv", true)]

        public async Task ParseFile(string filename, string contentType, bool _)
        {
            ParserResult? parserResult = null;
            using (Stream stream = File.OpenRead(Path.Combine("Resources", filename)))
            {
                // Use the stream here
                //test 
                parserResult = await ParserFactory.ParseAsync(stream, filename, contentType, null);
                bool actualResult = parserResult.Success;
                Assert.True(parserResult.Success);
            }
            var options = new ExportOptions
            {
                ExportType = ExportFormat.NEM12,
            };

            var siteDays = parserResult.SitesDays;
            options.SiteDays = siteDays;
            decimal totalEnergy = 0;
            foreach (var siteDay in siteDays)
            {
                totalEnergy += siteDay.EnergyDailySummary.TotalActivePowerConsumption_kWh;
            }
            var nem12 = ExportData.Export(options);
            nem12.Should().NotBeNullOrEmpty();

            // parse it back 
            var parserResult2 = await ParserFactory.ParseAsync(new MemoryStream(Encoding.UTF8.GetBytes(nem12)), "exported.csv", "text/csv", null);

            decimal totalEnergy2 = 0;
            foreach (var siteDay in parserResult2.SitesDays)
            {
                totalEnergy2 += siteDay.EnergyDailySummary.TotalActivePowerConsumption_kWh;
            }
            totalEnergy2.Should().Be(totalEnergy);

        }
















    }

}
