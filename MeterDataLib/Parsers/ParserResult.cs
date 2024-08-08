using Microsoft.Extensions.Logging;

namespace MeterDataLib.Parsers
{
    public class ParserResult
    {
        
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







    }

}
