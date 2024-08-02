using System.Diagnostics;

namespace MeterDataLib
{
    public enum UnitOfMeasure
    {
         Other = 0 ,
         MWh,
         kWh ,
         Wh,
         MVArh,
         kVArh,
         VArh,
         MVAr ,
         kVAr ,
         VAr,
         MW ,
         kW,
         W,
         MVAh , 
         kVAh ,
         VAh,
         MVA,
         kVA,
         VA,
         kV,
         V,
         kA,
         A,
         pf,
         Dollars
    }



    public static class UnitOfMeasureExtensions
    {
        public static string ToSymbol(this UnitOfMeasure uom)
        {
            return uom switch
            {
                UnitOfMeasure.MWh => "MWh",
                UnitOfMeasure.kWh => "kWh",
                UnitOfMeasure.Wh => "Wh",
                UnitOfMeasure.MVArh => "MVArh",
                UnitOfMeasure.kVArh => "kVArh",
                UnitOfMeasure.VArh => "VArh",
                UnitOfMeasure.MVAr => "MVAr",
                UnitOfMeasure.kVAr => "kVAr",
                UnitOfMeasure.VAr => "VAr",
                UnitOfMeasure.MW => "MW",
                UnitOfMeasure.kW => "kW",
                UnitOfMeasure.W => "W",
                UnitOfMeasure.MVAh => "MVAh",
                UnitOfMeasure.kVAh => "kVAh",
                UnitOfMeasure.VAh => "VAh",
                UnitOfMeasure.MVA => "MVA",
                UnitOfMeasure.kVA => "kVA",
                UnitOfMeasure.VA => "VA",
                UnitOfMeasure.kV => "kV",
                UnitOfMeasure.V => "V",
                UnitOfMeasure.kA => "kA",
                UnitOfMeasure.A => "A",
                UnitOfMeasure.pf => "pf",
                _ => "Other"
            };
        }

        public static (UnitOfMeasure Standard , decimal Factor ) ToStandardUnit ( this UnitOfMeasure uom)
        {
            return uom switch
            {
                UnitOfMeasure.MWh => ( UnitOfMeasure.kWh , 1000m ),
                UnitOfMeasure.kWh => (UnitOfMeasure.kWh, 1m),
                UnitOfMeasure.Wh => (UnitOfMeasure.kWh, 0.001m),
                UnitOfMeasure.MVArh => (UnitOfMeasure.kVArh, 1000m),
                UnitOfMeasure.kVArh => (UnitOfMeasure.kVArh, 1m),
                UnitOfMeasure.VArh => (UnitOfMeasure.kVArh, 0.001m),
                UnitOfMeasure.MVAr => (UnitOfMeasure.kVAr, 1000m),
                UnitOfMeasure.kVAr => (UnitOfMeasure.kVAr, 1m),
                UnitOfMeasure.VAr => (UnitOfMeasure.kVArh, 0.001m),
                UnitOfMeasure.MW => (UnitOfMeasure.kVArh, 1000m),
                UnitOfMeasure.kW => (UnitOfMeasure.kW, 1m),
                UnitOfMeasure.W => (UnitOfMeasure.kW, 0.001m),
                UnitOfMeasure.MVAh => (UnitOfMeasure.kVAh, 1000m),
                UnitOfMeasure.kVAh => (UnitOfMeasure.kVAh, 1m),
                UnitOfMeasure.VAh => (UnitOfMeasure.kVAh, 0.001m),
                UnitOfMeasure.MVA => (UnitOfMeasure.kVAh, 1000m),
                UnitOfMeasure.kVA => (UnitOfMeasure.kVA, 1m),
                UnitOfMeasure.VA => (UnitOfMeasure.kVA, 0.001m),
                UnitOfMeasure.kV => (UnitOfMeasure.kV, 1m),
                UnitOfMeasure.V => (UnitOfMeasure.kV, 0.001m),
                UnitOfMeasure.kA => (UnitOfMeasure.kA, 1m),
                UnitOfMeasure.A => (UnitOfMeasure.kA, 0.001m),
                UnitOfMeasure.pf => (UnitOfMeasure.pf, 1m),
                _ => (UnitOfMeasure.Other, 1m)
            };
        }

        public static UnitOfMeasure ToUom(this string value)
        {
            return value.ToLower().Trim() switch
            {
                "mwh" => UnitOfMeasure.MWh,
                "kwh" => UnitOfMeasure.kWh,
                "wh" => UnitOfMeasure.Wh,
                "mvarh" => UnitOfMeasure.MVArh,
                "kvarh" => UnitOfMeasure.kVArh,
                "varh" => UnitOfMeasure.VArh,
                "mvar" => UnitOfMeasure.MVAr,
                "kvar" => UnitOfMeasure.kVAr,
                "var" => UnitOfMeasure.VAr,
                "mw" => UnitOfMeasure.MW,
                "kw" => UnitOfMeasure.kW,
                "w" => UnitOfMeasure.W,
                "mvah" => UnitOfMeasure.MVAh,
                "kvah" => UnitOfMeasure.kVAh,
                "vah" => UnitOfMeasure.VAh,
                "mva" => UnitOfMeasure.MVA,
                "kva" => UnitOfMeasure.kVA,
                "va" => UnitOfMeasure.VA,
                "kv" => UnitOfMeasure.kV,
                "v" => UnitOfMeasure.V,
                "ka" => UnitOfMeasure.kA,
                "a" => UnitOfMeasure.A,
                "pf" => UnitOfMeasure.pf,
                _ => UnitOfMeasure.Other
            };
        }

    }   




}
