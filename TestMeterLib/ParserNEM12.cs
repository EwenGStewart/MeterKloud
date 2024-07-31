using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Xunit.Abstractions;

namespace TestMeterLib
{
    public class ParserNEM12
    {
        [Theory]
        [InlineData("4103354306_20160604_20170605_20170605000000_EnergyAustralia_DETAILED (1).csv", "text/csv")]
        [InlineData("6407276797_20150803_20160517_20160518160200_UNITEDENERGY_DETAILED.csv", "text/csv")]
        [InlineData("SampleNem", "")]
        [InlineData("SampleNem12.csv", "text/csv")]
        [InlineData("SampleNem12-2.csv", "text/csv")]
        [InlineData("SampleNem12-3.csv", "text/csv")]
        [InlineData("SampleNem12-4.CSV", "text/csv")]

        public void Nem12(string resource, string mimeType)
        {
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var nem12 = MeterDataLib.Parsers.Nem12.GetParser(stream, resource, mimeType);
                bool resultIsNem12 = nem12 != null;
                Assert.True(resultIsNem12);
                if ( nem12 == null )
                {
                   Assert.Fail("Nem12 parser not detected");
                }
                var result = nem12.Parse(stream, resource);
                Assert.True(result.Success);

            }
        }
    }

    public class CsvMultiLine1
    {
        [Theory]
        [InlineData("MultiLineCsv1.csv", "text/csv")]

        public void CsvMultiLine1Test1(string resource, string mimeType)
        {
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var parser = MeterDataLib.Parsers.CsvMultiLine1.GetParser(stream, resource, mimeType);
                bool resultIsValid = parser != null;
                Assert.True(resultIsValid);
                if (parser == null)
                {
                    Assert.Fail("parser not detected");
                }
                var result = parser.Parse(stream, resource);
                Assert.True(result.Success);

            }
        }
    }


    public class CsvSingleLineMultiColPeriod
    {
        [Theory]
        [InlineData("61029293722_meter_usage_data_cipc1_05_2015.csv", "text/csv")]

        public void CsvSingleLineMultiColPeriod_test1(string resource, string mimeType)
        {
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var parser = MeterDataLib.Parsers.CsvSingleLineMultiColPeriod.GetParser(stream, resource, mimeType);
                bool resultIsValid = parser != null;
                Assert.True(resultIsValid);
                if (parser == null)
                {
                    Assert.Fail("parser not detected");
                }
                var result = parser.Parse(stream, resource);
                Assert.True(result.Success);

            }
        }


        [Theory]
        [InlineData("SampleCsv8.csv", "text/csv")]

        public void CsvSingleLineMultiColPeriod2_test1(string resource, string mimeType)
        {
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var parser = MeterDataLib.Parsers.CsvSingleLineMultiColPeriod2.GetParser(stream, resource, mimeType);
                bool resultIsValid = parser != null;
                Assert.True(resultIsValid);
                if (parser == null)
                {
                    Assert.Fail("parser not detected");
                }
                var result = parser.Parse(stream, resource);
                Assert.True(result.Success);

            }
        }


    }

    public class CsvSingleLineSimpleEBKvaPFTests
    {
        [Theory]
        [InlineData("SampleCsv.csv", "text/csv" , 577 , 578263.04 )]

        public void Parse1(string resource, string mimeType , int expectedDays , decimal expectedTotal)
        {
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var parser = MeterDataLib.Parsers.CsvSingleLineSimpleEBKvaPF.GetParser(stream, resource, mimeType);
                bool resultIsValid = parser != null;
                Assert.True(resultIsValid);
                if (parser == null)
                {
                    Assert.Fail("parser not detected");
                }
                var result = parser.Parse(stream, resource);
                Assert.True(result.Success);
                int actualDays = result.SitesDays.Count;
                actualDays.Should().Be(expectedDays);
                decimal actualTotal = result.SitesDays.SelectMany(x=>x.Channels.Values).Where(x=>x.ChannelType== MeterDataLib.ChanelType.ActivePowerConsumption).Sum(x => x.Total);
                actualTotal.Should().Be(expectedTotal);


            }
        }
    }


    public class CsvSingleLinePeakOffPeakDateNumberTests
    {
        [Theory]
        [InlineData("SampleCsvFormat2.csv", "text/csv" )]

        public void Parse1(string resource, string mimeType )
        {
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var parser = MeterDataLib.Parsers.CsvSingleLinePeakOffPeakDateNumber.GetParser(stream, resource, mimeType);
                bool resultIsValid = parser != null;
                Assert.True(resultIsValid);
                if (parser == null)
                {
                    Assert.Fail("parser not detected");
                }
                var result = parser.Parse(stream, resource);
                Assert.True(result.Success);
                int actualDays = result.SitesDays.Count;
                actualDays.Should().BeGreaterThan(1);
                decimal actualTotal = result.SitesDays.SelectMany(x => x.Channels.Values).Where(x => x.ChannelType == MeterDataLib.ChanelType.ActivePowerConsumption).Sum(x => x.Total);
                actualTotal.Should().BeGreaterThan(0);


            }
        }
    }


    public class CsvSingleLine7Tests
    {
        [Theory]
        [InlineData("SampleMultiLineCsv7.csv", "text/csv")]

        public void Parse1(string resource, string mimeType)
        {
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var parser = MeterDataLib.Parsers.CsvSingleLine7.GetParser(stream, resource, mimeType);
                bool resultIsValid = parser != null;
                Assert.True(resultIsValid);
                if (parser == null)
                {
                    Assert.Fail("parser not detected");
                }
                var result = parser.Parse(stream, resource);
                Assert.True(result.Success);
                int actualDays = result.SitesDays.Count;
                actualDays.Should().BeGreaterThan(1);
                decimal actualTotal = result.SitesDays.SelectMany(x => x.Channels.Values).Where(x => x.ChannelType == MeterDataLib.ChanelType.ActivePowerConsumption).Sum(x => x.Total);
                actualTotal.Should().BeGreaterThan(0);


            }
        }
    }

    public class CsvPowerPalTests
    {
        [Theory]
        [InlineData("powerpal_data_0000f596 (1).csv", "text/csv")]

        public void Parse1(string resource, string mimeType)
        {
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var parser = MeterDataLib.Parsers.CsvPowerPal.GetParser(stream, resource, mimeType);
                bool resultIsValid = parser != null;
                Assert.True(resultIsValid);
                if (parser == null)
                {
                    Assert.Fail("parser not detected");
                }
                var result = parser.Parse(stream, resource);
                Assert.True(result.Success);
                int actualDays = result.SitesDays.Count;
                actualDays.Should().BeGreaterThan(1);
                decimal actualTotal = result.SitesDays.SelectMany(x => x.Channels.Values).Where(x => x.ChannelType == MeterDataLib.ChanelType.ActivePowerConsumption).Sum(x => x.Total);
                actualTotal.Should().BeGreaterThan(0);


            }
        }
    }



    public class CsvByChannelTests
    {
        [Theory]
        [InlineData("SampleCsvByChanel.csv", "text/csv")]

        public void Parse1(string resource, string mimeType)
        {
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var parser = MeterDataLib.Parsers.CsvByChannel.GetParser(stream, resource, mimeType);
                bool resultIsValid = parser != null;
                Assert.True(resultIsValid);
                if (parser == null)
                {
                    Assert.Fail("parser not detected");
                }
                var result = parser.Parse(stream, resource);
                Assert.True(result.Success);
                int actualDays = result.SitesDays.Count;
                actualDays.Should().BeGreaterThan(1);
                decimal actualTotal = result.SitesDays.SelectMany(x => x.Channels.Values).Where(x => x.ChannelType == MeterDataLib.ChanelType.ActivePowerConsumption).Sum(x => x.Total);
                actualTotal.Should().BeGreaterThan(0);


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

        public void Parse1(string resource, string mimeType)
        {
            Console.SetOut(new RedirectOutput(Output));
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var parser = MeterDataLib.Parsers.ExcelParser.GetParser(stream, resource, mimeType);
                bool resultIsValid = parser != null;
                Assert.True(resultIsValid);
                if (parser == null)
                {
                    Assert.Fail("parser not detected");
                }
                var result = parser.Parse(stream, resource);
                Assert.True(result.Success);
                int actualDays = result.SitesDays.Count;
                actualDays.Should().BeGreaterThan(1);
                decimal actualTotal = result.SitesDays.SelectMany(x => x.Channels.Values).Where(x => x.ChannelType == MeterDataLib.ChanelType.ActivePowerConsumption).Sum(x => x.Total);
                actualTotal.Should().BeGreaterThan(0);


            }
        }
    }


    public class ZipTests(ITestOutputHelper Output)
    {
        [Theory]
        [InlineData("SampleNem12Zip.zip", "application/x-zip-compressed")]
        
        public void Parse1(string resource, string mimeType)
        {
            Console.SetOut(new RedirectOutput(Output));
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var parser = MeterDataLib.Parsers.ZipParser.GetParser(stream, resource, mimeType);
                bool resultIsValid = parser != null;
                Assert.True(resultIsValid);
                if (parser == null)
                {
                    Assert.Fail("parser not detected");
                }
                var result = parser.Parse(stream, resource);
                Assert.True(result.Success);
                int actualDays = result.SitesDays.Count;
                actualDays.Should().BeGreaterThan(1);
                decimal actualTotal = result.SitesDays.SelectMany(x => x.Channels.Values).Where(x => x.ChannelType == MeterDataLib.ChanelType.ActivePowerConsumption).Sum(x => x.Total);
                actualTotal.Should().BeGreaterThan(0);


            }
        }
    }

}