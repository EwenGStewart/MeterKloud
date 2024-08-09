using MeterDataLib.Parsers;

namespace MeterDataLib
{
    public class SiteDay
    {

        public string? Id { get; set; }



        private string siteCode = ParserResult.UNKNOWN;
        public string SiteCode
        {
            get => siteCode;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    siteCode = ParserResult.UNKNOWN;
                }
                else
                {
                    siteCode = value.Trim().ToUpperInvariant();
                }
            }
        }

        

        public DateTime Date { get; set; }
        public Dictionary<string,ChannelDay> Channels { get; set; } = new Dictionary<string, ChannelDay>();
        
        public string TimeZoneName { get; set; } = string.Empty;
        public decimal? UCT_Offset { get; set; } = null;


    }


}
