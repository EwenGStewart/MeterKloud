using MeterDataLib.Parsers;
using Microsoft.AspNetCore.Components.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MeterKloud
{
    internal class MeterDataFile
    {
        private readonly IBrowserFile _browserFile;
        const long MAXALLOWEDSIZE = 1000 * 1000 * 1024;
        public MeterDataFile(IBrowserFile file)
        {
            FileName = file.Name;
            FileSize = file.Size;
            FileType = file.ContentType;
            _browserFile = file;
        }


        public string FileName { get; init; }
        public long FileSize { get; init; }
        public string FileSizeFormatted => GetFileSize(FileSize);
        public string FileType { get; init; }

        public string Format => ParserResult?.ParserName ?? "Unknown";
        public string ProcessingStatus => ParserResult?.Progress ?? "Pending"; 

        public int Errors => ParserResult?.Errors ?? 0;

        public int Warnings => ParserResult?.Warnings ?? 0;

        public int Sites => ParserResult?.Sites ?? 0;


        public string SiteName => ParserResult?.SiteName() ?? string.Empty;

        public int Days => ParserResult?.SitesDays.Count ?? 0 ;
        public int DataPoints => ParserResult?.TotalDataPoints ?? 0;

        public bool InProgress { get; private set; } = false;

        public bool Parsed { get; private set; } = false;
        public bool Success => ParserResult!=null &&  Errors == 0 && Sites >0 ;

        public bool HasUnknownSites => ParserResult?.UnknownSites ?? false;


        public ParserResult? ParserResult { get; private set; }


        public void AddException(Exception exception)
        {
            ParserResult ??= new ParserResult() { FileName = FileName };
            ParserResult.AddException(exception);
        }
        public void AddException(string exception) 
        {
            ParserResult ??= new ParserResult() { FileName = FileName };
            ParserResult.AddException(exception);

        }


        public async Task Parse( Func<ParserResult,Task> asyncCallBack , CancellationToken cancellationToken , MeterKloudClientApi api  )        
        {
            InProgress = true;
            try
            {
                ParserResult = new ParserResult() { InProgress = true, FileName = FileName };
                ParserResult = await api.UploadStream(_browserFile, asyncCallBack, cancellationToken, ParserResult);
                
            }
            catch (Exception ex)
            {
                ParserResult = new ParserResult() { InProgress = false, FileName = FileName, LogMessages = [new FileLogMessage(ex.Message, LogLevel.Critical, FileName, 0, 0)] };
            }
            finally
            {
                InProgress = false;
                Parsed = true; 
            }


        }


 

    

        static string GetFileSize(long length)
        {
            string[] sizes = ["B", "KB", "MB", "GB", "TB"];
            double len = (double)length;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            string result = string.Format("{0:0.##} {1}", len, sizes[order]);
            return result;
        }









    }

}
