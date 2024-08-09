using Microsoft.Extensions.Logging;
using System.Xml.Serialization;

namespace MeterDataLib.Parsers
{
    public class ParserResult
    {
        public const string UNKNOWN = "Unknown";

        public bool Success => SitesDays.Any() && Errors == 0;
        public  List<SiteDay> SitesDays { get; set; } = new List<SiteDay>();

        public List<FileLogMessage> LogMessages { get; set; } = new List<FileLogMessage>();

        public int Errors => LogMessages.Count(m => m.LogLevel == LogLevel.Error || m.LogLevel == LogLevel.Critical);

        public int Warnings => LogMessages.Count(m => m.LogLevel == LogLevel.Warning );

        public string FileName { get; set; } = string.Empty;

 
        public string ParserName { get; set; } = string.Empty;

        public int Sites => SitesDays.Select (s => s.Site).Distinct().Count();

        public int TotalSiteDays => SitesDays.Count;
        public int TotalDataPoints => SitesDays.SelectMany(x=>x.Channels.Values).Sum(s => s.ExpectedReadings);


        public bool InProgress { get; set; } = false;
        
    

        public string Progress { get; set; } = string.Empty;



        public string SiteName( ) 
            {
                SetUnknownSites();
                
                if ( Sites == 1 )
                {
                    return SitesDays.First().Site;
                }
                else if ( Sites > 1)
                {
                    return "Multiple Sites";
                }
                else
                {
                    return "No Sites";
                }
            }

        public void SetUnknownSiteName(string siteName)
        {
            foreach (var siteDay in SitesDays)
            {
                if (string.IsNullOrWhiteSpace(siteDay.Site) || siteDay.Site.Trim().Equals(UNKNOWN, StringComparison.OrdinalIgnoreCase))
                {
                    siteDay.Site = siteName;
                }
            }
        }

        public bool UnknownSites => SitesDays.Any(s => s.Site == UNKNOWN);  


        void SetUnknownSites()
        {
            foreach (var sites in SitesDays)
            {
                if (string.IsNullOrWhiteSpace(sites.Site) || sites.Site.Trim().Equals(UNKNOWN, StringComparison.OrdinalIgnoreCase))
                {
                    sites.Site = UNKNOWN;
                }
            }
        }




    }

}
