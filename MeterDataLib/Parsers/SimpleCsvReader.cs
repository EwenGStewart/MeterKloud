using Microsoft.Win32.SafeHandles;
using System.IO;

namespace MeterDataLib.Parsers
{
    internal class SimpleCsvReader : IDisposable
    {
        readonly StreamReader _streamReader;
        readonly CancellationToken _cancellationToken;
        bool _eof = false;
        readonly string _filename;
        private readonly List<CsvLine> _buffer = [];

        public int LineNumber { get; private set; } = 0;
        public int ColNumber { get; private set; } = 0;

        public string CurrentLine { get; private set; } = string.Empty;
        public string[] CurrentCols { get; private set; } = [];
        
        public string Filename => _filename;
        



        public SimpleCsvReader(Stream stream, string filename, CancellationToken? cancellationToken)
        {
            _streamReader = new StreamReader(stream);
            _filename = filename;
            _eof = false;
            _cancellationToken = cancellationToken ?? new CancellationToken(); 
        }

        public int PercentageCompleted()
        {
            long length = 0;
            long position = 0;
            switch (_streamReader.BaseStream)
            {
                case FileStream fs:
                    length = fs.Length;
                    position = fs.Position;
                    break;
                case MemoryStream ms:
                    length = ms.Length;
                    position = ms.Position;
                    break;
                case System.IO.Compression.DeflateStream sfh:
                    length = sfh.BaseStream.Length;
                    position = sfh.BaseStream.Position;
                    break;
            }


            if (length == 0)
            {
                return 0;
            }
            return (int)(position * 100 / length);
        }

        
        public void PushBack(CsvLine line)
        {
            _buffer.Insert(0, line);
        }


        public void PushBack(List<CsvLine> lines)
        {
            _buffer.InsertRange(0, lines);
        }




        public async Task<CsvLine> ReadBufferedAsync()
        {
            var line = await  InternalReadFromStreamAsync();
            if (line.Eof == false)
            {
                _buffer.Add(line);
            }
            return line;
        }

        public async Task<List<CsvLine>> ReadBufferedAsync(int count)
        {
            var result = new List<CsvLine>();
            for (int i = 0; i < count; i++)
            {
                var line = await ReadBufferedAsync();
                if (line.Eof)
                {
                    return result;
                }
                result.Add(line);
            }
            return result;
        }

        public async Task<CsvLine> ReadAsync()
        {
            if (_buffer.Count > 0)
            {
                var result = _buffer[0];
                _buffer.RemoveAt(0);
                return result;
            }

            return await InternalReadFromStreamAsync();
        }



        private static string[] ParseCsvLine(string line)
        {
            List<string> columns = new();
            bool inQuotes = false;
            int startIndex = 0;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    // Found a delimiter - extract the column
                    string col = line.Substring(startIndex, i - startIndex);
                    columns.Add(CleanColumn(col));
                    startIndex = i + 1;
                }
            }

            // Add the last column
            if (startIndex < line.Length)
            {
                string col = line.Substring(startIndex);
                columns.Add(CleanColumn(col));
            }
            else if (line.EndsWith(','))
            {
                // Empty column at the end
                columns.Add(string.Empty);
            }

            return columns.ToArray();
        }

        private static string CleanColumn(string column)
        {
            // Remove leading/trailing whitespace
            column = column.Trim();

            // Remove surrounding quotes if present
            if (column.StartsWith('"') && column.EndsWith('"') && column.Length >= 2)
            {
                column = column.Substring(1, column.Length - 2);
            }

            return column;
        }

        private async Task<CsvLine> InternalReadFromStreamAsync()
        {


            while (!_eof)
            {
                try
                {
                    _cancellationToken.ThrowIfCancellationRequested();
                    var nextLine = await _streamReader.ReadLineAsync(_cancellationToken);
                    if (nextLine == null)
                    {
                        _eof = true;
                        return new CsvLine([], LineNumber, true);
                    }
                    LineNumber++;
                    if (string.IsNullOrWhiteSpace(nextLine) == false)
                    {

                        CurrentLine = nextLine.Trim();
                        // detect if a line contains only [, ] characters skip it
                        if (CurrentLine.All(c => c == ',' || c == ' '))
                        {
                            continue;
                        }

                        // remove the trailing ',' or ' ' characters
                        while (CurrentLine.Length > 0 && ( CurrentLine.EndsWith(',') || CurrentLine.EndsWith(' ') ))
                        {
                            CurrentLine = CurrentLine.Substring(0, CurrentLine.Length - 1);
                        }


                        CurrentCols = ParseCsvLine(CurrentLine);
                        ColNumber = CurrentCols.Length;
                        return new CsvLine(CurrentCols, LineNumber, false); ;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GRLPN9] Error reading file - {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                    return new CsvLine([], LineNumber, true);
                }
            }
            return new CsvLine([], LineNumber, true);
        }




        public async Task<List<CsvLine>> ReadAsync(int count)
        {
            var result = new List<CsvLine>();
            for (int i = 0; i < count; i++)
            {
                var line = await ReadAsync();
                if (line.Eof)
                {
                    return result;
                }
                result.Add(line);
            }
            return result;
        }




        public void Dispose()
        {
            if (_streamReader != null)
            {
                _streamReader.Dispose();
              
            }
        }
    }

}
