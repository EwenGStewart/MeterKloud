using FluentAssertions;
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
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
               
                var result  = await  ParserFactory.ParseAsync(stream, resource, mimeType);
                Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
                Assert.True(result.Success);
                Assert.True(result.ParserName == "NEM12");
                Assert.True(result.Sites > 0);
                Assert.True(result.Errors == 0);
                Assert.True(result.TotalSiteDays > 0);
            }
        }
    }

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


    public class CsvSingleLineMultiColPeriod(ITestOutputHelper Output)
    {
        [Theory]
        [InlineData("61029293722_meter_usage_data_cipc1_05_2015.csv", "text/csv")]

        public async Task CsvSingleLineMultiColPeriod_test1(string resource, string mimeType)
        {
            Console.SetOut(new RedirectOutput(Output));
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
                Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
                Assert.True(result.Success);
                Assert.True(result.ParserName == "SingleLineByPeriod");

            }
        }


        [Theory]
        [InlineData("SampleCsv8.csv", "text/csv")]

        public async Task CsvSingleLineMultiColPeriod2_test1(string resource, string mimeType)
        {
            Console.SetOut(new RedirectOutput(Output));
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
                Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
                Assert.True(result.Success);
                Assert.True(result.ParserName == "SingleLineByPeriod2");

            }
        }


    }

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
                decimal actualTotal = result.SitesDays.SelectMany(x => x.Channels.Values).Where(x => x.ChannelType == MeterDataLib.ChanelType.ActivePowerConsumption).Sum(x => x.Total);
                actualTotal.Should().Be(expectedTotal);
                Assert.True(result.ParserName == "SingleLineWith_E_B_KVA_PF");


            }
        }
    }


    public class CsvSingleLinePeakOffPeakDateNumberTests(ITestOutputHelper Output)
    {
        [Theory]
        [InlineData("SampleCsvFormat2.csv", "text/csv" )]

        public async Task Parse1(string resource, string mimeType )
        {
            Console.SetOut(new RedirectOutput(Output));
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
                Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
                Assert.True(result.Success);
                Assert.True(result.ParserName == "SingleLineWith_PK_OP_DateNumber");


            }
        }
    }


    public class CsvSingleLine7Tests(ITestOutputHelper Output)
    {
        [Theory]
        [InlineData("SampleMultiLineCsv7.csv", "text/csv")]

        public async Task Parse1(string resource, string mimeType)
        {
            Console.SetOut(new RedirectOutput(Output));
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
                Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
                Assert.True(result.Success);
                Assert.True(result.ParserName == "SingleLineFromLine7");


            }
        }
    }

    public class CsvPowerPalTests(ITestOutputHelper Output)
    {
        [Theory]
        [InlineData("powerpal_data_0000f596 (1).csv", "text/csv")]

        public async Task Parse1(string resource, string mimeType)
        {
            Console.SetOut(new RedirectOutput(Output));
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
                Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
                Assert.True(result.Success);
                Assert.True(result.ParserName == "CsvPowerPal");



            }
        }
    }



    public class CsvByChannelTests(ITestOutputHelper Output)
    {
        [Theory]
        [InlineData("SampleCsvByChanel.csv", "text/csv")]

        public async Task Parse1(string resource, string mimeType)
        {
            Console.SetOut(new RedirectOutput(Output));
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
                Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
                Assert.True(result.Success);
                Assert.True(result.ParserName == "CsvByChannel");


            }
        }
    }


    public class ExcelTests(ITestOutputHelper Output)
    {
        [Theory]
        [InlineData("SampleExcelByChanel.XLSX", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [InlineData("SampleNem12Excel.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [InlineData("SampleNem12Excel2.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [InlineData("SampleNem12Excel3.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]

        public async Task Parse1(string resource, string mimeType)
        {
            Console.SetOut(new RedirectOutput(Output));
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
                Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
                Assert.True(result.Success);
                


            }
        }
    }


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