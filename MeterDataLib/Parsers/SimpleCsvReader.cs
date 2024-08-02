using System.IO;

namespace MeterDataLib.Parsers
{
    internal class SimpleCsvReader : IDisposable
    {
        StreamReader _streamReader;
        string _filename;
        public int LineNumber { get; private set; } = 0;
        public int ColNumber { get; private set; } = 0;

        public string CurrentLine { get; private set; } = string.Empty;
        public string[] CurrentCols { get; private set; } = Array.Empty<string>();
        public SimpleCsvReader(Stream stream, string filename)
        {
            _streamReader = new StreamReader(stream);
            _filename = filename;

        }

        public CsvLine Read()
        {
            if (_streamReader.EndOfStream)
            {
                return new CsvLine(Array.Empty<string>(), 0, true);
            }
            while (_streamReader.EndOfStream == false)
            {

                var nextLine = _streamReader.ReadLine();
                if (nextLine == null) new CsvLine(Array.Empty<string>(), LineNumber, true);
                LineNumber++;


                if (string.IsNullOrWhiteSpace(nextLine) == false)
                {

                    CurrentLine = nextLine.Trim();
                    // detect if a line contains only [, ] characters skip it
                    if (CurrentLine.All(c => c == ',' || c == ' '))
                    {
                        continue;
                    }
                    CurrentCols = CurrentLine.Split(',', StringSplitOptions.TrimEntries);
                    ColNumber = CurrentCols.Length;
                    return new CsvLine(CurrentCols, LineNumber, false); ;
                }
            }
            return new CsvLine(Array.Empty<string>(), LineNumber, true);
        }

        public void Dispose()
        {
            if (_streamReader != null)
            {
                if (_streamReader.BaseStream.CanSeek)
                {
                    _streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
                }
            }
        }
    }

}
