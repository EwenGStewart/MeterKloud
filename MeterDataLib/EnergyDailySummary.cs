namespace MeterDataLib
{
    public record struct EnergyDailySummary(DateTime ReadingDateTime, int Meters, int Channels, int IntervalMinutes, Quality Quality
        , decimal  TotalActivePowerConsumption_kWh
        , decimal  TotalActivePowerGeneration_kWh
        , decimal  TotalReactivePowerConsumption_kVArh
        , decimal  TotalReactivePowerGeneration_kVArh
        , decimal  Max_kW
        , decimal  Max_kVA
        , decimal  PowerFactor
        , DateTime TimeOfMax)
    {
          public decimal TotalNetActivePower_kWh => TotalActivePowerConsumption_kWh - TotalActivePowerGeneration_kWh;
        public decimal Max_Kw_atMax_kVa => Max_kVA * PowerFactor;
    }




}
