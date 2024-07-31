namespace MeterDataLib
{
    public struct IndexRead
    {

        public DateTime TimeStamp { get; set; }
        public decimal Value { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public decimal? Multiplier { get; set; }
        public Quality Quality { get; set; }

        public bool SetFromString(string read)
        {
            // try and convert the string to a decimal 
            if (decimal.TryParse(read, out decimal value))
            {
                Value = value;
                bool decimalPointFound = false;
                Precision = 0;
                Scale = 0;
                foreach (var c in read)
                {
                    if (char.IsNumber(c))
                    {
                        Precision++;
                        if (decimalPointFound)
                        {
                            Scale++;
                        }
                    }
                    else if (c == '.')
                    {
                        decimalPointFound = true;
                    }
                }
                return true;
            }
            return false;
        }



    }


}
