using Microsoft.Extensions.Logging;

namespace MeterDataLib.Parsers
{
    public record FileLogMessage (string LogMessage , LogLevel LogLevel = LogLevel.Information, string FileName = "", int LineNumber=0, int? ColumnNumber=null) 
    {
        
        public override string ToString()
        {
            return $"{LogLevel} {LogMessage}  File:{FileName} Line:{LineNumber} Col:{ColumnNumber} ";
        }
    }

}
