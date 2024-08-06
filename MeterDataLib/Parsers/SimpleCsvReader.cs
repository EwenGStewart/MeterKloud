using Microsoft.Win32.SafeHandles;
using System.IO;

namespace MeterDataLib.Parsers
{
    internal class SimpleCsvReader : IDisposable
    {
        readonly StreamReader _streamReader;
        bool _eof = false; 
        string _filename;
        private List<CsvLine> _buffer = new List<CsvLine>();

        public int LineNumber { get; private set; } = 0;
        public int ColNumber { get; private set; } = 0;

        public string CurrentLine { get; private set; } = string.Empty;
        public string[] CurrentCols { get; private set; } = Array.Empty<string>();
        
        public string Filename => _filename;
        
        public SimpleCsvReader(Stream stream, string filename)
        {
            _streamReader = new StreamReader(stream);
            _filename = filename;
            _eof = false;

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

        

        private async Task<CsvLine> InternalReadFromStreamAsync()
        {


            while (!_eof)
            {
                try
                {
                    var nextLine = await _streamReader.ReadLineAsync();
                    if (nextLine == null)
                    {
                        _eof = true;
                        return new CsvLine(Array.Empty<string>(), LineNumber, true);
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
                        CurrentCols = CurrentLine.Split(',', StringSplitOptions.TrimEntries);
                        ColNumber = CurrentCols.Length;
                        return new CsvLine(CurrentCols, LineNumber, false); ;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GRLPN9] Error reading file - {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                    return new CsvLine(Array.Empty<string>(), LineNumber, true);
                }
            }
            return new CsvLine(Array.Empty<string>(), LineNumber, true);
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
