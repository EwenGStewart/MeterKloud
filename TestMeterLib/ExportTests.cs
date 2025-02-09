using FluentAssertions;
using MeterDataLib;
using MeterDataLib.Export;
using MeterDataLib.Parsers;
using System.IO.Compression;
using System.Text;
using Xunit.Abstractions;

namespace TestMeterLib
{
    public class ExportTests(ITestOutputHelper Output)
    {
        private const string ExcelMimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        private const string TestNEM12FileNAme = "61029293722_meter_usage_data_cipc1_05_2015.csv";

        [Fact]
        public async Task OptionsDontChange()
        {
            var options = new ExportOptions
            {
                ExportType = ExportFormat.NEM12,
            };
            Console.SetOut(new RedirectOutput(Output));
            var filename = TestNEM12FileNAme;
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
            options.Site = new Site() { Code = siteDays.First().SiteCode };
            options.SiteDays = siteDays;
            var nem12 = ExportData.ExportSiteDataToText(options);
            nem12.Should().NotBeNullOrEmpty();
            options.Site.Should().NotBeNull();
            options.Site.Code.Should().Be(siteDays.First().SiteCode);
        }



        [Fact]
        public async Task ExportToNEM12()
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
            var nem12 = ExportData.ExportSiteDataToText(options);
            nem12.Should().NotBeNullOrEmpty();

        }


        [Fact]
        public async Task ExportToQuads_SiteLevel()
        {
            var options = new ExportOptions
            {
                ExportType = ExportFormat.QuadrantCSV,
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
            var exportFile = ExportData.ExportSiteDataToText(options);
            exportFile.Should().NotBeNullOrEmpty();
        }


        [Fact]
        public async Task ExportToQuads_SiteLevel_30Min()
        {
            var options = new ExportOptions
            {
                ExportType = ExportFormat.QuadrantCSV,
                IntervalInMinutes = 30,
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
            var exportFile = ExportData.ExportSiteDataToText(options);
            exportFile.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ExportToQuads_MeterChanLevel_30Min()
        {
            var options = new ExportOptions
            {
                ExportType = ExportFormat.QuadrantCSV,
                IntervalInMinutes = 30,
                IncludeMeter = true,
                IncludeChannel = true,
            };
            Console.SetOut(new RedirectOutput(Output));
            var filename = "SampleNem12Excel.xlsx";
            ParserResult? parserResult = null;
            using (Stream stream = File.OpenRead(Path.Combine("Resources", filename)))
            {
                // Use the stream here
                //test 
                parserResult = await ParserFactory.ParseAsync(stream, filename, ExcelMimeType, null);
                bool actualResult = parserResult.Success;
                Assert.True(parserResult.Success);
            }
            var siteDays = parserResult.SitesDays;
            options.SiteDays = siteDays;
            var exportFile = ExportData.ExportSiteDataToText(options);
            exportFile.Should().NotBeNullOrEmpty();
        }








        [Fact]
        public async Task ExportToCols_MeterChanLevel()
        {
            var options = new ExportOptions
            {
                ExportType = ExportFormat.ColumnarCSV,
                IncludeMeter = true,
                IncludeChannel = true,
            };
            Console.SetOut(new RedirectOutput(Output));
            var filename = "SampleNem12Excel.xlsx";
            ParserResult? parserResult = null;
            using (Stream stream = File.OpenRead(Path.Combine("Resources", filename)))
            {
                // Use the stream here
                //test 
                parserResult = await ParserFactory.ParseAsync(stream, filename, ExcelMimeType, null);
                bool actualResult = parserResult.Success;
                Assert.True(parserResult.Success);
            }
            var siteDays = parserResult.SitesDays;
            options.SiteDays = siteDays;
            var exportFile = ExportData.ExportSiteDataToText(options);
            exportFile.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ExportToCols()
        {
            var options = new ExportOptions
            {
                ExportType = ExportFormat.ColumnarCSV,
                IncludeMeter = false,
                IncludeChannel = true,
            };
            Console.SetOut(new RedirectOutput(Output));
            var filename = "SampleNem12Excel.xlsx";
            ParserResult? parserResult = null;
            using (Stream stream = File.OpenRead(Path.Combine("Resources", filename)))
            {
                // Use the stream here
                //test 
                parserResult = await ParserFactory.ParseAsync(stream, filename, ExcelMimeType, null);
                bool actualResult = parserResult.Success;
                Assert.True(parserResult.Success);
            }
            var siteDays = parserResult.SitesDays;
            options.SiteDays = siteDays;
            var exportFile = ExportData.ExportSiteDataToText(options);
            exportFile.Should().NotBeNullOrEmpty();
        }



        [Fact]
        public async Task ExportToCols_AllLevel()
        {
            var options = new ExportOptions
            {
                ExportType = ExportFormat.ColumnarCSV,
                IncludeSite = false,
            };
            Console.SetOut(new RedirectOutput(Output));
            var filename = "SampleNem12Excel.xlsx";
            ParserResult? parserResult = null;
            using (Stream stream = File.OpenRead(Path.Combine("Resources", filename)))
            {
                // Use the stream here
                //test 
                parserResult = await ParserFactory.ParseAsync(stream, filename, ExcelMimeType, null);
                bool actualResult = parserResult.Success;
                Assert.True(parserResult.Success);
            }
            var siteDays = parserResult.SitesDays;
            options.SiteDays = siteDays;
            var exportFile = ExportData.ExportSiteDataToText(options);
            exportFile.Should().NotBeNullOrEmpty();

        }


        [Fact]
        public async Task ExportToRows_AllLevel()
        {
            var options = new ExportOptions
            {
                ExportType = ExportFormat.RowCSV,
                IncludeSite = false,
            };
            Console.SetOut(new RedirectOutput(Output));
            var filename = "SampleNem12Excel.xlsx";
            ParserResult? parserResult = null;
            using (Stream stream = File.OpenRead(Path.Combine("Resources", filename)))
            {
                // Use the stream here
                //test 
                parserResult = await ParserFactory.ParseAsync(stream, filename, ExcelMimeType, null);
                bool actualResult = parserResult.Success;
                Assert.True(parserResult.Success);
            }
            var siteDays = parserResult.SitesDays;
            options.SiteDays = siteDays;
            var exportFile = ExportData.ExportSiteDataToText(options);
            exportFile.Should().NotBeNullOrEmpty();

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
        //[InlineData("SampleExcelByChanel.XLSX", ExcelMimeType, true)]
        [InlineData("SampleMultiLineCsv7.csv", "text/csv", true)]
        [InlineData("SampleNem12Excel.xlsx", ExcelMimeType, true)]
        [InlineData("SampleNem12Excel2.xlsx", ExcelMimeType, true)]
        [InlineData("SampleNem12Excel3.xlsx", ExcelMimeType, true)]
        [InlineData("SampleNem12Zip.zip", "application/x-zip-compressed", true)]
        [InlineData("powerpal_data_0000f596 (1).csv", "text/csv", true)]
        [InlineData("powerpal_data_0000f596 (2).csv", "text/csv", true)]
        [InlineData("powerpal_data_0000f596 (3).csv", "text/csv", true)]
        [InlineData("powerpal_data_0000f596 (4).csv", "text/csv", true)]
        //[InlineData("SampleCsvByChanel.csv", "text/csv", true)]

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
            decimal totalConsumption = 0;
            decimal totalGen = 0;
            decimal totalReactiveConsumption = 0;
            decimal totalReactiveGen = 0;
            foreach (var siteDay in siteDays)
            {
                totalConsumption += siteDay.EnergyDailySummary.TotalActiveEnergyConsumption_kWh;
                totalGen += siteDay.EnergyDailySummary.TotalActiveEnergyGeneration_kWh;
                totalReactiveConsumption += siteDay.EnergyDailySummary.TotalReactiveEnergyConsumption_kVArh;
                totalReactiveGen += siteDay.EnergyDailySummary.TotalReactiveEnergyGeneration_kVArh;
            }
            var nem12 = ExportData.ExportSiteDataToText(options);
            nem12.Should().NotBeNullOrEmpty();

            // parse it back 
            var parserResult2 = await ParserFactory.ParseAsync(new MemoryStream(Encoding.UTF8.GetBytes(nem12)), "exported.csv", "text/csv", null);

            decimal totalConsumption2 = 0;
            decimal totalGen2 = 0;
            decimal totalReactiveConsumption2 = 0;
            decimal totalReactiveGen2 = 0;
            foreach (var siteDay in parserResult2.SitesDays)
            {
                totalConsumption2 += siteDay.EnergyDailySummary.TotalActiveEnergyConsumption_kWh;
                totalGen2 += siteDay.EnergyDailySummary.TotalActiveEnergyGeneration_kWh;
                totalReactiveConsumption2 += siteDay.EnergyDailySummary.TotalReactiveEnergyConsumption_kVArh;
                totalReactiveGen2 += siteDay.EnergyDailySummary.TotalReactiveEnergyGeneration_kVArh;
            }
            totalConsumption2.Should().Be(totalConsumption);
            totalGen2.Should().Be(totalGen);
            totalReactiveConsumption2.Should().Be(totalReactiveConsumption);
            totalReactiveGen2.Should().Be(totalReactiveGen);

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
        //[InlineData("SampleExcelByChanel.XLSX", ExcelMimeType, true)]
        [InlineData("SampleMultiLineCsv7.csv", "text/csv", true)]
        [InlineData("SampleNem12Excel.xlsx", ExcelMimeType, true)]
        [InlineData("SampleNem12Excel2.xlsx", ExcelMimeType, true)]
        [InlineData("SampleNem12Excel3.xlsx", ExcelMimeType, true)]
        [InlineData("SampleNem12Zip.zip", "application/x-zip-compressed", true)]
        [InlineData("powerpal_data_0000f596 (1).csv", "text/csv", true)]
        [InlineData("powerpal_data_0000f596 (2).csv", "text/csv", true)]
        [InlineData("powerpal_data_0000f596 (3).csv", "text/csv", true)]
        [InlineData("powerpal_data_0000f596 (4).csv", "text/csv", true)]
        //[InlineData("SampleCsvByChanel.csv", "text/csv", true)]

        public async Task ParseFileFor30Min(string filename, string contentType, bool _)
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
                IntervalInMinutes = 30,

            };

            var siteDays = parserResult.SitesDays;
            options.SiteDays = siteDays;
            decimal totalConsumption = 0;
            decimal totalGen = 0;
            decimal totalReactiveConsumption = 0;
            decimal totalReactiveGen = 0;
            foreach (var siteDay in siteDays)
            {
                totalConsumption += siteDay.EnergyDailySummary.TotalActiveEnergyConsumption_kWh;
                totalGen += siteDay.EnergyDailySummary.TotalActiveEnergyGeneration_kWh;
                totalReactiveConsumption += siteDay.EnergyDailySummary.TotalReactiveEnergyConsumption_kVArh;
                totalReactiveGen += siteDay.EnergyDailySummary.TotalReactiveEnergyGeneration_kVArh;
            }
            var nem12 = ExportData.ExportSiteDataToText(options);
            nem12.Should().NotBeNullOrEmpty();

            // parse it back 
            var parserResult2 = await ParserFactory.ParseAsync(new MemoryStream(Encoding.UTF8.GetBytes(nem12)), "exported.csv", "text/csv", null);

            decimal totalConsumption2 = 0;
            decimal totalGen2 = 0;
            decimal totalReactiveConsumption2 = 0;
            decimal totalReactiveGen2 = 0;
            foreach (var siteDay in parserResult2.SitesDays)
            {
                totalConsumption2 += siteDay.EnergyDailySummary.TotalActiveEnergyConsumption_kWh;
                totalGen2 += siteDay.EnergyDailySummary.TotalActiveEnergyGeneration_kWh;
                totalReactiveConsumption2 += siteDay.EnergyDailySummary.TotalReactiveEnergyConsumption_kVArh;
                totalReactiveGen2 += siteDay.EnergyDailySummary.TotalReactiveEnergyGeneration_kVArh;
            }
            totalConsumption2.Should().Be(totalConsumption);
            totalGen2.Should().Be(totalGen);
            totalReactiveConsumption2.Should().Be(totalReactiveConsumption);
            totalReactiveGen2.Should().Be(totalReactiveGen);

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
        //[InlineData("SampleExcelByChanel.XLSX", ExcelMimeType, true)]
        [InlineData("SampleMultiLineCsv7.csv", "text/csv", true)]
        [InlineData("SampleNem12Excel.xlsx", ExcelMimeType, true)]
        [InlineData("SampleNem12Excel2.xlsx", ExcelMimeType, true)]
        [InlineData("SampleNem12Excel3.xlsx", ExcelMimeType, true)]
        [InlineData("SampleNem12Zip.zip", "application/x-zip-compressed", true)]
        [InlineData("powerpal_data_0000f596 (1).csv", "text/csv", true)]
        [InlineData("powerpal_data_0000f596 (2).csv", "text/csv", true)]
        [InlineData("powerpal_data_0000f596 (3).csv", "text/csv", true)]
        [InlineData("powerpal_data_0000f596 (4).csv", "text/csv", true)]
        //[InlineData("SampleCsvByChanel.csv", "text/csv", true)]

        public async Task ParseFileFor5Min(string filename, string contentType, bool _)
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
                IntervalInMinutes = 5,

            };

            var siteDays = parserResult.SitesDays;
            options.SiteDays = siteDays;
            decimal totalConsumption = 0;
            decimal totalGen = 0;
            decimal totalReactiveConsumption = 0;
            decimal totalReactiveGen = 0;
            foreach (var siteDay in siteDays)
            {
                totalConsumption += siteDay.EnergyDailySummary.TotalActiveEnergyConsumption_kWh;
                totalGen += siteDay.EnergyDailySummary.TotalActiveEnergyGeneration_kWh;
                totalReactiveConsumption += siteDay.EnergyDailySummary.TotalReactiveEnergyConsumption_kVArh;
                totalReactiveGen += siteDay.EnergyDailySummary.TotalReactiveEnergyGeneration_kVArh;
            }
            var nem12 = ExportData.ExportSiteDataToText(options);
            nem12.Should().NotBeNullOrEmpty();

            // parse it back 
            var parserResult2 = await ParserFactory.ParseAsync(new MemoryStream(Encoding.UTF8.GetBytes(nem12)), "exported.csv", "text/csv", null);

            decimal totalConsumption2 = 0;
            decimal totalGen2 = 0;
            decimal totalReactiveConsumption2 = 0;
            decimal totalReactiveGen2 = 0;
            foreach (var siteDay in parserResult2.SitesDays)
            {
                totalConsumption2 += siteDay.EnergyDailySummary.TotalActiveEnergyConsumption_kWh;
                totalGen2 += siteDay.EnergyDailySummary.TotalActiveEnergyGeneration_kWh;
                totalReactiveConsumption2 += siteDay.EnergyDailySummary.TotalReactiveEnergyConsumption_kVArh;
                totalReactiveGen2 += siteDay.EnergyDailySummary.TotalReactiveEnergyGeneration_kVArh;
            }
            totalConsumption2.Should().Be(totalConsumption);
            totalGen2.Should().Be(totalGen);
            totalReactiveConsumption2.Should().Be(totalReactiveConsumption);
            totalReactiveGen2.Should().Be(totalReactiveGen);

        }



        [Fact]
        public async Task MutliSiteExportToZip()
        {

            string[] filenames = new string[] {
                "4103354306_20160604_20170605_20170605000000_EnergyAustralia_DETAILED (1).csv",
                   "6407276797_20150803_20160517_20160518160200_UNITEDENERGY_DETAILED.csv",
                   "6407274172_20140506_20160506_20160507211600_UNITEDENERGY_DETAILED.csv",
                   "64082253748_20220812_20240812_20240813120626_UNITEDENERGY_DETAILED.csv" };
            string contentType = "text/csv";



            List<SiteDay> siteDays = new List<SiteDay>();
            foreach (var filename in filenames)
            {

                ParserResult? parserResult = null;
                using (Stream stream = File.OpenRead(Path.Combine("Resources", filename)))
                {
                    // Use the stream here
                    //test 
                    parserResult = await ParserFactory.ParseAsync(stream, filename, contentType, null);
                    bool actualResult = parserResult.Success;
                    Assert.True(parserResult.Success);
                    foreach (var siteDay in parserResult.SitesDays)
                    {

                        siteDays.Add(siteDay);
                    }
                }
            }

            var options = new ExportOptions
            {
                ExportType = ExportFormat.NEM12,
                IntervalInMinutes = 30,
                SiteDays = siteDays,

            };
            var result = await ExportData.ExportMultiSitesToMultiFiles(options);

            result.isZip.Should().BeTrue();
            result.preview.Should().NotBeNullOrEmpty();

            var parserResult2 = await ParserFactory.ParseAsync( result.file, "sample.zip", "application/x-zip-compressed", null);
            parserResult2.Success.Should().BeTrue();
            parserResult2.SitesDays.Count.Should().Be(siteDays.Count);

           


        }

    }

}
