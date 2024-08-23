namespace MeterDataLib
{
    public record MeterDataInfo
    {
        public MeterDataInfo()  
        {
        }

        public MeterDataInfo(string metaDataName, int value) : this(metaDataName, 0, 0, value.ToString())
        {

        }



        public MeterDataInfo(string metaDataName, string value)  : this(metaDataName, 0, 0, value)
        {
            
        }

        public MeterDataInfo(string metaDataName, int fromIndex, int toIndex, int value) : this(metaDataName, fromIndex, toIndex, value.ToString())
        { }
        public MeterDataInfo(string metaDataName, int fromIndex, int toIndex, string value)
        {
            this.MeterDataName = metaDataName;
            this.FromIndex = fromIndex;
            this.ToIndex = toIndex;
            this.Value = value;
        }



        public string MeterDataName { get; set; } = string.Empty;
        public int FromIndex { get; set; } 
        public  int ToIndex { get; set; }
        public string Value { get; set; } = string.Empty;



    }



}
