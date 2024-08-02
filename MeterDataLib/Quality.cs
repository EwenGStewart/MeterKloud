namespace MeterDataLib
{


    public enum Quality { Unknown=0, Actual, Null, Final, Substituted, Estimated , Variable}


    public static class QualityExtensions
    {
        public static string ToShortString(this Quality quality)
        {
            return quality switch
            {
                Quality.Actual => "A",
                Quality.Null => "N",
                Quality.Final => "F",
                Quality.Substituted => "S",
                Quality.Estimated => "E",
                Quality.Variable => "V",
                _ =>  "?"
            };
        }

        public static Quality ToQuality(this string quality)
        {
            if ( string.IsNullOrEmpty(quality) || quality.Length < 1)
            {
                return Quality.Unknown;
            }
                

            return quality.Substring(0,1).ToUpper() switch
            {
                "A" => Quality.Actual,
                "N" => Quality.Null,
                "F" => Quality.Final,
                "S" => Quality.Substituted,
                "E" => Quality.Estimated,
                "V" => Quality.Variable,
                "?" => Quality.Unknown,
                _ => Quality.Unknown
            };
        }
    }

}
