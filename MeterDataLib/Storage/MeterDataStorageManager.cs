using MeterDataLib.Parsers;

namespace MeterDataLib.Storage
{
    public class MeterDataStorageManager
    {
        readonly IMeterDataStore _storage;
        public MeterDataStorageManager(IMeterDataStore storage)
        {
            _storage = storage;
        }

        public async Task SaveParserResult(ParserResult parserResult)
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

                if (site.LastUpdatedTimeStampUtc > site.DbTimeStampUtc)
                {
                    site = await _storage.PutSiteAsync(site);
                }

                await _storage.PutSiteAsync(site);
                foreach (var siteDay in siteGroup)
                {
                    await _storage.PutSiteDay(site.Id, siteDay);
                }

            }
        }

    }
}
