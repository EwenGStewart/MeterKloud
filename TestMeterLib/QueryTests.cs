using FluentAssertions;
using MeterDataLib;
using MeterDataLib.Parsers;
using MeterDataLib.Query;
using Xunit.Abstractions;

namespace TestMeterLib
{
    public class QueryTests(ITestOutputHelper Output)
    {

        [Fact]
        public async Task ValidateNem12Sample()
        {
            var resource = "SampleNem12-2.csv";
            var mimeType = "text/csv";
            Console.SetOut(new RedirectOutput(Output));
            using Stream stream = File.OpenRead(Path.Combine("Resources", resource));
            // Use the stream here
            //test 

            var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
            Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");

            var fromDate = result.SitesDays.Select(x => x.Date).Min();
            var toDate = result.SitesDays.Select(x => x.Date).Max();
            var daily = MeterDataQuery.GetDailyNetConsumption(fromDate, toDate, result.SitesDays);
            daily.Should().NotBeNull();
            Console.WriteLine($"Profile: Days:{daily.Days} Range:{daily.ActualDateRange}  Max:{daily.MaxValue}@{daily.MaxDay:yyyy-MM-dd}     Avg:{daily.AvgDay}  Med:{daily.Median}  Sum:{daily.Profile.Sum()}   ");
            Console.WriteLine("-----------------------------------------------");
            for (int i = 0; i < daily.Profile.Length; i++)
            {
                Console.WriteLine($"{daily.Dates[i]:yyyy-MM-dd},{daily.Profile[i]}");
            }
            //Max:16143.1@2020-02-19   Min:4561.5@2020-02-19  Avg:9745.319398907103825136612022  Med:10201.1  Sum:3566786.9  

            daily.MaxValue.Should().BeApproximately(16143.1M, 0.1m);
            daily.MinValue.Should().BeApproximately(4561.5M, 0.1m);
            daily.Profile.Sum().Should().BeApproximately(3566786.9M, 0.1m);
        }

        [Fact]
        public async Task ValidateNem12SampleDetailed()
        {
            var resource = "SampleNem12-2.csv";
            var mimeType = "text/csv";
            Console.SetOut(new RedirectOutput(Output));
            using Stream stream = File.OpenRead(Path.Combine("Resources", resource));
            // Use the stream here
            //test 

            var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
            Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");

            var fromDate = result.SitesDays.Select(x => x.Date).Min();
            var toDate = result.SitesDays.Select(x => x.Date).Max();
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            var detailed = MeterDataQuery.GetDetailedNetConsumption(fromDate, toDate, result.SitesDays);
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");

            detailed.Should().NotBeNull();
            Console.WriteLine($"Profile: Days:{detailed.Days} Range:{detailed.ActualDateRange} ");
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            Console.WriteLine($"Max:{detailed.MaxValue}");
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            Console.WriteLine($"@{detailed.MaxDateTime:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            //Console.WriteLine($"Min:{detailed.MinValue}@{detailed.MinDateTime:yyyy-MM-dd HH:mm:ss}  ");
            //Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            Console.WriteLine($"Avg:{detailed.AvgDay}  ");
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            Console.WriteLine($"Med:{detailed.Median} ");
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            Console.WriteLine($"Sum:{detailed.Profile.Sum()}   ");
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            Console.WriteLine("-----------------------------------------------");
            for (int i = 0; i < detailed.Profile.Length && i < 1000; i++)
            {
                Console.WriteLine($"{detailed.Dates[i]:yyyy-MM-dd HH:mm:ss},{detailed.Profile[i]}");
            }
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            //Max:16143.1@2020-02-19   Min:4561.5@2020-02-19  Avg:9745.319398907103825136612022  Med:10201.1  Sum:3566786.9  


            detailed.Profile.Sum().Should().BeApproximately(3566786.9M, 0.1m);
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
        }


        [Fact]
        public async Task ValidateNem12SampleDetailed1Min()
        {
            var resource = "powerpal_data_0000f596 (2).csv";
            var mimeType = "text/csv";
            Console.SetOut(new RedirectOutput(Output));
            using Stream stream = File.OpenRead(Path.Combine("Resources", resource));
            // Use the stream here
            //test 

            var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
            Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");

            var fromDate = result.SitesDays.Select(x => x.Date).Min();
            var toDate = result.SitesDays.Select(x => x.Date).Max();
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            var detailed = MeterDataQuery.GetDetailedNetConsumption(fromDate, toDate, result.SitesDays);
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");

            detailed.Should().NotBeNull();
            Console.WriteLine($"Profile: Days:{detailed.Days} Range:{detailed.ActualDateRange} ");
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            Console.WriteLine($"Max:{detailed.MaxValue}");
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            Console.WriteLine($"@{detailed.MaxDateTime:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            //Console.WriteLine($"Min:{detailed.MinValue}@{detailed.MinDateTime:yyyy-MM-dd HH:mm:ss}  ");
            //Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            Console.WriteLine($"Avg:{detailed.AvgDay}  ");
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            Console.WriteLine($"Med:{detailed.Median} ");
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            Console.WriteLine($"Sum:{detailed.Profile.Sum()}   ");
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            Console.WriteLine("-----------------------------------------------");
            for (int i = 0; i < detailed.Profile.Length; i++)
            {
                Console.WriteLine($"{detailed.Dates[i]:yyyy-MMM-dd HH:mm:ss},{detailed.Profile[i]}");
            }
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            //Max:16143.1@2020-02-19   Min:4561.5@2020-02-19  Avg:9745.319398907103825136612022  Med:10201.1  Sum:3566786.9  


            detailed.Profile.Sum().Should().BeApproximately(1031.7571875000M, 0.1m);
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
        }



        [Fact]
        public async Task ValidateNem12SampleDetailed1MinlimitedRange()
        {
            var resource = "powerpal_data_0000f596 (2).csv";
            var mimeType = "text/csv";
            Console.SetOut(new RedirectOutput(Output));
            using Stream stream = File.OpenRead(Path.Combine("Resources", resource));
            // Use the stream here
            //test 

            var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
            Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");

            var fromDate = new DateTime(2022, 6, 22);
            var toDate = new DateTime(2022, 6, 27);
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            var detailed = MeterDataQuery.GetDetailedNetConsumption(fromDate, toDate, result.SitesDays);
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");

            detailed.Should().NotBeNull();
            Console.WriteLine($"Profile: Days:{detailed.Days} Range:{detailed.ActualDateRange} ");
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            Console.WriteLine($"Max:{detailed.MaxValue}");
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            Console.WriteLine($"@{detailed.MaxDateTime:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            //Console.WriteLine($"Min:{detailed.MinValue}@{detailed.MinDateTime:yyyy-MM-dd HH:mm:ss}  ");
            //Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            Console.WriteLine($"Avg:{detailed.AvgDay}  ");
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            Console.WriteLine($"Med:{detailed.Median} ");
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            Console.WriteLine($"Sum:{detailed.Profile.Sum()}   ");
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            Console.WriteLine("-----------------------------------------------");
            for (int i = 0; i < detailed.Profile.Length; i++)
            {
                Console.WriteLine($"{detailed.Dates[i]:yyyy-MMM-dd HH:mm:ss},{detailed.Profile[i]}");
            }
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            //Max:16143.1@2020-02-19   Min:4561.5@2020-02-19  Avg:9745.319398907103825136612022  Med:10201.1  Sum:3566786.9  


            //detailed.Profile.Sum().Should().BeApproximately(1031.7571875000M, 0.1m);
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(15)]
        [InlineData(30)]
        [InlineData(60)]
        [InlineData(120)]
        [InlineData(180)]
        [InlineData(240)]
        [InlineData(1440)]

        public async Task ValidateNem12SampleDetailed1MinlimitedRange2(int mins )
        {
            var resource = "powerpal_data_0000f596 (2).csv";
            var mimeType = "text/csv";
            Console.SetOut(new RedirectOutput(Output));
            using Stream stream = File.OpenRead(Path.Combine("Resources", resource));
            // Use the stream here
            //test 

            var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
            Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");

            var fromDate = new DateTime(2022, 6, 25);
            var toDate = new DateTime(2022, 6, 25);
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            var detailed = MeterDataQuery.GetDetailedNetConsumption(fromDate, toDate, result.SitesDays, new TimeInterval(TimeIntervalSize.Minute, mins));
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");

            detailed.Should().NotBeNull();
            Console.WriteLine($"Profile: Days:{detailed.Days} Range:{detailed.ActualDateRange} ");
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            Console.WriteLine($"Max:{detailed.MaxValue}");
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            Console.WriteLine($"@{detailed.MaxDateTime:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            //Console.WriteLine($"Min:{detailed.MinValue}@{detailed.MinDateTime:yyyy-MM-dd HH:mm:ss}  ");
            //Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            Console.WriteLine($"Avg:{detailed.AvgDay}  ");
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            Console.WriteLine($"Med:{detailed.Median} ");
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            Console.WriteLine($"Sum:{detailed.Profile.Sum()}   ");
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            Console.WriteLine("-----------------------------------------------");
            for (int i = 0; i < detailed.Profile.Length; i++)
            {
                Console.WriteLine($"{detailed.Dates[i]:yyyy-MMM-dd HH:mm:ss},{detailed.Profile[i]}");
            }
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
            //Max:16143.1@2020-02-19   Min:4561.5@2020-02-19  Avg:9745.319398907103825136612022  Med:10201.1  Sum:3566786.9  


            detailed.Profile.Sum().Should().BeApproximately(41.7809375000M, 0.1m);
            Console.WriteLine($"Elapsed:{timer.ElapsedMilliseconds}ms");
        }

        [Theory]
        [InlineData("4103354306_20160604_20170605_20170605000000_EnergyAustralia_DETAILED (1).csv", "text/csv", 14900.823)]
        [InlineData("61029293722_meter_usage_data_cipc1_05_2015.csv", "text/csv")]
        [InlineData("6407276797_20150803_20160517_20160518160200_UNITEDENERGY_DETAILED.csv", "text/csv", 1895.430)]
        [InlineData("64082253748_20220812_20240812_20240813120626_UNITEDENERGY_DETAILED.csv", "text/csv", 5929.804)]
        [InlineData("SampleNem", "" )]
        [InlineData("SampleNem12.csv", "text/csv" )]
        [InlineData("SampleNem12-2.csv", "text/csv" )]
        [InlineData("SampleNem12-3.csv", "text/csv" )]
        [InlineData("SampleNem12-4.CSV", "text/csv" )]
        [InlineData("MultiLineCsv1.csv", "text/csv", 106694.08)]
        [InlineData("SampleCsv.csv", "text/csv" )]
        [InlineData("SampleCsv3.csv", "text/csv" )]
        [InlineData("SampleCsv4.csv", "text/csv" )]
        [InlineData("SampleCsv5.csv", "text/csv" )]
        [InlineData("SampleCsv8.csv", "text/csv" )]
        [InlineData("SampleCsvFormat2.csv", "text/csv" )]
        [InlineData("SampleExcelByChanel.XLSX", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" )]
        [InlineData("SampleMultiLineCsv7.csv", "text/csv" )]
        [InlineData("SampleNem12Excel.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" )]
        [InlineData("SampleNem12Excel2.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" )]
        [InlineData("SampleNem12Excel3.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" )]
        [InlineData("SampleNem12Zip.zip", "application/x-zip-compressed" )]
        [InlineData("powerpal_data_0000f596 (1).csv", "text/csv" , 111.1950000000)]
        [InlineData("powerpal_data_0000f596 (2).csv", "text/csv", 1031.7571875000)]
        [InlineData("powerpal_data_0000f596 (3).csv", "text/csv" )]
        [InlineData("powerpal_data_0000f596 (4).csv", "text/csv" )]
        [InlineData("SampleCsvByChanel.csv", "text/csv" )]



        public async Task DailyProfileAllFiles(string resource, string mimeType, double? sum=null)
        {
            Console.SetOut(new RedirectOutput(Output));
            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            Console.WriteLine($"File:{resource}");
            Console.WriteLine("---------------------------------------------");

            using (Stream stream = File.OpenRead(Path.Combine("Resources", resource)))
            {
                // Use the stream here
                //test 
                var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
                bool actualResult = result.Success;
                Assert.True(actualResult);

                bool first = true; 
                foreach(var siteDays in result.SitesDays.GroupBy(x=>x.SiteCode))
                {
                    Console.WriteLine($"Site:{siteDays.Key}");
                    Console.WriteLine("---------------------------------------------");


                    DateTime fromDate = siteDays.Select(x => x.Date).Min();
                    DateTime toDate = siteDays.Select(x => x.Date).Max();
                    var daily = MeterDataQuery.GetDailyNetConsumption(fromDate, toDate, siteDays);

                    daily.Should().NotBeNull();
                    Console.WriteLine($"Profile: Days:{daily.Days} Range:{daily.ActualDateRange}  Max:{daily.MaxValue}@{daily.MaxDay:yyyy-MM-dd}     Avg:{daily.AvgDay}  Med:{daily.Median}  Sum:{daily.Profile.Sum()}   ");
                    Console.WriteLine("-----------------------------------------------");

                    if (first && sum.HasValue && sum.Value != 0 )
                    {
                        daily.Profile.Sum().Should().BeApproximately((decimal)sum.Value, 0.1m);
                    }



                    for (int i = 0; i < daily.Profile.Length; i++)
                    {
                        Console.WriteLine($"{daily.Dates[i]:yyyy-MMM-dd},{daily.Profile[i]}");
                    }






                }



            }
        }














    }

}
