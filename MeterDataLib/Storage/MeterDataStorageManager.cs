using MeterDataLib.Parsers;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MeterDataLib.Storage
{
    public class MeterDataStorageManager
    {
        readonly IMeterDataStore _storage;
        public MeterDataStorageManager(IMeterDataStore storage)
        {
            _storage = storage;
        }

        public Site? LastSIte { get; private set; }


        public async Task SaveParserResult(ParserResult parserResult , List<Site>? _siteListCache  )
        {

            parserResult.FixSiteNames();
            parserResult.Progress = "Saving Sites";
            foreach (var siteGroup in parserResult.SitesDays.GroupBy(x => x.SiteCode))
            {
                parserResult.Progress = $"Saving Site {siteGroup.Key} ";
                var site = await _storage.GetSiteByCodeAsync(siteGroup.Key);
                if (site == null)
                {
                    site = new Site() { Code = siteGroup.Key, Name = siteGroup.Key };
                    site = await _storage.PutSiteAsync(site);
                }
                var first = siteGroup.Min(x => x.Date);
                var last = siteGroup.Max(x => x.Date);
                if (site.FirstDay == DateTime.MinValue || first < site.FirstDay)
                {
                    site.FirstDay = first;
                    site.LastUpdatedTimeStampUtc = DateTime.UtcNow;
                }
                if (site.LastDay == DateTime.MinValue || last > site.LastDay)
                {
                    site.LastDay = last;
                    site.LastUpdatedTimeStampUtc = DateTime.UtcNow;

                }

                site.LastAccessTimeUtc   = DateTime.UtcNow;
                site = await _storage.PutSiteAsync(site);
                await _storage.PutSiteAsync(site);
                LastSIte = site;
                if (_siteListCache != null)
                {
                    var existing = _siteListCache.Where(x => x.Id == site.Id).FirstOrDefault();
                    if (existing != null)
                    {
                        _siteListCache.Remove(existing);
                    }
                    _siteListCache.Add(site);
                }
                foreach (var siteDay in siteGroup)
                {
                    await _storage.PutSiteDay(site.Id, siteDay);
                }

            }
        }


        public async Task<List<Site>> GetSites()
        {
            return await _storage.GetSitesAsync();
        }


        public async Task<Site?> GetSite(string id)
        {
            return await _storage.GetSiteAsync(id);
        }

        public async Task<Site> PutSite(Site site)
        {
            return await _storage.PutSiteAsync(site);
        }


        public async Task<Site?> GetSiteByCode(string code)
        {
            return await _storage.GetSiteByCodeAsync(code);
        }

        public async Task<Site?> GetLastSiteAccessed()
        {
            return await _storage.GetLastSiteAccessed();
        }

        public async Task<List<SiteDay>> GetSiteDays(string siteId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _storage.GetSiteDays(siteId, fromDate, toDate);
        }





    }
}
