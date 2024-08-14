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
            var sites = _cache.Get<List<Site>>(GetCacheKeySites());
            if (sites != null)
            {
                var cachedSite = sites.Find(x => x.Id == siteId);
                if (cachedSite != null)
                {
                    return cachedSite;
                }
            }
            
            var site = await _indexedDbAccessor.GetValueAsync<Site>(SITE_COLLECTION, siteId);
            return site;
        }






        public async Task<Site?> GetSiteByCodeAsync(string siteCode)
        {

            siteCode = siteCode.Trim().ToUpperInvariant();
            var sites = _cache.Get<List<Site>>(GetCacheKeySites());
            if (sites != null)
            {
                var cachedSite = sites.Find(x => x.Code == siteCode);
                if (cachedSite != null)
                {
                    return cachedSite;
                }
            }



            var site =  ( await GetSitesAsync()).Where(x=>x.Code == siteCode).FirstOrDefault() ;
            return site;
        }

        public async Task<List<SiteDay>> GetSiteDays(string siteId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            fromDate ??= DateTime.Today.AddYears(-10);
            toDate ??= DateTime.Today.AddDays(10);
            string fromKey1 = GetSiteDayId(siteId, fromDate.Value);
            string toKey1 = GetSiteDayId(siteId, toDate.Value);
                
            var last = _cache.Get<CacheSiteDayLookUp>(GetCacheKeySiteDays());
            if (last != null && last.siteId == siteId && last.fromdate <= fromDate && last.todate >= toDate && last.siteDays.Any())
            {
                return last.siteDays.Where( x=>x.Date >= fromDate && x.Date <= toDate ).ToList();
            }

            var result = await _indexedDbAccessor.GetRangeAsync<SiteDay>(SITEDAY_COLLECTION, fromKey1, toKey1);
            if ( result != null && result.Count != 0)
            {
                _cache.Set(GetCacheKeySiteDays(), new CacheSiteDayLookUp(siteId, fromDate.Value, toDate.Value, result), new TimeSpan(1, 0, 0));
            }
            return result!;

        }



        public async Task<List<Site>> GetSitesAsync()
        {
            var sites = _cache.Get<List<Site>>(GetCacheKeySites());
            if (sites == null)
            {
                sites = await _indexedDbAccessor.GetAllValuesAsync<Site>(SITE_COLLECTION);
                sites??= new List<Site>();
                _cache.Set(GetCacheKeySites(), sites , new  TimeSpan(1,0,0));
            }
            return sites;
        }

        public async  Task<Site> PutSiteAsync(Site site)
        {
             await _indexedDbAccessor.SetValueAsync(SITE_COLLECTION, site);
            _cache.Remove(GetCacheKeySites());
            return site;
        }

        public  async Task<SiteDay?> PutSiteDay(string siteId, SiteDay siteDay)
        {
             var key = GetSiteDayId(siteId, siteDay.Date);
             siteDay.Id = key;
             await _indexedDbAccessor.SetValueAsync(SITEDAY_COLLECTION, siteDay);
            _cache.Remove(GetCacheKeySiteDays());
            return siteDay;
        }


        string GetSiteDayId(string siteId, DateTime day)
        {
            return $"{siteId}_{day:yyyy-MM-dd}";
        }

         

        string GetCacheKeySites()
        {
            return $"{SITE_COLLECTION}##ALL";
        }

        string GetCacheKeySiteDays( )
        {
            return $"{SITEDAY_COLLECTION}##Last";
        }


        record CacheSiteDayLookUp (string siteId , DateTime fromdate , DateTime todate , List<SiteDay> siteDays); 
         



    }






}
