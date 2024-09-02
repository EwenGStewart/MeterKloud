namespace MeterDataLib
{
    public record struct EnergyDailySummary(DateTime ReadingDateTime, int Meters, int Channels, int IntervalMinutes, Quality Quality
        , decimal  TotalActiveEnergyConsumption_kWh
        , decimal  TotalActiveEnergyGeneration_kWh
        , decimal  TotalReactiveEnergyConsumption_kVArh
        , decimal  TotalReactiveEnergyGeneration_kVArh
        , decimal  Max_kW
        , decimal  Max_kVA
        , decimal  PowerFactor
        , DateTime TimeOfMax)
    {
          public decimal TotalNetActivePower_kWh => TotalActiveEnergyConsumption_kWh - TotalActiveEnergyGeneration_kWh;
        public decimal Max_Kw_atMax_kVa => Max_kVA * PowerFactor;
    }




}
