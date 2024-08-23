namespace MeterDataLib
{
    public enum GenerationHandlingOption
        {
        /// <summary>
        ///  Include generation and consumption as per normal 
        /// </summary>
        IncludeGenerationAndConsumption = 0 ,
        /// <summary>
        ///  Net the generation from the consumption such that only one value is returned ( eg one is always zero ) 
        /// </summary>
        NetGeneration = 1,
        /// <summary>
        /// Ignore generation values and only return consumption values
        /// </summary>
        IgnoreGeneration = 2,
        /// <summary>
        ///  Net the generation from the consumption such that only one value is returned ( eg one is always zero ) and ignore the generation values - this will only return consumption or ZERO 
        /// </summary>
        NetAndIgnoreGeneration = 3,
        /// <summary>
        ///  Ignore the consumption values and only return generation values
        /// </summary>
        IgnoreConsumption = 4,

        /// <summary>
        ///  Net the generation from the consumption such that only one value is returned ( eg one is always zero ) and ignore the consumption values - this will only return generation or ZERO
        /// </summary>
        NetAndIgnoreConsumption = 5,

        /// <summary>
        ///  Set both sides to Zero 
        /// </summary>
        IgnoreBoth = 6


    }
 


 

}
