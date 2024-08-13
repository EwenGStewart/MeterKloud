namespace MeterDataLib.Parsers
{
    internal interface IParser
    {
        bool CanParse(List<CsvLine> lines);
        string Name { get; }
        Task Parse(SimpleCsvReader csvReader, ParserResult result, Func<ParserResult, Task>? callBack = null , CancellationToken? cancellationToken = null  );


        static string GetDefaultSiteCodeFromFilename(string filename)
        {


            var siteCode = new string(Path.GetFileNameWithoutExtension(filename).Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
            if (siteCode.Length > 0)
            {
                return siteCode;
            }
            return ParserResult.UNKNOWN; ;
        }

    }


}
