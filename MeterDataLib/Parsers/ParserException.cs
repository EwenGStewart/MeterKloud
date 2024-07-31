namespace MeterDataLib.Parsers
{
    public class  ParserException : Exception
    {
    
        public int Col { get; init; }
        public int Line { get; init;  }

        public string Value { get; init; }

        public ParserException(string message, string value , int  line , int col ) : base(message)
        {
            Line = line; 
            Col = col;
            Value = value;
        }
    }
        





}
