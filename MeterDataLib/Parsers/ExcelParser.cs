using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

using System.Data;
using ExcelDataReader;
using System.Net.Http.Headers;

namespace MeterDataLib.Parsers
{
    //public class ExcelParser : Parser
    //{
    //    static readonly string[] ExcelMimeTypes = new string[]
    //    {
    //        "application/vnd.ms-excel",
    //        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    //        "application/msexcel",
    //        "application/vnd.ms-excel"
    //    };

    //    public override string Name => "Excel";


    //    public override bool CanParseType(string mimeType)
    //    {
    //        if (ExcelMimeTypes.Contains(mimeType?.ToLowerInvariant() ?? string.Empty))
    //        {
    //            return true;
    //        }
    //        return false;
    //    }


    //    public override bool CanParseInternal(Stream stream, string filename, string? mimeType)
    //    {
    //         if (ExcelMimeTypes.Contains(mimeType?.ToLowerInvariant() ?? string.Empty))
    //         {
    //            return true; 
    //         }
    //         if ( filename.EndsWith(".xls") || filename.EndsWith(".xlsx"))
    //         {
    //            return true; 
    //         }
    //        return false; 
    //    }

    //    public override ParserResult Parse(Stream stream, string filename)
    //    {
    //        var result = new ParserResult();
    //        result.FileName = filename;
    //        result.ParserName = Name;

    //        stream.Position = 0;
    //        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    //        using (var edr = ExcelReaderFactory.CreateReader(stream))
    //        {

           
    //            int sheet = 0;
    //            StringWriter stringWriter = new StringWriter();
    //            do
    //            {
    //                sheet++;

    //                int line = 0;
    //                while (edr.Read())
    //                {


    //                    line++;
    //                    int prevCol = 0;
    //                    for (int i = 0; i < edr.FieldCount; i++)
    //                    {
    //                        var value = edr.GetValue(i);
    //                        if (value != null )
    //                        {
    //                            string strValue = value?.ToString() ?? string.Empty;
    //                            if ( ! string.IsNullOrEmpty(strValue))
    //                            {
    //                                strValue.Replace(",", "|");
    //                                if ( i > 0 )
    //                                {
    //                                    while (prevCol < i)
    //                                    {
    //                                        stringWriter.Write(",");
    //                                        prevCol++;
    //                                    }
    //                                }
    //                                stringWriter.Write(strValue);
    //                            }
    //                        }
    //                    }
    //                    stringWriter.WriteLine();
    //                }
    //                var data = stringWriter.ToString();
    //                if ( ! string.IsNullOrEmpty(data))
    //                {
    //                    result.LogMessages.Add(new FileLogMessage($"Processing Sheet {sheet}", Microsoft.Extensions.Logging.LogLevel.Information, filename, 0, 0));
    //                    using (Stream strStream = StringToStream.GenerateStreamFromString(data))
    //                    {
    //                        var parser = ParserFactory.GetParser(strStream, $"{filename}-{sheet}.csv", "text/csv");
    //                        bool actualResult = parser != null;
    //                        if (parser == null)
    //                        {
    //                            result.LogMessages.Add(new FileLogMessage($"Could not detect the format for Sheet {sheet}", Microsoft.Extensions.Logging.LogLevel.Error, filename, 0, 0));
    //                        }
    //                        else
    //                        {
    //                            try
    //                            {
    //                                var sheetResult = parser.Parse(strStream, $"{filename}-{sheet}.csv");
    //                                result.LogMessages.AddRange(sheetResult.LogMessages);
    //                                result.SitesDays.AddRange(sheetResult.SitesDays);
    //                            }
    //                            catch (Exception ex)
    //                            {
    //                                result.LogMessages.Add(new FileLogMessage($"Error processing sheet {sheet} - {ex.Message}", Microsoft.Extensions.Logging.LogLevel.Error, filename, 0, 0));
    //                            }
    //                        }
    //                    }
    //                }
    //            } while (edr.NextResult());
    //        }
    //        return result;
    //    }
    //}
}
