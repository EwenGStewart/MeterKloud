using MeterDataLib.Storage;
using System.IO.Compression;
using System.Text;

namespace MeterDataLib.Export
{
    public static class ExportData
    {
        public const int MaxDays = 366 * 4;
        private static readonly ChanelType[] StandardChannelTypes = [ChanelType.ActiveEnergyConsumption, ChanelType.ActiveEnergyGeneration, ChanelType.ReactiveEnergyConsumption, ChanelType.ReactiveEnergyGeneration];

        
        
        
        
        
        public static string ExportSiteDataToText(ExportOptions options, MeterDataStorageManager? meterDataStore = null)
        {
                var result = ExportSiteDataToTextAsync(options, null, meterDataStore).GetAwaiter().GetResult();
                return result;  

        }


        public static async Task<string> ExportSiteDataToTextAsync(ExportOptions options , CancellationToken? cancellationToken = null, MeterDataStorageManager? meterDataStore = null )
        {
            
            
            options = CreateInternalSiteList(options, cancellationToken);
            options = DefaultInitialDateRange(options, cancellationToken);
            // Fix the interval minutes
            if ( options.IntervalInMinutes <= 0 )
            {
                options.IntervalInMinutes = null;
            }


            foreach (var site in options.Sites)
            {
                // have site days been provided 
                if ( options.SiteDays.Any( x=>x.SiteCode.Equals( site.Code, StringComparison.OrdinalIgnoreCase )) )
                {
                    // ok - the site days have been explicitly provided - now check if we need to remove any days that are not in the date range
                    var excludedDayList = options.SiteDays.Where(x => x.SiteCode.Equals(site.Code, StringComparison.OrdinalIgnoreCase) && ((x.Date < options.FromDate!.Value.Date) || (x.Date > options.ToDate!.Value.Date))).ToList();
                    if (excludedDayList.Count > 0)
                    {
                        // remove them 
                        options.SiteDays = options.SiteDays.Except(excludedDayList).ToList();
                    }
                }
                else   // no site days have been provide - we will request them 
                {
                    var newDays = meterDataStore==null ? [] :  await meterDataStore.GetSiteDays(site.Id, options.FromDate!.Value, options.ToDate!.Value);
                    cancellationToken?.ThrowIfCancellationRequested();
                    if ( newDays.Count > 0)
                    {
                        // combine options.sitedays with newDays
                        options.SiteDays = options.SiteDays.Concat(newDays).ToList();
                    }
                }
            }
            var exportableDays = options.SiteDays.Count();
            if (exportableDays == 0)
            {
                throw new ArgumentException("No data in date range for specified sites can be exported. [Code:P1UJBB]");
            }
            if (exportableDays > MaxDays)
            {
                throw new ArgumentException("Too many days to export, [Code:PXCIXD]");
            }


            if (options.ChannelTypes.Count == 0)
            {
                if (options.ExportType == ExportFormat.ColumnarCSV || options.ExportType == ExportFormat.RowCSV)
                {
                    options.ChannelTypes = options.SiteDays.SelectMany(x => x.Channels.Values).Select(x => x.ChannelType).Distinct().ToList();
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


        public static async Task<(Stream file, bool isZip, string preview)> ExportMultiSitesToMultiFiles(ExportOptions options, CancellationToken? cancellationToken = null , MeterDataStorageManager? meterDataStore = null)
        {

            options = CreateInternalSiteList(options, cancellationToken);
            options = DefaultInitialDateRange(options, cancellationToken);
            if ( options.Sites.Count  <= 1 )
            {
                // do as a single site 
                var fileOutput  = await ExportSiteDataToTextAsync(options, cancellationToken, meterDataStore);
                if ( string.IsNullOrWhiteSpace(fileOutput) ) throw new ArgumentException("No data to export. [Code:ARR5FI]");
                // convert the text to a stream 
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileOutput));
                return (stream, false, fileOutput[0..5000]);
            }
            // multiple sites - create a zip file

            string preview = string.Empty;
            int entryNumber = 0;
            var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (var site in options.Sites)
                {
                    try
                    {
                        var siteOptions = options with { Site = site };
                        siteOptions.Sites = [];
                        if ( siteOptions.SiteDays.Any())
                        {
                            siteOptions.SiteDays = siteOptions.SiteDays.Where(x => x.SiteCode.Equals(site.Code, StringComparison.OrdinalIgnoreCase)).ToList();
                        }
                        cancellationToken?.ThrowIfCancellationRequested();
                        var siteOutput = await ExportSiteDataToTextAsync(siteOptions, cancellationToken, meterDataStore);
                        if (string.IsNullOrWhiteSpace(siteOutput)) continue;

                        cancellationToken?.ThrowIfCancellationRequested();
                        var entry = archive.CreateEntry($"{site.Code}.csv");
                        using var entryStream = entry.Open();
                        var bytes = Encoding.UTF8.GetBytes(siteOutput);
                        await entryStream.WriteAsync(bytes, cancellationToken ?? CancellationToken.None);
                        ++entryNumber;
                        if (entryNumber == 1)
                        {
                            preview = siteOutput[0..5000];
                        }
                    }
                    catch (Exception ex)
                    {
                        // log the error 
                        
                    }
                }
                if(entryNumber == 0)
                {
                    throw new ArgumentException("No data to export [Code:HDW1QA]");
                }

            }
            zipStream.Seek(0, SeekOrigin.Begin);
            return (zipStream, true, preview);

        }




        private static ExportOptions DefaultInitialDateRange(ExportOptions options, CancellationToken? cancellationToken)
        {
        
            if (options.FromDate == null)
            {
                options.FromDate = options.SiteDays.Any() ? options.SiteDays.Min(x => x.Date) : DateTime.Today.AddYears(-1);
            }
            if (options.ToDate == null)
            {
                options.ToDate = options.SiteDays.Any() ? options.SiteDays.Max(x => x.Date) : DateTime.Today;
            }
            if (options.FromDate > options.ToDate)
            {
                throw new ArgumentException("Invalid date range. [Code:PU1CCH]");
            }

            return options;
        }

        private static ExportOptions CreateInternalSiteList(ExportOptions options, CancellationToken? cancellationToken)
        {
            cancellationToken?.ThrowIfCancellationRequested();

            if (options.Site != null)
            {
                if (!options.Sites.Any(x => x.Code.Equals(options.Site.Code, StringComparison.OrdinalIgnoreCase)))
                {
                    options.Sites.Add(options.Site);
                }
                options.Site = null;
            }
            if (options.Sites.Count == 0)
            {
                var codes = options.SiteDays.Where(x => !string.IsNullOrWhiteSpace(x.SiteCode)).Select(x => x.SiteCode.ToUpperInvariant().Trim()).Distinct().ToList();
                if (codes.Count > 0)
                {
                    foreach (var code in codes)
                    {

                        var site = new Site() { Code = code };
                        if (site != null)
                        {
                            options.Sites.Add(site);
                        }
                    }
                }
            }


            if (options.Sites.Count == 0)
            {
                throw new ArgumentException("No sites specified or found [Code:BHN43C]");
            }
            return options;
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