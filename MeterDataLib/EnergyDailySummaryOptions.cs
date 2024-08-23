namespace MeterDataLib
{
    public record EnergyDailySummaryOptions
    {
        public int Interval { get; set; } = 30;
        public GenerationHandlingOption RealPowerGeneration = GenerationHandlingOption.IgnoreGeneration;
        public GenerationHandlingOption ReactivePowerGeneration = GenerationHandlingOption.IncludeGenerationAndConsumption;
        public bool CalculateKvaAtMaxKw { get; set; } = false; 


    }




}
