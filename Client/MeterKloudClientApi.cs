using MeterDataLib.Storage;
using MeterKloud.Pages;
using Microsoft.Extensions.Caching.Memory;
using System.IO;
using System.Threading;
using System;
using static System.Formats.Asn1.AsnWriter;
using MeterDataLib.Parsers;
using Microsoft.AspNetCore.Components.Forms;

namespace MeterKloud
{
    public class MeterKloudClientApi
    {
        readonly IMemoryCache _memCache;
        bool _initialized = false;
        bool _inInit = false;
        private static object LockingVar = new object();
        private readonly IndexedDbAccessor _indexedDb ;
        private IndexDbMeterDataStore? indexDbMeterDataStore = null; 
        private MeterDataStorageManager? meterDataStorageManager = null;
        private List<MeterDataLib.Site> _sites = new();

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


        public MeterDataLib.Site? GetLastSite => _sites.Any() ? _sites.OrderByDescending(xx => xx.LastAccessTimeUtc).ThenBy(xx => xx.Code).FirstOrDefault() : null;


        public async Task<ParserResult> UploadStream(IBrowserFile browserFile, Func<ParserResult, Task>? CallBack, CancellationToken? cancellationToken , ParserResult result  ) 
        {
     
            try
            {
                string fileName = browserFile.Name;
                string fileType = browserFile.ContentType;
                result.FileName = fileName;
                result.LogMessages.Add(new FileLogMessage($"Load file {fileName} with content type {fileType} ", LogLevel.Information, fileName, 0, 0));
                result = await  MeterDataLib.Parsers.ParserFactory.ParseAsync(browserFile.OpenReadStream(), fileName, fileType, CallBack, cancellationToken, result);
                if ( result.TotalSiteDays > 0  && meterDataStorageManager!=null )
                {
                    await meterDataStorageManager.SaveParserResult(result, _sites);
                }
                

            }
            catch (OperationCanceledException ex)
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



        async Task LoadPreviousSites()
        {
            if (meterDataStorageManager == null)
            {
                return;
            }
            _sites = await meterDataStorageManager.GetSites();
            
 
            _initialized = true; 

        }





    }
}
