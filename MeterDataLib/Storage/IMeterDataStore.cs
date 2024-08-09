using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeterDataLib.Storage
{
    public interface IMeterDataStore
    {
        public Task<List<Site>> GetSitesAsync();

        public Task<Site?> GetSiteAsync(string siteId);

        public Task<Site?> GetSiteByCodeAsync(string siteCode);
        public Task<Site> PutSiteAsync(Site site); 



        public Task<List<SiteDay>> GetSiteDays(string siteId , DateTime? fromDate = null , DateTime? toDate = null );


        public Task<SiteDay?> PutSiteDay(string siteId, SiteDay siteDay);




    }
}
