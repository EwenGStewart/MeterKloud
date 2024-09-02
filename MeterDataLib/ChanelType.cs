namespace MeterDataLib
{
    public enum ChanelType
    {
        Unknown = 0 , 
        ActiveEnergyConsumption ,
        ActiveEnergyGeneration,
        ReactiveEnergyConsumption ,
        ReactiveEnergyGeneration ,

        Volts ,
        Current , 
        PowerFactor ,
        RealPower , 
        ReactivePower ,
        ApparentPower ,
        ApparentPowerConsumption,
        ApparentPowerGeneration,
        Cost,
        Other, 
    }

}
