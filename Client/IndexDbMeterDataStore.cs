using MeterDataLib;
using MeterDataLib.Storage;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Policy;
using System.Security.Principal;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Site = MeterDataLib.Site;

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
        
 

        public IndexDbMeterDataStore ( IndexedDbAccessor indexedDbAccessor , IMemoryCache memoryCache)
        {
            _indexedDbAccessor = indexedDbAccessor;
            _cache = memoryCache;
        }


        public async Task<Site?> GetSiteAsync(string siteId)
        {
            var sites = GetSiteListFromCache();
            if (sites != null)
            {
                var cachedSite = sites.Find(x => x.Id == siteId);
                if (cachedSite != null)
                {
                    SetLastSiteId(cachedSite.Id);
                    return cachedSite;
                }
            }
            
            var site = await _indexedDbAccessor.GetValueAsync<Site>(SITE_COLLECTION, siteId);
            if ( site != null )
            {
                site.LastAccessTimeUtc = DateTime.UtcNow;
                await _indexedDbAccessor.SetValueAsync(SITE_COLLECTION, site);

                sites ??= [];
                sites.Add(site);
                SetSiteListToCache(sites);
                SetLastSiteId(site.Id);
            }
            return site;
        }



        public async Task<Site?> GetSiteByCodeAsync(string siteCode)
        {

            siteCode = siteCode.Trim().ToUpperInvariant();
            var sites =  GetSiteListFromCache();
            if (sites != null)
            {
                var cachedSite = sites.Find(x => x.Code == siteCode);
                if (cachedSite != null)
                {
                    // set the last key 
                    SetLastSiteId(cachedSite.Id);
                    return cachedSite;
                }
            }
            var allSites = await GetSitesAsync();
            var site =  allSites.Where(x=>x.Code == siteCode).FirstOrDefault() ;
            if (site != null)
            {
                this.SetSiteListToCache(allSites);
                SetLastSiteId(site.Id);
            }
            return site;
        }

        public async Task<List<SiteDay>> GetSiteDays(string siteId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            fromDate ??= DateTime.Today.AddYears(-10);
            toDate ??= DateTime.Today.AddDays(10);
            string fromKey1 = GetSiteDayId(siteId, fromDate.Value);
            string toKey1 = GetSiteDayId(siteId, toDate.Value);
                
            var last = this.GetSiteDaysFromCache(siteId);
            if (last != null && last.siteId == siteId && last.fromdate <= fromDate && last.todate >= toDate && last.siteDays.Any())
            {
                return last.siteDays.Where( x=>x.Date >= fromDate && x.Date <= toDate ).ToList();
            }

            var result = await _indexedDbAccessor.GetRangeAsync<SiteDay>(SITEDAY_COLLECTION, fromKey1, toKey1);
            if ( result != null && result.Count != 0)
            {
                SetSiteDaysToCache(siteId, fromDate.Value, toDate.Value, result);
            }
            
            return result!;

        }


        public async Task<Site?> GetLastSiteAccessed()
        {
            var key = _cache.Get<string>(GetCacheKeyLast);
            if ( ! string.IsNullOrWhiteSpace(key))
            {
                var site = await this.GetSiteAsync(key);
                if (site != null)
                {
                    return site;
                }
            }
            var sites = await GetSitesAsync();
            if ( sites.Count > 0 )
            {
                var site = sites.OrderByDescending(x => x.LastAccessTimeUtc).First();
                SetLastSiteId(site.Id);
                return site;
            }
            return null;

        }







        public async Task<List<Site>> GetSitesAsync()
        {
            var sites =  GetSiteListFromCache();

            if (sites == null)
            {

                sites = await _indexedDbAccessor.GetAllValuesAsync<Site>(SITE_COLLECTION);
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} -> GetSitesAsync - from IndexDb {sites.Count}");
                sites ??= [];
                SetSiteListToCache(sites);
            }
            else
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} -> GetSitesAsync - from cache {sites.Count}");
             
            }
            return sites;
        }

        public async  Task<Site> PutSiteAsync(Site site)
        {
            site.LastAccessTimeUtc = DateTime.UtcNow;   
            await _indexedDbAccessor.SetValueAsync(SITE_COLLECTION, site);

            UpdateSiteInCache(site);
            SetLastSiteId ( site.Id);

            return site;
        }


        private void UpdateSiteInCache(Site site)
        {
            
            
            var sites = GetSiteListFromCache();
            if (sites != null)
            {

                var existing = sites.Find(x => x.Id == site.Id);
                if (existing != null)
                {
                    sites.Remove(existing);
                }
                sites.Add(site);
                Console.WriteLine($"Update {site.Id} in site cache list ");
                SetSiteListToCache(sites);
            }
        }


        public  async Task<SiteDay?> PutSiteDay(string siteId, SiteDay siteDay)
        {
             var key = GetSiteDayId(siteId, siteDay.Date);
             siteDay.Id = key;
             await _indexedDbAccessor.SetValueAsync(SITEDAY_COLLECTION, siteDay);
            ClearSiteDaysFromCache(siteId);
            return siteDay;
        }


        string GetSiteDayId(string siteId, DateTime day)
        {
            return $"{siteId}_{day:yyyy-MM-dd}";
        }




         

        const string GetCacheKeySites = "SITES";
        string GetCacheKeySiteDays(string id)  => $"SITEDAYS~~{id}";
        const string GetCacheKeyLast = "LASTSITE";


        private List<Site>? GetSiteListFromCache()
        {
            try
            {
                var result = _cache.Get<List<Site>>(GetCacheKeySites);
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} -> GetSiteListFromCache {result?.Count}");
                return result;
            }
            catch (Exception x)
            {

                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} -> GetSiteListFromCache - error");
                Console.WriteLine(x);
                return null; 
            }
        }
        

        private void SetSiteListToCache(List<Site> sites)
        {
            try
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} -> SetSiteListToCache {sites.Count}");
                _cache.Set(GetCacheKeySites, sites, new TimeSpan(1, 0, 0));
                
            }
            catch (Exception x)
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} -> SetSiteListToCache - error");
                Console.WriteLine(x);
            }
        }   

        private void SetLastSiteId(string siteId)
        {
            try
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} -> SetLastSiteId {siteId}");
                _cache.Set(GetCacheKeyLast, siteId);

            }
            catch (Exception x)
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} -> SetLastSiteId - error");
                Console.WriteLine(x);
            }
        }



        private CacheSiteDayLookUp? GetSiteDaysFromCache(string id)
        {
            try
            {
                var result = _cache.Get<CacheSiteDayLookUp>(GetCacheKeySiteDays(id));
                
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} -> GetSiteDaysFromCache {result?.siteId ?? "null"}");
                return result;
            }
            catch (Exception x)
            {

                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} -> GetSiteDaysFromCache - error");
                Console.WriteLine(x);
                return null;
            }
        }

         
        private void ClearSiteDaysFromCache(string id)
        {
            try
            {
     
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} -> ClearSiteDaysFromCache");
                _cache.Remove(GetCacheKeySiteDays(id));
      
            }
            catch (Exception x)
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} -> ClearSiteDaysFromCache - error");
                Console.WriteLine(x);
            }
        }


        private async void SetSiteDaysToCache(string siteId , DateTime fromDate, DateTime toDate, List<SiteDay> result )
        {
            try

            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} -> SetSiteDaysToCache {siteId}");
                var site = await GetSiteAsync(siteId);
                if ( site != null)
                {
                    site.LastAccessTimeUtc = DateTime.UtcNow;
                    await PutSiteAsync(site);
                }
                _cache.Set(GetCacheKeySiteDays(siteId), new CacheSiteDayLookUp(siteId, fromDate, toDate, result), new TimeSpan(1, 0, 0));
      
            }
            catch (Exception x)
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} -> SetSiteDaysToCache - error");
                Console.WriteLine(x);
            }
        }




        record CacheSiteDayLookUp (string siteId , DateTime fromdate , DateTime todate , List<SiteDay> siteDays); 
         



    }






}
