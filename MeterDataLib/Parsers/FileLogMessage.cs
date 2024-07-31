using Microsoft.Extensions.Logging;

namespace MeterDataLib.Parsers
{
    public record FileLogMessage (string LogMessage , LogLevel LogLevel = LogLevel.Information, string FileName = "", int LineNumber=0, int? ColumnNumber=null) 
    {
        

    }

}
