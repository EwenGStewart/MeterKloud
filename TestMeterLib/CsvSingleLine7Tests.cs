﻿using MeterDataLib.Parsers;
using Xunit.Abstractions;

namespace TestMeterLib
{
    public class CsvSingleLine7Tests(ITestOutputHelper Output)
    {
        [Theory]
        [InlineData("SampleMultiLineCsv7.csv", "text/csv")]

        public async Task Parse1(string resource, string mimeType)
        {
            Console.SetOut(new RedirectOutput(Output));
            using Stream stream = File.OpenRead(Path.Combine("Resources", resource));
            var result = await ParserFactory.ParseAsync(stream, resource, mimeType);
            Console.WriteLine($"Parsed {result.ParserName} Errors:{result.Errors} Days:{result.TotalSiteDays} Sites:{result.Sites}");
            Assert.True(result.Success);
            Assert.True(result.ParserName == "SingleLineFromLine7");
        }
    }

}