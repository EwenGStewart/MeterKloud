using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

using System.Data;
using System.IO.Compression;
using System.Net.Http.Headers;

namespace MeterDataLib.Parsers
{
    public class ZipParser : IParser
    {
        public string Name => "Zip";


        static readonly string[] ZipMimeTypes = new string[]
        {
            "application/x-zip-compressed",

        };
        public static IParser? GetParser(Stream stream, string filename, string? mimeType)
        {
            if (ZipMimeTypes.Contains(mimeType?.ToLowerInvariant() ?? string.Empty))
            {
                return new ZipParser();
            }
            if (filename.EndsWith(".zip"))
            {
                return new ZipParser();
            }
            return null;
        }

        public ParserResult Parse(Stream stream, string filename)
        {
            var result = new ParserResult();
            result.FileName = filename;
            result.ParserName = Name;

            stream.Position = 0;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                foreach (var entry in zip.Entries)
                {
                    using var entryStream = entry.Open();
                    using var memStream = StreamConverter.ConvertToMemoryStream(entryStream);

                    result.LogMessages.Add(new FileLogMessage($"Processing file {entry.Name}", Microsoft.Extensions.Logging.LogLevel.Information, filename, 0, 0));
                    string mimeType = MimeTypeHelper.GetMimeType(entry.Name);
                    var parser = ParserFactory.GetParser(memStream, entry.Name, mimeType);
                    if (parser != null)
                    {
                        try
                        {
                            var entryResult = parser.Parse(memStream, entry.FullName);
                            result.LogMessages.AddRange(entryResult.LogMessages);
                            result.SitesDays.AddRange(entryResult.SitesDays);
                        }
                        catch (Exception ex)
                        {
                            result.LogMessages.Add(new FileLogMessage($"Error processing file {entry.Name} - {ex.Message}", Microsoft.Extensions.Logging.LogLevel.Error, filename, 0, 0));
                        }
                    }
                    else
                    {
                        result.LogMessages.Add(new FileLogMessage($"Could not detect the format for file {entry.Name}", Microsoft.Extensions.Logging.LogLevel.Error, filename, 0, 0));
                    }
                }
            }
            return result;
        }
    }
}
