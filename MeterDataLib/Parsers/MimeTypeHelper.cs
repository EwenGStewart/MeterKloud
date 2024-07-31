
namespace MeterDataLib.Parsers
{
    public class MimeTypeHelper
    {
        private static readonly Dictionary<string, string> MimeTypes = new(StringComparer.InvariantCultureIgnoreCase)
        {
        { ".zip", "application/zip" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        { ".csv", "text/csv" }
        };

        public static string GetMimeType(string fileName)
        {
            string extension = System.IO.Path.GetExtension(fileName);
            if (extension != null && MimeTypes.TryGetValue(extension, out string? mimeType))
            {
                return mimeType;
            }
            return "application/octet-stream"; // Default MIME type
        }
    }


}
