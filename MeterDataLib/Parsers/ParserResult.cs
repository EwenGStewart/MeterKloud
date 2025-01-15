using Microsoft.Extensions.Logging;
using System.Xml.Serialization;

namespace MeterDataLib.Parsers
{
    public class ParserResult
    {
        public const string UNKNOWN = "UNKNOWN";

        public bool Success => SitesDays.Any() && Errors == 0;
        public  List<SiteDay> SitesDays { get; set; } = [];

        public List<FileLogMessage> LogMessages { get; set; } = [];


        public void AddException( Exception exception)
        {
            LogMessages.Add(new FileLogMessage(exception.Message, LogLevel.Error , FileName, 0, 0));
        }

        public void AddException(string exception)
        {
            LogMessages.Add(new FileLogMessage(exception, LogLevel.Error, FileName, 0, 0));
        }



        public int Errors => LogMessages.Count(m => m.LogLevel == LogLevel.Error || m.LogLevel == LogLevel.Critical);

        public int Warnings => LogMessages.Count(m => m.LogLevel == LogLevel.Warning );

        public string FileName { get; set; } = string.Empty;

 
        public string ParserName { get; set; } = string.Empty;

        public int Sites => SitesDays.Select (s => s.SiteCode).Distinct().Count();

        public int TotalSiteDays => SitesDays.Count;
        public int TotalDataPoints => SitesDays.SelectMany(x=>x.Channels.Values).Sum(s => s.ExpectedReadings);


        public bool InProgress { get; set; } = false;
        
    

        public string Progress { get; set; } = string.Empty;



        public string SiteName( ) 
            {
                FixSiteNames();
                
                if ( Sites == 1 )
                {
                    return SitesDays.First().SiteCode;
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
                if (string.IsNullOrWhiteSpace(siteDay.SiteCode) || siteDay.SiteCode.Trim().Equals(UNKNOWN, StringComparison.OrdinalIgnoreCase))
                {
                    siteDay.SiteCode = siteName;
                }
            }
        }

        public bool UnknownSites => SitesDays.Any(s => s.SiteCode == UNKNOWN);  


        internal void FixSiteNames()
        {
            foreach (var siteDay in SitesDays)
            {
                if (string.IsNullOrWhiteSpace(siteDay.SiteCode) || siteDay.SiteCode.Trim().Equals(UNKNOWN, StringComparison.OrdinalIgnoreCase))
                {
                    siteDay.SiteCode = UNKNOWN;
                }
                if (!siteDay.SiteCode.Trim().Equals(siteDay.SiteCode, StringComparison.InvariantCultureIgnoreCase)) 
                {
                    siteDay.SiteCode = siteDay.SiteCode.Trim().ToUpperInvariant();
                }
            }
        }




    }

}
