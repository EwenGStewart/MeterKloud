using FluentAssertions;
using MeterDataLib.Parsers;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace TestMeterLib
{
    public class ParserDetectionTests
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



        public void Any(string resource, string mimeType, bool expectedResult)
        {
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var parser = ParserFactory.GetParser(stream, resource, mimeType);
                bool actualResult = parser != null;
                Assert.Equal(expectedResult, actualResult);
            }
        }



        [Theory]
        [InlineData("4103354306_20160604_20170605_20170605000000_EnergyAustralia_DETAILED (1).csv", "text/csv"  )]
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



        public void ParseAll(string resource, string mimeType)
        {
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var result  = ParserFactory.Parse(stream, resource, mimeType);
           
                    result.Should().NotBeNull();
                    result.Errors.Should().Be(0);
                    result.SitesDays.Count.Should().BeGreaterThan(0);
                    result.FileName.Should().Be(resource);
                    result.ParserName.Should().NotBeNullOrEmpty();
             
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

        public void AnyWithParse(string resource, string mimeType)
        {
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var parser = ParserFactory.GetParser(stream, resource, mimeType);
                bool actualResult = parser != null;
                actualResult.Should().BeTrue();
                if (parser != null)
                {
                    var result = parser.Parse(stream, resource);
                    result.Should().NotBeNull();
                    result.Errors.Should().Be(0);
                    result.SitesDays.Count.Should().BeGreaterThan(0);
                    result.FileName.Should().Be(resource);
                    result.ParserName.Should().NotBeNullOrEmpty();
                }
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
        [InlineData("SampleNem12Excel.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", false)]
        [InlineData("SampleNem12Excel2.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", false)]
        [InlineData("SampleNem12Excel3.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", false)]
        [InlineData("SampleNem12Zip.zip", "application/x-zip-compressed", false)]

 


        public void Nem12(string resource, string mimeType, bool isNem12)
        {
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var nem12 = new MeterDataLib.Parsers.Nem12();
                var resultIsNem12 =nem12.CanParse(stream, resource, mimeType);
                Assert.Equal(isNem12, resultIsNem12);
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


        public void CsvMulti1(string resource, string mimeType, bool expectedResult)
        {
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var parser = new MeterDataLib.Parsers.CsvMultiLine1(); 
                bool result = parser.CanParse(stream, resource, mimeType); 
                Assert.Equal(expectedResult, result);
            }
        }

        [Theory]
        [InlineData("SampleCsv.csv", "text/csv", true)]
        public void Detect_CsvSingleLineSimpleEBKvaPF(string resource, string mimeType, bool expectedResult)
        {
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var parser = new MeterDataLib.Parsers.CsvSingleLineSimpleEBKvaPF();
                bool result = parser.CanParse(stream, resource, mimeType);
                Assert.Equal(expectedResult, result);
            }
        }


        [Theory]
        [InlineData("SampleCsv4.csv", "text/csv", true)]
        [InlineData("SampleCsv5.csv", "text/csv", true)]
        public void Detect_CsvSingleLineSimpleEBQK(string resource, string mimeType, bool expectedResult)
        {
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var parser = new MeterDataLib.Parsers.CsvSingleLineSimpleEBQK();
                bool result = parser.CanParse(stream, resource, mimeType);
                Assert.Equal(expectedResult, result);
            }
        }

        [Theory]
        [InlineData("SampleCsv8.csv", "text/csv", true)]
        
        public void Detect_CsvSingleLineMultiColPeriod2(string resource, string mimeType, bool expectedResult)
        {
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var parser = new MeterDataLib.Parsers.CsvSingleLineMultiColPeriod2();
                bool result = parser.CanParse(stream, resource, mimeType);
                Assert.Equal(expectedResult, result);
            }
        }


        [Theory]
        [InlineData("SampleCsvFormat2.csv", "text/csv", true)]

        public void Detect_CsvSingleLinePeakOffPeakDateNumber(string resource, string mimeType, bool expectedResult)
        {
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var parser = new MeterDataLib.Parsers.CsvSingleLinePeakOffPeakDateNumber();
                bool result = parser.CanParse(stream, resource, mimeType);
                Assert.Equal(expectedResult, result);
            }
        }



    }


}