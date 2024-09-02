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
        , decimal ActiveEnergyConsumption_kWh
        , decimal ActiveEnergyGeneration_kWh
        , decimal ReactiveEnergyConsumption_kVArh
        , decimal ReactiveEnergyGeneration_kVArh
        )
    {

        public decimal NetActiveEnergy_kWh => ActiveEnergyConsumption_kWh - ActiveEnergyGeneration_kWh;
        public decimal NetReactiveEnergy_kVArh => ReactiveEnergyConsumption_kVArh - ReactiveEnergyGeneration_kVArh;
        public decimal RealPowerConsumption_kW => IntervalMinutes <= 0 ? 0 : 60 / IntervalMinutes * ActiveEnergyConsumption_kWh;
        public decimal RealPowerGeneration_kW => IntervalMinutes <= 0 ? 0 : 60 / IntervalMinutes * ActiveEnergyGeneration_kWh;
        public decimal RealPowerNet_kW => IntervalMinutes <= 0 ? 0 : 60 / IntervalMinutes * NetActiveEnergy_kWh;

        public decimal ReactivePowerConsumption_kVAr => IntervalMinutes <= 0 ? 0 : 60 / IntervalMinutes * ReactiveEnergyConsumption_kVArh;
        public decimal ReactivePowerGeneration_kVAr => IntervalMinutes <= 0 ? 0 : 60 / IntervalMinutes * ReactiveEnergyGeneration_kVArh;
        public decimal ReactivePowerNet_kVAr => IntervalMinutes <= 0 ? 0 : 60 / IntervalMinutes * NetReactiveEnergy_kVArh;




        public int Quadrant =>
              NetActiveEnergy_kWh == 0 && NetReactiveEnergy_kVArh == 0 ? 0
            : NetActiveEnergy_kWh >= 0 && NetReactiveEnergy_kVArh >= 0 ? 1
            : NetActiveEnergy_kWh < 0 && NetReactiveEnergy_kVArh >= 0 ? 2
            : NetActiveEnergy_kWh < 0 && NetReactiveEnergy_kVArh < 0 ? 3
            : 4;

        public bool Inductive => Quadrant == 1 || Quadrant == 2;

        public bool LaggingCurrent => Inductive;
        public bool Capacitive => Quadrant == 3 || Quadrant == 4;
        public bool LeadingCurrent => Inductive;



        public decimal ApparentPower_kVA => IntervalMinutes <= 0 ? 0 : 60 / IntervalMinutes * (decimal)Math.Sqrt(Math.Pow((double)ActiveEnergyConsumption_kWh, 2) + Math.Pow((double)NetReactiveEnergy_kVArh, 2));
        public decimal PowerFactor => IntervalMinutes <= 0 || ApparentPower_kVA == 0 ? 0 : RealPowerConsumption_kW / ApparentPower_kVA;



        public decimal ApparentPower_Net_kVA => IntervalMinutes <= 0 ? 0 : 60 / IntervalMinutes * (decimal)Math.Sqrt(Math.Pow((double)NetActiveEnergy_kWh, 2) + Math.Pow((double)NetReactiveEnergy_kVArh, 2));
        public decimal PowerFactor_Net => ApparentPower_Net_kVA == 0 ? 0 : RealPowerNet_kW / ApparentPower_Net_kVA;


        public double PhaseAngle => IntervalMinutes <= 0 ? 0 : Math.Acos(Math.Abs((double)PowerFactor_Net)) * 180 / Math.PI;

        public override string ToString()
        {
            return $"{ReadingDateTime:yyyy-MM-dd HH:mm},M={Meter},C={ChannelNumber},Chans={ChannelList},I={IntervalMinutes},Q={Quality},E={ActiveEnergyConsumption_kWh},B={ActiveEnergyGeneration_kWh},Q={ReactiveEnergyConsumption_kVArh},K={ReactiveEnergyGeneration_kVArh},Kw={RealPowerConsumption_kW:0.0000.000},Kva={ApparentPower_kVA:0.000},PF={PowerFactor:0.000}";
        }
        public string ToCsvString()
        {
            return $"{ReadingDateTime:yyyy-MM-dd HH:mm},{Meter},{ChannelNumber},{ChannelList},{IntervalMinutes},{Quality},{ActiveEnergyConsumption_kWh},{ActiveEnergyGeneration_kWh},{ReactiveEnergyConsumption_kVArh},{ReactiveEnergyGeneration_kVArh},{RealPowerConsumption_kW:0.000},{ApparentPower_kVA:0.000},{PowerFactor:0.000}";
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
                    realPower  = NetActiveEnergy_kWh;
                    break;
                case GenerationHandlingOption.NetAndIgnoreGeneration:
                    realPower = Math.Max(0, NetActiveEnergy_kWh) ;
                    break; 

                case GenerationHandlingOption.IgnoreGeneration:
                    realPower = ActiveEnergyConsumption_kWh;
                    break;
        
                    
                case GenerationHandlingOption.IgnoreConsumption:
                    realPower = ActiveEnergyGeneration_kWh;
                    break;

                case GenerationHandlingOption.NetAndIgnoreConsumption:
                    realPower = Math.Max(0, NetActiveEnergy_kWh*-1m);
                    break;
                case GenerationHandlingOption.IgnoreBoth:
                    realPower = 0;
                    break;
            }
            switch (reactivePowerHandling)
            {
                case GenerationHandlingOption.IncludeGenerationAndConsumption:
                case GenerationHandlingOption.NetGeneration:
                    reactivePower = NetReactiveEnergy_kVArh;
                    break;
                case GenerationHandlingOption.NetAndIgnoreGeneration:
                    reactivePower = Math.Max(0, NetReactiveEnergy_kVArh);
                    break;

                case GenerationHandlingOption.IgnoreGeneration:
                    reactivePower = ReactiveEnergyConsumption_kVArh;
                    break;


                case GenerationHandlingOption.IgnoreConsumption:
                    reactivePower = ReactiveEnergyGeneration_kVArh;
                    break;

                case GenerationHandlingOption.NetAndIgnoreConsumption:
                    reactivePower = Math.Max(0, NetReactiveEnergy_kVArh * -1m);
                    break;
                case GenerationHandlingOption.IgnoreBoth:
                    reactivePower = 0;
                    break;
            }

            if ( setKvaToZeroWhenActiveGenerationExceedsConsumption && NetActiveEnergy_kWh < 0)
            {
                return 0;
            }
            return   IntervalMinutes ==0 ? 0 :  60 / (decimal)IntervalMinutes *  (decimal)Math.Sqrt(Math.Pow((double)realPower, 2) + Math.Pow((double)reactivePower, 2));
        }




    }
}