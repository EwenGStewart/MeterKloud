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

        public Stream OpenStream()
        {
            return _browserFile.OpenReadStream(MAXALLOWEDSIZE);
        }



        public string FileName { get; init; }
        public long FileSize { get; init; }
        public string FileSizeFormatted => GetFileSize(FileSize);
        public string FileType { get; init; }
        public string ProcessingStatus { get; private set; } = "Pending";

        public int? Errors { get; private set; }
        public int? Sites { get; private set; }
        public int? Days { get; private set; }
        public int? DataPoints { get; private set; }

        public int PercentageCompleted { get; private set; }

        public async Task Parse( Func<Task>?  asyncCallBack = null )
        {
            ProcessingStatus = "Opening..";
            using var stream = OpenStream();
            ProcessingStatus = "parsing..";
            var result = await MeterDataLib.Parsers.ParserFactory.ParseAsync(stream, FileName, FileType , (r)=> UpdateProgress(r, asyncCallBack) );
            Errors = result.Errors;
            Sites = result.Sites;
            Days = result.SitesDays.Count;
            DataPoints = result.TotalDataPoints;
            if (result.Errors > 0)
            {
                ProcessingStatus = $"{result.SitesDays.Count} days -  {result.Errors} errors";
                foreach (var error in result.LogMessages)
                {
                    Console.WriteLine(error);
                }


            }
            else
            {
                ProcessingStatus = $"{result.SitesDays.Count} days";
            }
            

        }


        async Task UpdateProgress (ParserResult result , Func<Task>? asyncCallBack = null )
        {
            PercentageCompleted = result.PercentageCompleted;
            ProcessingStatus = $"parsing {PercentageCompleted}%";

            Console.WriteLine(ProcessingStatus);
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
