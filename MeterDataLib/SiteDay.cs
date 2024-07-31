namespace MeterDataLib
{
    public class SiteDay
    {
        public String Site { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public Dictionary<string,ChannelDay> Channels { get; set; } = new Dictionary<string, ChannelDay>();
        
        public string TimeZoneName { get; set; } = string.Empty;
        public decimal? UCT_Offset { get; set; } = null;


    }


}
