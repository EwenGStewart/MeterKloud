using MeterDataLib.Parsers;

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

        public string ChannelNumberOrMeterName => string.IsNullOrWhiteSpace(ChannelNumber) ? (string.IsNullOrWhiteSpace(MeterId) ? ParserResult.UNKNOWN : MeterId.Trim().ToUpperInvariant()) : ChannelNumber.Trim().ToUpperInvariant();


        public string RegisterId { get; set; } = string.Empty;

        public UnitOfMeasure UnitOfMeasure { get; set; }

        public string? OriginalUnitOfMeasureSymbol { get; set; }
        public UnitOfMeasure? OriginalUnitOfMeasure { get; set; }

        public int IntervalMinutes { get; set; }
        public int ExpectedReadings => IntervalMinutes > 0 ? 60 * 24 / IntervalMinutes : 0;
        public decimal Total { get; set; }
        public Quality OverallQuality { get; set; }
        public IndexRead? StartIndexRead { get; set; }
        public IndexRead? EndIndexRead { get; set; }
        public decimal[] Readings { get; set; } = [];
        public Quality[]? ReadQualities { get; set; }

        public Quality[] QualityArray()
        {
            if (Readings.Length == 0)
            {
                return [];
            }

            if (OverallQuality == Quality.Variable && ReadQualities != null)
            {

                if (ReadQualities.Length == 0)
                {
                    var result = new Quality[Readings.Length];
                    Array.Fill(result, Quality.Unknown);
                    return result;
                }
                if (ReadQualities.Length == Readings.Length)
                {
                    return ReadQualities;
                }
                else
                {
                    return ProfileHelpers.UnifyQuality(ReadQualities, Readings.Length);
                }
            }
            else
            {
                var result = new Quality[Readings.Length];
                Array.Fill(result, OverallQuality);
                return result;
            }
        }


        public string[] MetadataArray(string metaDataName)
        {
            if (Readings.Length == 0)
            {
                return [];
            }
            var result = new string[Readings.Length];
            foreach (var item in Metadata.Where(x => x.MeterDataName.Equals(metaDataName, StringComparison.OrdinalIgnoreCase)).OrderBy(x => x.FromIndex))
            {

                int a;
                int b;
                if (item.FromIndex == 0 && item.ToIndex == 0)
                {
                    a = 0;
                    b = Readings.Length - 1;
                }
                else
                {
                    a = item.FromIndex-1;
                    b = item.ToIndex-1;

                }
                for (int i = a; i <= b && i<=result.Length; i++)
                {
                        result[i] = item.Value;
                }
            }
            return result;

        }




        public TimeOfUseClass[]? TimeOfUseClasses { get; set; }
        public DateTime TimeStampUtc { get; set; } = DateTime.UtcNow;
        public string SourceFile { get; set; } = string.Empty;
        public string SourceProviderCode { get; set; } = string.Empty;

        public EquipmentType EquipmentType { get; set; }
        public List<MeterDataInfo> Metadata { get; set; } = [];
        public bool Ignore { get; set; }
        public bool Controlled { get; set; }
        public SeasonClass? Season { get; set; }

        public List<ChannelDay>? PreviousVersions { get; set; }

    }



}
