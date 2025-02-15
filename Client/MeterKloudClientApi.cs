using MeterDataLib.Storage;
using MeterKloud.Pages;
using Microsoft.Extensions.Caching.Memory;
using System.IO;
using System.Threading;
using System;
using static System.Formats.Asn1.AsnWriter;
using MeterDataLib.Parsers;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Sockets;
using Site = MeterDataLib.Site;
using MeterDataLib;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.JSInterop;
using static MeterDataLib.Query.MeterDataQuery;
using System.Security.Policy;
using MeterDataLib.Export;




namespace MeterKloud
{
    public class MeterKloudClientApi
    {
        const long MAXALLOWEDSIZE = 1000 * 1000 * 1024;

        readonly IMemoryCache _memCache;
        bool _initialized = false;
        bool _inInit = false;
        private static readonly object LockingVar = new();
        private readonly IndexedDbAccessor _indexedDb ;
        private IndexDbMeterDataStore? indexDbMeterDataStore = null; 
        private MeterDataStorageManager? meterDataStorageManager = null;
        private List<MeterDataLib.Site> _sites = [];

        public MeterKloudClientApi( IMemoryCache memoryCache, IndexedDbAccessor indexedDbAccessor)
        {
            _memCache = memoryCache;
            _indexedDb = indexedDbAccessor;
            Console.WriteLine("MeterKloudClientApi Created");
            Console.WriteLine(DateTime.Now.ToString("hh:mm:ss.ffff"));

        }

        public async Task InitApi() 
        {
            // lock critical section - for init 
            if ( ! _inInit)
            {
                lock (LockingVar)
                {
                    if (_inInit) { return;}
                    _inInit = true;
                }
            }
            else   { return; }

            if (_initialized)
            {
                return;
            }
            Console.WriteLine("start InitApi");
            Console.WriteLine(DateTime.Now.ToString("hh:mm:ss.ffff"));

            // TOOD : lookup setting to determine what cache to use 
            // init the cache DB 
             await _indexedDb.InitializeAsync();
             indexDbMeterDataStore = new IndexDbMeterDataStore(_indexedDb, _memCache);
             meterDataStorageManager = new MeterDataStorageManager(indexDbMeterDataStore);


             // Perform No blocking startup tasks    
             // load previous sites ( this is non awaited task )
             _ =  Task.Run(async () => await LoadPreviousSites());

            Console.WriteLine("end  InitApi");
            Console.WriteLine(DateTime.Now.ToString("hh:mm:ss.ffff"));
        }


        public async Task<Site?>  GetLastSiteAccessed()
        {
            if (meterDataStorageManager == null)
            {
                return null;
            }
            var site = await meterDataStorageManager.GetLastSiteAccessed();
            return site;
        }


        public async Task<ParserResult> UploadStream(IBrowserFile browserFile, Func<ParserResult, Task>? CallBack, CancellationToken? cancellationToken , ParserResult result  ) 
        {
     
            try
            {
                string fileName = browserFile.Name;
                string fileType = browserFile.ContentType;
                result.FileName = fileName;

                result.LogMessages.Add(new FileLogMessage($"Load file {fileName} with content type {fileType} ", LogLevel.Information, fileName, 0, 0));
                result = await  MeterDataLib.Parsers.ParserFactory.ParseAsync(browserFile.OpenReadStream(MAXALLOWEDSIZE), fileName, fileType, CallBack, cancellationToken, result);
                if ( result.TotalSiteDays > 0  && meterDataStorageManager!=null )
                {
                    await meterDataStorageManager.SaveParserResult(result, _sites);
                }
                

            }
            catch (OperationCanceledException )
            {
                result.AddException("Operation Cancelled");

            }
            catch (Exception ex)
            {
                result.AddException(ex);
            }
            finally
            {

            }

            return result;

        }



        public async Task<List<Site>> GetSites()
        {
            if (meterDataStorageManager == null)
            {
                return [];
            }
            return await meterDataStorageManager.GetSites();
        }



        public async Task<Site?> GetSite(string id)
        {
            if (meterDataStorageManager == null)
            {
                return null;
            }
            var site = await  meterDataStorageManager.GetSite(id);
            return site;
        }


        public async Task<Site> PutSite(Site site)
        {
            if (meterDataStorageManager == null)
            {
                throw new Exception("Storage Manager not initialized");
            }
            await meterDataStorageManager.PutSite(site);
            return site;
        }




        public async Task<Site?> GetSiteByCode(string id)
        {
            if (meterDataStorageManager == null)
            {
                return null;
            }
            var site = await meterDataStorageManager.GetSiteByCode(id);
            if (site != null)
            {
                _sites.Add(site);
            }
            return site;
        }



        public async Task<List<SiteDay>> GetSiteDays(string id, DateTime fromDate , DateTime toDate )
        {
            if (meterDataStorageManager == null)
            {
                return [];
            }
            var siteDays  = await meterDataStorageManager.GetSiteDays(id, fromDate , toDate );
           
            return siteDays;
        }

        public async Task<List<SiteDay>> GetSiteDays(string id)
        {
            if (meterDataStorageManager == null)
            {
                return [];
            }
            var site = await meterDataStorageManager.GetSite(id);
            if (site == null)
            {
                return [];
            }
            var fromDate = site.FirstDay.Date;
            var toDate = site.LastDay.Date;
            var minFromDate = toDate.Year >= 2000 ? new DateTime(toDate.Year - 2, toDate.Month, 1) : new DateTime(2000, 1, 1);

            if ( fromDate < minFromDate)
            {
                fromDate = minFromDate;
            }
            var siteDays = await meterDataStorageManager.GetSiteDays(id, fromDate, toDate);
            return siteDays;
        }


        public async   Task<MeterDataLib.Query.MeterDataQuery.DailyConsumptionProfileResult>    GetDailyConsumption ( string siteId )
        {
            if (meterDataStorageManager == null)
            {
                return new MeterDataLib.Query.MeterDataQuery.DailyConsumptionProfileResult(new MeterDataLib.Query.QueryDateRange(DateTime.Today, DateTime.Today));
            }
            
            var days = await GetSiteDays(siteId);
            if ( days.Count == 0)
            {
                return new MeterDataLib.Query.MeterDataQuery.DailyConsumptionProfileResult(new MeterDataLib.Query.QueryDateRange(DateTime.Today, DateTime.Today));
            }
            var result = MeterDataLib.Query.MeterDataQuery.GetDailyNetConsumption(days.OrderBy(x=>x.Date).First().Date, days.OrderBy(x=>x.Date).Last().Date, days);
            return result;

        }


        public async Task<MeterDataLib.Query.MeterDataQuery.DetailedConsumptionProfileResult> GetDetailedConsumption(string siteId)
        {
            if (meterDataStorageManager == null)
            {
                return new MeterDataLib.Query.MeterDataQuery.DetailedConsumptionProfileResult(new MeterDataLib.Query.QueryDateRange(DateTime.Today, DateTime.Today));
            }

            var days = await GetSiteDays(siteId);
            if (days.Count == 0)
            {
                return new MeterDataLib.Query.MeterDataQuery.DetailedConsumptionProfileResult(new MeterDataLib.Query.QueryDateRange(DateTime.Today, DateTime.Today));
            }
            var result = MeterDataLib.Query.MeterDataQuery.GetDetailedNetConsumption(days.OrderBy(x => x.Date).First().Date, days.OrderBy(x => x.Date).Last().Date, days);
            return result;

        }


        public async Task<MeterDataLib.Query.MeterDataQuery.HeatMapResult> GetHeatAsync(string siteId)
        {
            var days = await GetSiteDays(siteId);
            var result = MeterDataLib.Query.MeterDataQuery.GetHeatMapConsumption(days.OrderBy(x => x.Date).First().Date, days.OrderBy(x => x.Date).Last().Date, days);
            return result;
        }


        public async Task<MeterDataLib.Query.MeterDataQuery.DailyDemandResult> GetDailyDemand(string siteId, DateTime? fromDate= null , DateTime? toDate=null)
        {
            if (meterDataStorageManager == null)
            {
                return new MeterDataLib.Query.MeterDataQuery.DailyDemandResult(new MeterDataLib.Query.QueryDateRange(DateTime.Today, DateTime.Today));
            }


            var days = fromDate.HasValue && toDate.HasValue ?  await GetSiteDays(siteId, fromDate.Value, toDate.Value) : await GetSiteDays(siteId);
            if (days.Count == 0)
            {
                return new MeterDataLib.Query.MeterDataQuery.DailyDemandResult(new MeterDataLib.Query.QueryDateRange(DateTime.Today, DateTime.Today));
            }
            fromDate ??= days.OrderBy(x => x.Date).First().Date;
            toDate ??= days.OrderBy(x => x.Date).Last().Date;
            var result = MeterDataLib.Query.MeterDataQuery.GetDailyDemand(fromDate.Value, toDate.Value, days,30);
            return result;


        }



        public async Task<QuadrantResult> GetQuadrants(string siteId, DateTime? fromDate, DateTime? toDate  )
        {
            if (meterDataStorageManager == null)
            {
                return new MeterDataLib.Query.MeterDataQuery.QuadrantResult(new MeterDataLib.Query.QueryDateRange(DateTime.Today, DateTime.Today));
            }


            var days = fromDate.HasValue && toDate.HasValue ? await GetSiteDays(siteId, fromDate.Value, toDate.Value) : await GetSiteDays(siteId);
            if (days.Count == 0)
            {
                return new MeterDataLib.Query.MeterDataQuery.QuadrantResult(new MeterDataLib.Query.QueryDateRange(DateTime.Today, DateTime.Today));
            }
            fromDate ??= days.OrderBy(x => x.Date).First().Date;
            toDate ??= days.OrderBy(x => x.Date).Last().Date;
            var result = MeterDataLib.Query.MeterDataQuery.GetQuadrants(fromDate.Value, toDate.Value, days, 30);
            return result;
        }


        async Task LoadPreviousSites()
        {
            if (meterDataStorageManager == null)
            {
                return;
            }
            _sites = await meterDataStorageManager.GetSites();
            
 
            _initialized = true; 

        }


        public async Task<string> Export(ExportOptions options, CancellationToken? cancellationToken)
        {
            var result = await MeterDataLib.Export.ExportData.ExportSiteDataToTextAsync(options, cancellationToken);
            return result;
        }

        public async Task<Stream> ExportMultiFile(ExportOptions options, CancellationToken? cancellationToken)
        {
            var result = await MeterDataLib.Export.ExportData.ExportMultiSitesToMultiFiles(options, cancellationToken);
            return result;
        }



    }
}
