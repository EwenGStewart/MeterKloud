using MeterDataLib;
using MeterDataLib.Storage;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Principal;

namespace MeterKloud
{

    /// <summary>
    ///  this is designed to run as a singleton in a WASM App 
    /// </summary>
    public class IndexDbMeterDataStore : IMeterDataStore
    {

        private const string SITE_COLLECTION = "sites";
        private const string SITEDAY_COLLECTION = "sitedays";

        private readonly IndexedDbAccessor _indexedDbAccessor;
        private readonly IMemoryCache _cache;
        private readonly List<Site> sites = new List<Site>();
        public IndexDbMeterDataStore ( IndexedDbAccessor indexedDbAccessor , IMemoryCache memoryCache)
        {
            _indexedDbAccessor = indexedDbAccessor;
            _cache = memoryCache;
        }

        public async Task<Site?> GetSiteAsync(string siteId)
        {
            return await _indexedDbAccessor.GetValueAsync<Site?>(SITE_COLLECTION, siteId);
        }

        public async Task<Site?> GetSiteByCodeAsync(string siteCode)
        {

            siteCode = siteCode.Trim().ToUpperInvariant();
            var site =  ( await GetSitesAsync()).Where(x=>x.Code == siteCode).FirstOrDefault() ;
            return site;
        }

        public async Task<List<SiteDay>> GetSiteDays(string siteId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            fromDate ??= DateTime.Today.AddYears(-10);
            toDate ??= DateTime.Today.AddDays(10);
            string fromKey1 = GetSiteDayId(siteId, fromDate.Value);
            string toKey1 = GetSiteDayId(siteId, toDate.Value);
            return await _indexedDbAccessor.GetRangeAsync<SiteDay>(SITEDAY_COLLECTION, fromKey1, toKey1);
         }



        public async Task<List<Site>> GetSitesAsync()
        {
            var sites = _cache.Get<List<Site>>(SITE_COLLECTION);
            if (sites == null)
            {
                sites = await _indexedDbAccessor.GetAllValuesAsync<Site>(SITE_COLLECTION);
                sites??= new List<Site>();
                _cache.Set(SITE_COLLECTION, sites , new  TimeSpan(1,0,0));
            }
            return sites;
        }

        public async  Task<Site> PutSiteAsync(Site site)
        {
             await _indexedDbAccessor.SetValueAsync(SITE_COLLECTION, site);
            _cache.Remove(SITE_COLLECTION);
            return site;
        }

        public  async Task<SiteDay?> PutSiteDay(string siteId, SiteDay siteDay)
        {
             var key = GetSiteDayId(siteId, siteDay.Date);
             siteDay.Id = key;
             await _indexedDbAccessor.SetValueAsync(SITEDAY_COLLECTION, siteDay);
             return siteDay;
        }


        string GetSiteDayId(string siteId, DateTime day)
        {
            return $"{siteId}_{day:yyyy-MM-dd}";
        }


    }



}
