using MeterDataLib.Parsers;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Xunit.Abstractions;

namespace TestMeterLib
{


    public class ParserDetectionTests(ITestOutputHelper Output)
    {

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



        public async Task Any(string resource, string mimeType, bool expectedResult)
        {
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var result = await ParserFactory.ParseAsync(stream, resource, mimeType, null );
                bool actualResult = result.Success;
                Assert.Equal(expectedResult, actualResult);
            }
        }



        [Theory]
        [InlineData("4103354306_20160604_20170605_20170605000000_EnergyAustralia_DETAILED (1).csv", "text/csv")]
        [InlineData("61029293722_meter_usage_data_cipc1_05_2015.csv", "text/csv")]
        [InlineData("6407276797_20150803_20160517_20160518160200_UNITEDENERGY_DETAILED.csv", "text/csv")]
        [InlineData("SampleNem", "")]
        [InlineData("SampleNem12.csv", "text/csv")]
        [InlineData("SampleNem12-2.csv", "text/csv")]
        [InlineData("SampleNem12-3.csv", "text/csv")]
        [InlineData("SampleNem12-4.CSV", "text/csv")]


        [InlineData("MultiLineCsv1.csv", "text/csv")]
        [InlineData("SampleCsv.csv", "text/csv")]
        [InlineData("SampleCsv3.csv", "text/csv")]
        [InlineData("SampleCsv4.csv", "text/csv")]
        [InlineData("SampleCsv5.csv", "text/csv")]
        [InlineData("SampleCsv8.csv", "text/csv")]
        [InlineData("SampleCsvFormat2.csv", "text/csv")]
        [InlineData("SampleExcelByChanel.XLSX", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [InlineData("SampleMultiLineCsv7.csv", "text/csv")]
        [InlineData("SampleNem12Excel.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [InlineData("SampleNem12Excel2.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [InlineData("SampleNem12Excel3.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [InlineData("SampleNem12Zip.zip", "application/x-zip-compressed")]
        [InlineData("powerpal_data_0000f596 (1).csv", "text/csv")]
        [InlineData("powerpal_data_0000f596 (2).csv", "text/csv")]
        [InlineData("powerpal_data_0000f596 (3).csv", "text/csv")]
        [InlineData("powerpal_data_0000f596 (4).csv", "text/csv")]
        [InlineData("SampleCsvByChanel.csv", "text/csv")]



        public async Task ParseAll(string resource, string mimeType)
        {
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
                bool actualResult = result.Success;
                Assert.True(actualResult);

            }
        }

        [Theory]
        [InlineData("4103354306_20160604_20170605_20170605000000_EnergyAustralia_DETAILED (1).csv", "text/csv")]
        [InlineData("61029293722_meter_usage_data_cipc1_05_2015.csv", "text/csv")]
        [InlineData("6407276797_20150803_20160517_20160518160200_UNITEDENERGY_DETAILED.csv", "text/csv")]
        [InlineData("SampleNem", "")]
        [InlineData("SampleNem12.csv", "text/csv")]
        [InlineData("SampleNem12-2.csv", "text/csv")]
        [InlineData("SampleNem12-3.csv", "text/csv")]
        [InlineData("SampleNem12-4.CSV", "text/csv")]
        [InlineData("MultiLineCsv1.csv", "text/csv")]
        [InlineData("SampleCsv.csv", "text/csv")]
        [InlineData("SampleCsv3.csv", "text/csv")]
        [InlineData("SampleCsv4.csv", "text/csv")]
        [InlineData("SampleCsv5.csv", "text/csv")]
        [InlineData("SampleCsv8.csv", "text/csv")]
        [InlineData("SampleCsvFormat2.csv", "text/csv")]
        [InlineData("SampleExcelByChanel.XLSX", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [InlineData("SampleMultiLineCsv7.csv", "text/csv")]
        [InlineData("SampleNem12Excel.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [InlineData("SampleNem12Excel2.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [InlineData("SampleNem12Excel3.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [InlineData("SampleNem12Zip.zip", "application/x-zip-compressed")]
        [InlineData("powerpal_data_0000f596 (1).csv", "text/csv")]
        [InlineData("powerpal_data_0000f596 (2).csv", "text/csv")]
        [InlineData("powerpal_data_0000f596 (3).csv", "text/csv")]
        [InlineData("powerpal_data_0000f596 (4).csv", "text/csv")]
        [InlineData("SampleCsvByChanel.csv", "text/csv")]

        public async Task AnyWithParse(string resource, string mimeType)
        {
            Console.SetOut(new RedirectOutput(Output));
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 

                Console.WriteLine("Parsing " + resource);
                var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
                Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
                foreach (var log in result.LogMessages)
                {
                    Console.WriteLine($"{log.LogLevel} {log.LogMessage}   Line:{log.LineNumber} Col:{log.ColumnNumber}  Filename:{log.FileName}");
                }

                bool actualResult = result.Success;
                Assert.True(actualResult);
            }
        }


        [Theory]
        [InlineData("4103354306_20160604_20170605_20170605000000_EnergyAustralia_DETAILED (1).csv", "text/csv", true)]
        [InlineData("61029293722_meter_usage_data_cipc1_05_2015.csv", "text/csv", false)]
        [InlineData("6407276797_20150803_20160517_20160518160200_UNITEDENERGY_DETAILED.csv", "text/csv", true)]
        [InlineData("SampleNem", "", true)]
        [InlineData("SampleNem12.csv", "text/csv", true)]
        [InlineData("SampleNem12-2.csv", "text/csv", true)]
        [InlineData("SampleNem12-3.csv", "text/csv", true)]
        [InlineData("SampleNem12-4.CSV", "text/csv", true)]


        [InlineData("MultiLineCsv1.csv", "text/csv", false)]
        [InlineData("SampleCsv.csv", "text/csv", false)]
        [InlineData("SampleCsv3.csv", "text/csv", false)]
        [InlineData("SampleCsv4.csv", "text/csv", false)]
        [InlineData("SampleCsv5.csv", "text/csv", false)]
        [InlineData("SampleCsv8.csv", "text/csv", false)]
        [InlineData("SampleCsvFormat2.csv", "text/csv", false)]
        [InlineData("SampleExcelByChanel.XLSX", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", false)]
        [InlineData("SampleMultiLineCsv7.csv", "text/csv", false)]
        [InlineData("SampleNem12Excel.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", true)]
        [InlineData("SampleNem12Excel2.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", true)]
        [InlineData("SampleNem12Excel3.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", true)]
        [InlineData("SampleNem12Zip.zip", "application/x-zip-compressed", true)]




        public async Task Nem12(string resource, string mimeType, bool isNem12)
        {
            Console.SetOut(new RedirectOutput(Output));
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
                Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
                bool actualResult = result.Success && result.ParserName == "NEM12";
                Assert.Equal(isNem12, actualResult);
            }
        }


        [Theory]
        [InlineData("MultiLineCsv1.csv", "text/csv", true)]
        [InlineData("4103354306_20160604_20170605_20170605000000_EnergyAustralia_DETAILED (1).csv", "text/csv", false)]
        [InlineData("61029293722_meter_usage_data_cipc1_05_2015.csv", "text/csv", false)]
        [InlineData("6407276797_20150803_20160517_20160518160200_UNITEDENERGY_DETAILED.csv", "text/csv", false)]
        [InlineData("SampleNem", "", false)]
        [InlineData("SampleNem12.csv", "text/csv", false)]
        [InlineData("SampleCsv.csv", "text/csv", false)]
        [InlineData("SampleCsv3.csv", "text/csv", false)]
        [InlineData("SampleCsv4.csv", "text/csv", false)]
        [InlineData("SampleCsvFormat2.csv", "text/csv", false)]
        [InlineData("SampleMultiLineCsv7.csv", "text/csv", false)]
        [InlineData("SampleNem12Excel.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", false)]


        public async Task CsvMulti1(string resource, string mimeType, bool expectedResult)
        {
            Console.SetOut(new RedirectOutput(Output));
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
                Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
                bool actualResult = result.Success && result.ParserName == "MultiLineCSV1";
                Assert.Equal(expectedResult, actualResult);
            }
        }

        [Theory]
        [InlineData("SampleCsv.csv", "text/csv", true)]
        public async Task Detect_CsvSingleLineSimpleEBKvaPF(string resource, string mimeType, bool expectedResult)
        {
            Console.SetOut(new RedirectOutput(Output));
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
                Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
                bool actualResult = result.Success && result.ParserName == "SingleLineWith_E_B_KVA_PF";
                Assert.Equal(expectedResult, actualResult); ;
            }
        }


        [Theory]
        [InlineData("SampleCsv4.csv", "text/csv", true)]
        [InlineData("SampleCsv5.csv", "text/csv", true)]
        public async Task Detect_CsvSingleLineSimpleEBQK(string resource, string mimeType, bool expectedResult)
        {
            Console.SetOut(new RedirectOutput(Output));
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
                Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
                bool actualResult = result.Success && result.ParserName == "SingleLineWith_E_B_K_Q";
                Assert.Equal(expectedResult, actualResult); ;
            }
        }

        [Theory]
        [InlineData("SampleCsv8.csv", "text/csv", true)]

        public async Task Detect_CsvSingleLineMultiColPeriod2(string resource, string mimeType, bool expectedResult)
        {
            Console.SetOut(new RedirectOutput(Output));
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
                Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
                bool actualResult = result.Success && result.ParserName == "SingleLineByPeriod2";
                Assert.Equal(expectedResult, actualResult); ;
            }
        }


        [Theory]
        [InlineData("SampleCsvFormat2.csv", "text/csv", true)]

        public async Task Detect_CsvSingleLinePeakOffPeakDateNumber(string resource, string mimeType, bool expectedResult)
        {
            Console.SetOut(new RedirectOutput(Output));
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
                Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
                bool actualResult = result.Success && result.ParserName == "SingleLineWith_PK_OP_DateNumber";
                Assert.Equal(expectedResult, actualResult); ;
            }
        }



    }

}
