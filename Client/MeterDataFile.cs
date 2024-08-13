using MeterDataLib.Parsers;
using Microsoft.AspNetCore.Components.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MeterKloud
{
    internal class MeterDataFile
    {
        private IBrowserFile _browserFile;
        const long MAXALLOWEDSIZE = 1000 * 1000 * 1024;
        public MeterDataFile(IBrowserFile file)
        {
            FileName = file.Name;
            FileSize = file.Size;
            FileType = file.ContentType;
            _browserFile = file;
        }

        public Stream OpenStream(CancellationToken cancellationToken)
        {
            return _browserFile.OpenReadStream(MAXALLOWEDSIZE, cancellationToken);
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


        public async Task Parse( Func<Task>?  asyncCallBack = null  , CancellationToken? cancellationToken = null )
        {
            InProgress = true;
            try
            {
                cancellationToken ??= new CancellationToken();
                using var stream = OpenStream(cancellationToken.Value);
                ParserResult = await MeterDataLib.Parsers.ParserFactory.ParseAsync(stream, FileName, FileType, (r) => UpdateProgress(r, asyncCallBack) , cancellationToken);
                
                
            }
            catch (Exception ex)
            {
                ParserResult = new ParserResult() { InProgress = false, FileName = FileName, LogMessages = new List<FileLogMessage>() { new FileLogMessage(ex.Message, LogLevel.Critical, FileName, 0, 0) } };
            }
            finally
            {
                InProgress = false;
                Parsed = true; 
            }


        }


        async Task UpdateProgress (ParserResult result , Func<Task>? asyncCallBack = null )
        {
      
            this.ParserResult = result;
            if (asyncCallBack != null)
            {
                await asyncCallBack();
            }
        }

    

        static string GetFileSize(long length)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = (double)length;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            string result = string.Format("{0:0.##} {1}", len, sizes[order]);
            return result;
        }









    }

}
