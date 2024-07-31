namespace MeterDataLib
{
    public class ChannelDay
    {

        public string Channel { get; set; } = string.Empty;
        public string ChannelNumber { get; set; } = string.Empty;

        public ChanelType ChannelType { get; set; }

        public bool IsNet { get; set; }
        public bool IsCheck { get; set; }
        public bool IsAvg { get; set; }
        public string MeterId { get; set; } = string.Empty;
        public string RegisterId     { get; set; } = string.Empty;

        public UnitOfMeasure UnitOfMeasure { get; set; }
        
        public string? OriginalUnitOfMeasureSymbol { get; set; }
        public UnitOfMeasure? OriginalUnitOfMeasure { get; set; }

        public int IntervalMinutes { get; set; }
        public int ExpectedReadings => IntervalMinutes > 0 ? 60 * 24 / IntervalMinutes : 0;
        public decimal Total { get; set; }
        public Quality OverallQuality { get; set; }
        public IndexRead? StartIndexRead { get; set; }
        public IndexRead? EndIndexRead { get;set; } 
        public decimal[] Readings { get; set; } = Array.Empty<decimal>();
        public Quality[]? ReadQualities { get; set; }
 
        public TimeOfUseClass[]? TimeOfUseClasses { get; set; }
        public DateTime TimeStampUtc { get; set; } = DateTime.UtcNow;
        public string SourceFile { get; set; } = string.Empty;
        public string SourceProviderCode { get; set; } = string.Empty;

        public EquipmentType EquipmentType { get; set; }
        public List<MeterDataInfo>  Metadata { get; set; } = new();
        public bool Ignore { get; set; }
        public bool Controlled { get; set; }
        public SeasonClass? Season { get; set; }
        
        public List<ChannelDay>? PreviousVersions { get; set; } 

    }


    public record MeterDataInfo(string MetaDataName, int FromIndex = 0, int ToIndex = 0, string Value = "")
    {
        public MeterDataInfo(string MetaDataName, string Value) : this(MetaDataName, 0, 0, Value)
        {
        }

    }

    public static class MetaDataName
    {
        public const string ReasonCode = "ReasonCode";
        public const string ReasonDescription = "ReasonDescription";
        public const string EstimationType = "EstimationType";
        public const string TransactionCode = "TransCode";
        public const string ServiceOrder = "ServiceOrder";

    }



}
