namespace MeterDataLib
{
    public struct IndexRead
    {

        public DateTime TimeStamp { get; set; }
        public decimal Value { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }

        public int LeftDials()
        {
            int power = 1 + (int)Math.Floor(Math.Log10(Math.Abs((double)Value)));
            if (Precision.HasValue)
            {
                if ((Precision.Value - (Scale ?? 0)) > power)
                {
                    power = Precision.Value - (Scale ?? 0);
                }
            }
            return power;
        }

        public int RightDials()
        {
            var scale = Scale ?? GetScale(Value);
            if (scale < 0)
            {
                return 0;
            }
            return scale;

        }


        int GetScale(decimal x)
        {
            var scale = 0;
            var str = x.ToString();
            var decimalIndex = str.IndexOf('.');
            if (decimalIndex >= 0)
            {
                scale = str.Length - decimalIndex - 1;
            }
            return scale;
        }


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

        public override string ToString()
        {
            string left = new('0', LeftDials());
            string right = new('0', RightDials());
            return Value.ToString(left + "." + right);
        }
    }

}
