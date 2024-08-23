namespace MeterDataLib
{

    public enum FlagFilter
    {
        Ignore = 0,
        Include = 1,
        Exclude = 2
    }


    public record QuadrantOptions
        {
            public bool ByMeter { get; set; } = false;
            public int? Interval { get; set; } = null; 
            public string[] OnlyIncludeMeterFilter  { get; set; } = [];
            public string[] OnlyIncludeChannelFilter { get; set; } = [];
            public string[] OnlyIncludeChanelNumberFilter { get; set; } = [];

            public string[] AlwaysExcludeMeterFilter { get; set; } = [];
            public string[] AlwaysExcludeChannelFilter { get; set; } = [];
            public string[] AlwaysExcludeChanelNumberFilter { get; set; } = [];

            public FlagFilter ControlledLoad { get; set; } = FlagFilter.Ignore;
            public bool UseSimpleIntervalCorrection { get; set; } = false; 


            public GenerationHandlingOption RealPowerGenerationHandling { get; set; } = GenerationHandlingOption.IncludeGenerationAndConsumption;
            public GenerationHandlingOption ReactivePowerGenerationHandling { get; set; } = GenerationHandlingOption.IncludeGenerationAndConsumption;

    }
 


 

}
