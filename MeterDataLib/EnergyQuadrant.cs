using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MeterDataLib
{

    public record struct EnergyQuadrant(DateTime ReadingDateTime, string? Meter, string? ChannelNumber, string? ChannelList, int IntervalMinutes, Quality Quality
        , decimal ActivePowerConsumption_kWh
        , decimal ActivePowerGeneration_kWh
        , decimal ReactivePowerConsumption_kVArh
        , decimal ReactivePowerGeneration_kVArh
        )
    {

        public decimal NetActivePower_kWh => ActivePowerConsumption_kWh - ActivePowerGeneration_kWh;
        public decimal NetReactivePower_kVArh => ReactivePowerConsumption_kVArh - ReactivePowerGeneration_kVArh;
        public decimal RealPowerConsumption_Kw => IntervalMinutes <= 0 ? 0 : 60 / IntervalMinutes * ActivePowerConsumption_kWh;
        public decimal RealPowerGeneration_kW => IntervalMinutes <= 0 ? 0 : 60 / IntervalMinutes * ActivePowerGeneration_kWh;
        public decimal RealPowerNet_kW => IntervalMinutes <= 0 ? 0 : 60 / IntervalMinutes * NetActivePower_kWh;

        public int Quadrant =>
              NetActivePower_kWh == 0 && NetReactivePower_kVArh == 0 ? 0
            : NetActivePower_kWh >= 0 && NetReactivePower_kVArh >= 0 ? 1
            : NetActivePower_kWh < 0 && NetReactivePower_kVArh >= 0 ? 2
            : NetActivePower_kWh < 0 && NetReactivePower_kVArh < 0 ? 3
            : 4;

        public bool Inductive => Quadrant == 1 || Quadrant == 2;

        public bool LaggingCurrent => Inductive;
        public bool Capacitive => Quadrant == 3 || Quadrant == 4;
        public bool LeadingCurrent => Inductive;



        public decimal ApparentPower_kVA => IntervalMinutes <= 0 ? 0 : 60 / IntervalMinutes * (decimal)Math.Sqrt(Math.Pow((double)ActivePowerConsumption_kWh, 2) + Math.Pow((double)NetReactivePower_kVArh, 2));
        public decimal PowerFactor => IntervalMinutes <= 0 || ApparentPower_kVA == 0 ? 0 : RealPowerConsumption_Kw / ApparentPower_kVA;



        public decimal ApparentPower_Net_kVA => IntervalMinutes <= 0 ? 0 : 60 / IntervalMinutes * (decimal)Math.Sqrt(Math.Pow((double)NetActivePower_kWh, 2) + Math.Pow((double)NetReactivePower_kVArh, 2));
        public decimal PowerFactor_Net => ApparentPower_Net_kVA == 0 ? 0 : RealPowerNet_kW / ApparentPower_Net_kVA;


        public double PhaseAngle => IntervalMinutes <= 0 ? 0 : Math.Acos(Math.Abs((double)PowerFactor_Net)) * 180 / Math.PI;

        public override string ToString()
        {
            return $"{ReadingDateTime:yyyy-MM-dd HH:mm},M={Meter},C={ChannelNumber},Chans={ChannelList},I={IntervalMinutes},Q={Quality},E={ActivePowerConsumption_kWh},B={ActivePowerGeneration_kWh},Q={ReactivePowerConsumption_kVArh},K={ReactivePowerGeneration_kVArh},Kw={RealPowerConsumption_Kw:0.0000.000},Kva={ApparentPower_kVA:0.000},PF={PowerFactor:0.000}";
        }
        public string ToCsvString()
        {
            return $"{ReadingDateTime:yyyy-MM-dd HH:mm},{Meter},{ChannelNumber},{ChannelList},{IntervalMinutes},{Quality},{ActivePowerConsumption_kWh},{ActivePowerGeneration_kWh},{ReactivePowerConsumption_kVArh},{ReactivePowerGeneration_kVArh},{RealPowerConsumption_Kw:0.000},{ApparentPower_kVA:0.000},{PowerFactor:0.000}";
        }



        public Decimal CalculateKva ( 
            GenerationHandlingOption realPowerHandling  = GenerationHandlingOption.IgnoreGeneration , 
            GenerationHandlingOption reactivePowerHandling = GenerationHandlingOption.IncludeGenerationAndConsumption,
            bool setKvaToZeroWhenActiveGenerationExceedsConsumption = false
            )
        {

            decimal realPower = 0;
            decimal reactivePower = 0;
            switch (realPowerHandling)
            {
                case GenerationHandlingOption.IncludeGenerationAndConsumption:
                case GenerationHandlingOption.NetGeneration:
                    realPower  = NetActivePower_kWh;
                    break;
                case GenerationHandlingOption.NetAndIgnoreGeneration:
                    realPower = Math.Max(0, NetActivePower_kWh) ;
                    break; 

                case GenerationHandlingOption.IgnoreGeneration:
                    realPower = ActivePowerConsumption_kWh;
                    break;
        
                    
                case GenerationHandlingOption.IgnoreConsumption:
                    realPower = ActivePowerGeneration_kWh;
                    break;

                case GenerationHandlingOption.NetAndIgnoreConsumption:
                    realPower = Math.Max(0, NetActivePower_kWh*-1m);
                    break;
                case GenerationHandlingOption.IgnoreBoth:
                    realPower = 0;
                    break;
            }
            switch (reactivePowerHandling)
            {
                case GenerationHandlingOption.IncludeGenerationAndConsumption:
                case GenerationHandlingOption.NetGeneration:
                    reactivePower = NetReactivePower_kVArh;
                    break;
                case GenerationHandlingOption.NetAndIgnoreGeneration:
                    reactivePower = Math.Max(0, NetReactivePower_kVArh);
                    break;

                case GenerationHandlingOption.IgnoreGeneration:
                    reactivePower = ReactivePowerConsumption_kVArh;
                    break;


                case GenerationHandlingOption.IgnoreConsumption:
                    reactivePower = ReactivePowerGeneration_kVArh;
                    break;

                case GenerationHandlingOption.NetAndIgnoreConsumption:
                    reactivePower = Math.Max(0, NetReactivePower_kVArh * -1m);
                    break;
                case GenerationHandlingOption.IgnoreBoth:
                    reactivePower = 0;
                    break;
            }

            if ( setKvaToZeroWhenActiveGenerationExceedsConsumption && NetActivePower_kWh < 0)
            {
                return 0;
            }
            return   IntervalMinutes ==0 ? 0 :  60 / (decimal)IntervalMinutes *  (decimal)Math.Sqrt(Math.Pow((double)realPower, 2) + Math.Pow((double)reactivePower, 2));
        }




    }
}