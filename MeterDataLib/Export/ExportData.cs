using MeterDataLib.Storage;
using System.IO.Compression;
using System.Text;

namespace MeterDataLib.Export
{
    public static class ExportData
    {
        public const int MaxDays = 365 * 10;
        private static readonly ChanelType[] StandardChannelTypes = [ChanelType.ActiveEnergyConsumption, ChanelType.ActiveEnergyGeneration, ChanelType.ReactiveEnergyConsumption, ChanelType.ReactiveEnergyGeneration];

        
        
        
        
        
        public static string ExportSiteDataToText(ExportOptions options )
        {
                var result = ExportSiteDataToTextAsync(options, null ).GetAwaiter().GetResult();
                return result;  

        }


        public static async Task<string> ExportSiteDataToTextAsync(ExportOptions options , CancellationToken? cancellationToken = null  )
        {
            
            
           
 
            var exportableDays = options.SiteDays.Count();
            if (exportableDays == 0)
            {
                throw new ArgumentException("No days has been requested to export. [Code:P1UJBB]");
            }
            if (exportableDays > MaxDays)
            {
                throw new ArgumentException("Too many days to export, [Code:PXCIXD]");
            }


            if (options.ChannelTypes.Count == 0)
            {
                if (options.ExportType == ExportFormat.ColumnarCSV || options.ExportType == ExportFormat.RowCSV)
                {
                    options.ChannelTypes = [.. options.SiteDays.SelectMany(x => x.Channels.Values).Select(x => x.ChannelType).Distinct()];
                }
                else
                {
                    options.ChannelTypes = [.. StandardChannelTypes];
                }
            }

            var writer = new StringBuilder();


            switch (options.ExportType)
            {
                case ExportFormat.NEM12:
                    await Nem12Exporter.ExportNem12(options, writer, cancellationToken);
                    break;
                case ExportFormat.QuadrantCSV:
                    await QuadrantExporter.ExportQuadrantCSV(options, writer, cancellationToken);
                    break;
                case ExportFormat.ColumnarCSV:
                    await ColumnExporter.ExportColumnCSV(options, writer, cancellationToken);
                    break;
                case ExportFormat.RowCSV:
                    await RowExporter.ExportRowCSV(options, writer, cancellationToken);
                    break;
                default: 
                    throw new NotImplementedException();
            }
            string result = writer.ToString();
            return result;
        }


        public static async Task<Stream> ExportMultiSitesToMultiFiles(ExportOptions options, CancellationToken? cancellationToken = null  )
        {

        
            // multiple sites - create a zip file

             
            int entryNumber = 0;
            var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (var site in options.SiteDays.GroupBy(x=>x.SiteCode))
                {
                     
                        string siteName = site.Key;
                        DateTime fromDate = site.Min(x => x.Date);
                        DateTime toDate = site.Max(x => x.Date);
                        
                        var siteOptions = options with { SiteDays = site };
                        cancellationToken?.ThrowIfCancellationRequested();
                        var siteOutput = await ExportSiteDataToTextAsync(siteOptions, cancellationToken);
                        if (string.IsNullOrWhiteSpace(siteOutput)) continue;
                        cancellationToken?.ThrowIfCancellationRequested();
                        var entry = archive.CreateEntry($"{siteName}_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}_{siteOptions.ExportType}.csv");
                        using var entryStream = entry.Open();
                        var bytes = Encoding.UTF8.GetBytes(siteOutput);
                        await entryStream.WriteAsync(bytes, cancellationToken ?? CancellationToken.None);
                        ++entryNumber;
                        await Task.Yield();

                }
                if(entryNumber == 0)
                {
                    throw new ArgumentException("No data to export [Code:HDW1QA]");
                }

            }
            zipStream.Seek(0, SeekOrigin.Begin);
            return zipStream;

        }









         





        internal static string Site(SiteDay siteDay, ExportOptions options)
        {
            if (options.IncludeSite)

                if (string.IsNullOrEmpty(siteDay.SiteCode))
                {
                    return "UNKNOWN";
                }
                else
                {
                    return siteDay.SiteCode.Trim().ToUpperInvariant();
                }
            else
            {
                return "ALL";
            }
        }

        internal static string Serial(ChannelDay channelDay, ExportOptions options)
        {
            if (options.IncludeMeter)

                if (string.IsNullOrEmpty(channelDay.MeterId))
                {
                    return "UNKNOWN";
                }
                else
                {
                    return channelDay.MeterId.Trim().ToUpperInvariant();
                }
            else
            {
                return "ALL";
            }
        }

        public static string ChannelNum(ChannelDay channelDay, ExportOptions options)
        {
            if (options.IncludeChannel)

                if (string.IsNullOrEmpty(channelDay.ChannelNumber))
                {
                    return "?";
                }
                else
                {
                    return channelDay.ChannelNumber.Trim().ToUpperInvariant();
                }
            else
            {
                return "*";
            }
        }

 


      

    }


}