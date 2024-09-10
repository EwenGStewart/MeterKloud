namespace MeterDataLib.Query
{
    public class TimeSeries
    {
        private QueryDateRange dateRange = new(DateTime.MinValue, DateTime.MaxValue);
        private TimeInterval interval = new(TimeIntervalSize.Day, 1);
        private DateTime[]? dateTimes = null;
        private readonly List<QueryColumn> columns = [];




        public QueryDateRange DateRange { get => dateRange; set { dateRange = value; dateTimes = null; } }
        public TimeInterval Interval { get => interval; set { interval = value; dateTimes = null; } }
        public DateTime[] DateTimes
        {
            get
            {
                dateTimes ??= Interval.Range(DateRange); return dateTimes;
            }
        }

        public int NumberOfPoints
        {
            get
            {
                return DateTimes.Length;
            }
        }


        public void AddColumn(string name, QueryColumnTypes type ,  object?[] values)
        {
            name = name.Trim();
            if (columns.Any(x => x.Name == name))
            {
                throw new ArgumentException($"Column {name} already exists");
            }
            if (values.Length != NumberOfPoints)
            {
                throw new ArgumentException($"Column {name} has {values.Length} values but expected {NumberOfPoints}");
            }
            columns.Add(new QueryColumn(name, type, values));
        }

        public void AddColumn(string name, decimal?[] values) => AddColumn(name, QueryColumnTypes.Numeric, values.Cast<object?>().ToArray());
        public void AddColumn(string name, decimal[] values) => AddColumn(name, QueryColumnTypes.Numeric, values.Cast<object?>().ToArray());

        public void AddColumn(string name, string?[] values) => AddColumn(name, QueryColumnTypes.String, values.Cast<object?>().ToArray());
        
        public void AddColumn(string name, DateTime[] values) => AddColumn(name, QueryColumnTypes.Time, values.Cast<object?>().ToArray());
        public void AddColumn(string name, DateTime?[] values) => AddColumn(name, QueryColumnTypes.Time, values.Cast<object?>().ToArray());

    }




}
