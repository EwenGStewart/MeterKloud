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

    }

}
