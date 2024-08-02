using System.Reflection;

namespace MeterDataLib.Parsers
{
    public static class ParserFactory
    {

        static List<Parser> parsers = new List<Parser>() { new ExcelParser(), new ZipParser(), new Nem12(), new CsvMultiLine1(), new CsvPowerPal()
            , new CsvSingleLine7()
            , new CsvByChannel()
            , new CsvSingleLineMultiColPeriod()
            , new CsvSingleLineMultiColPeriod2()
            , new CsvSingleLinePeakOffPeakDateNumber()
            , new CsvSingleLineSimpleEBQK()
            , new CsvSingleLineSimpleEBKvaPF()
        };

        public static List<Parser> GetParserTypes()
       {
            return parsers;

        //    var parserType = typeof(IParser); 

           

        //    //var types = Assembly.GetExecutingAssembly().GetTypes()
        //    //    .Where(t => parserType.IsAssignableFrom(t) && t.GetMethod("GetParser", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) != null)
        //    //    .Where( t=> t.IsInterface == false)
        //    //    .ToList();


        //    return types;
        }


        public static Parser? GetParser(Stream stream, string filename, string? mimeType)
        {
            Stream seekableStream = stream;
            if ( stream.CanSeek == false)
            {
                seekableStream = StreamConverter.ConvertToMemoryStream(stream);
            }


            foreach( var parser in GetParserTypes())
            {
                if ( seekableStream.Position != 0 )
                {
                    seekableStream.Seek(0, System.IO.SeekOrigin.Begin);
                }
                if ( parser.CanParse(seekableStream,filename,mimeType))
                {

                    return parser;
                }
            }
            return null;
        }
        public static async Task<Parser?> GetParserAsync(Stream stream, string filename, string? mimeType)
        {
            Stream seekableStream = stream;
            if (stream.CanSeek == false)
            {
                seekableStream = await  StreamConverter.ConvertToMemoryStreamAsync(stream);
            }


            foreach (var parser in GetParserTypes())
            {
                if (seekableStream.Position != 0)
                {
                    seekableStream.Seek(0, System.IO.SeekOrigin.Begin);
                }
                if (parser.CanParse(seekableStream, filename, mimeType))
                {

                    return parser;
                }
            }
            return null;
        }


        public static ParserResult Parse(Stream stream, string filename, string? mimeType)
        {
            ParserResult result = new ParserResult() { FileName = filename };
            try
            {
                var parser = GetParser(stream, filename, mimeType);
                if (parser == null)
                {
                    result.LogMessages.Add(new FileLogMessage("[I4GCND]: No parser found", Microsoft.Extensions.Logging.LogLevel.Error, filename, 0, 0));
                    return result;
                }
                try
                {
                    result.LogMessages.Add(new FileLogMessage($"[Y1PRWV]: Parsing with {parser.Name}", Microsoft.Extensions.Logging.LogLevel.Information, filename, 0, 0));


                    Stream seekableStream = stream;
                    if (stream.CanSeek == false)
                    {
                        seekableStream = StreamConverter.ConvertToMemoryStream(stream);
                    }

                    if (seekableStream.Position != 0)
                    {
                        seekableStream.Seek(0, System.IO.SeekOrigin.Begin);
                    }

                    result = parser.Parse(seekableStream, filename);
                    result.ParserName = parser.Name;
                    result.FileName = filename;
                    
                    return result;
                } 
                catch (Exception ex)
                { 
                    result.ParserName = parser.Name;
                    result.LogMessages.Add(new FileLogMessage($"[QVR9SE]: {ex.Message}", Microsoft.Extensions.Logging.LogLevel.Critical, filename, 0, 0));
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.LogMessages.Add(new FileLogMessage($"[VDKA7C]: {ex.Message}", Microsoft.Extensions.Logging.LogLevel.Error, filename, 0, 0));
                return result;
            }

        }



        public static  async Task<ParserResult> ParseAsync(Stream stream, string filename, string? mimeType)
        {
            ParserResult result = new ParserResult() { FileName = filename };
            try
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss:fff}  {filename} start ");
                using var seekableStream = await StreamConverter.ConvertToMemoryStreamAsync(stream);
                Console.WriteLine($"{DateTime.Now:HH:mm:ss:fff}  {filename} stream loaded  ");
                var parser = await GetParserAsync(seekableStream, filename, mimeType);
                Console.WriteLine($"{DateTime.Now:HH:mm:ss:fff}  {filename} got parser  ");
                if (parser == null)
                {
                    result.LogMessages.Add(new FileLogMessage("[I4GCND]: No parser found", Microsoft.Extensions.Logging.LogLevel.Error, filename, 0, 0));
                    return result;
                }
                try
                {
                    result.LogMessages.Add(new FileLogMessage($"[Y1PRWV]: Parsing with {parser.Name}", Microsoft.Extensions.Logging.LogLevel.Information, filename, 0, 0));
                    if (seekableStream.Position != 0)
                    {
                        Console.WriteLine($"{DateTime.Now:HH:mm:ss:fff}  {filename} re-seek   ");
                        seekableStream.Seek(0, System.IO.SeekOrigin.Begin);
                    }
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss:fff}  {filename} start parse    ");
                    result = parser.Parse(seekableStream, filename);
                    result.ParserName = parser.Name;
                    result.FileName = filename;
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss:fff}  {filename} completed parse    ");
                    return result;
                }
                catch (Exception ex)
                {
                    result.ParserName = parser.Name;
                    result.LogMessages.Add(new FileLogMessage($"[QVR9SE]: {ex.Message}", Microsoft.Extensions.Logging.LogLevel.Critical, filename, 0, 0));
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.LogMessages.Add(new FileLogMessage($"[VDKA7C]: {ex.Message}", Microsoft.Extensions.Logging.LogLevel.Error, filename, 0, 0));
                return result;
            }

        }


    }



}
