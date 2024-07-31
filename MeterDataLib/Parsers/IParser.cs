using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MeterDataLib.Parsers
{
    public  interface IParser
    {
        static abstract  IParser? GetParser(Stream stream, string filename, string? mimeType);
        ParserResult Parse(Stream stream , string filename );

        string Name { get; }


    }


    public static class ParserFactory
    {

        public static List<Type> GetParserTypes()
        {
            var parserType = typeof(IParser);
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => parserType.IsAssignableFrom(t) && t.GetMethod("GetParser", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) != null)
                .Where( t=> t.IsInterface == false)
                .ToList();
            return types;
        }


        public static IParser? GetParser(Stream stream, string filename, string? mimeType)
        {
            foreach( var parser in GetParserTypes())
            {

                var method = parser.GetMethod("GetParser" );
                
                var result = method?.Invoke(null, new object[] { stream, filename, mimeType! });
                if (result != null) return (IParser)result;
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
                    result.LogMessages.Add(new FileLogMessage("No parser found", Microsoft.Extensions.Logging.LogLevel.Error, filename, 0, 0));
                    return result;
                }
                try
                {
                    result = parser.Parse(stream, filename);
                    if (string.IsNullOrWhiteSpace(result.ParserName))
                    {
                        result.ParserName = parser.Name;
                    }
                    if (string.IsNullOrWhiteSpace(result.FileName))
                    {
                        result.FileName = filename;
                    }
                    return result;
                } 
                catch (Exception ex)
                { 
                    result.ParserName = parser.Name;
                    result.LogMessages.Add(new FileLogMessage(ex.Message, Microsoft.Extensions.Logging.LogLevel.Critical, filename, 0, 0));
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.LogMessages.Add(new FileLogMessage(ex.Message, Microsoft.Extensions.Logging.LogLevel.Error, filename, 0, 0));
                return result;
            }

        }




    }



}
