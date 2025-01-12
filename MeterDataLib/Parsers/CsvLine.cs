using Microsoft.VisualBasic;
using System.Globalization;

namespace MeterDataLib.Parsers
{
    public record CsvLine(string[] Columns, int LineNumber, bool Eof)
    {
        public int ColCount => Columns.Length;

        public override string ToString()
        {
            return  string.Join(',', Columns);
        }


        public int? GetIntCol(int col)
        {
            if (Columns.Length > col)
            {
                if (int.TryParse(Columns[col], out int result))
                {
                    return result;
                }
            }
            return null;
        }

        public int GetIntMandatory(int col , int minValue = int.MinValue , int maxValue = int.MaxValue)
        {
            var v = GetIntCol(col );
            if (v is null)
            {
                if (col > Columns.Length)
                    throw new ParserException($"Missing column {col} on Line {LineNumber}", string.Empty, LineNumber, col);
                else
                    throw new ParserException($"Invalid integer value {Columns[col]} at line {LineNumber}, col {col + 1}", Columns[col], LineNumber, col);

            }
            if (v.Value < minValue || v.Value > maxValue)
            {
                throw new ParserException($"Invalid integer value {Columns[col]} at line {LineNumber}, col {col + 1} - expected value between {minValue} and {maxValue}", Columns[col], LineNumber, col);
            }
            return v.Value;
        }


        public decimal? GetDecimalCol(int col)
        {
            if (Columns.Length > col)
            {
                if (decimal.TryParse(Columns[col], out decimal result))
                {
                    return result;
                }
            }
            return null;
        }

        public decimal GetDecimalMandatory(int col )
        {
            var v = GetDecimalCol(col);
            if (v is null)
            {
                if (col > Columns.Length)
                    throw new ParserException($"Missing column {col} on Line {LineNumber}", string.Empty, LineNumber, col);
                else
                    throw new ParserException($"Invalid decimal value {Columns[col]} at line {LineNumber}, col {col + 1}", Columns[col], LineNumber, col);

            }
            return v.Value;
        }



        public string GetStringUpper(int col)
        {
            if (Columns.Length > col)
            {
                return Columns[col].ToUpper().Replace('"',' ').Trim();
            }
            return string.Empty;
        }

        public string GetStringUpperMandatory(int col , int min=0 , int max = 1000)
        {
            var v = GetStringUpper(col);
            if ( string.IsNullOrEmpty(v))
            {
                if (col > Columns.Length)
                    throw new ParserException($"Missing column {col} on Line {LineNumber}", string.Empty, LineNumber, col);
                else
                    throw new ParserException($"Missing value {Columns[col]} at line {LineNumber}, col {col + 1}", Columns[col], LineNumber, col);

            }
            if (v.Length < min || v.Length > max)
                throw new ParserException($"Invalid value {Columns[col]} at line {LineNumber}, col {col + 1} - expected length between {min} and {max}", Columns[col], LineNumber, col);
            return v;
        }

        public string GetString(int col)
        {
            if (Columns.Length > col)
            {
                return Columns[col];
            }
            return string.Empty;
        }

        public DateTime? GetDate(int col, string format)
        {
            if (Columns.Length > col)
            {
                if (DateTime.TryParseExact(Columns[col], format, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime result))
                {
                    return result;
                }
            }
            return null;
        }

        public DateTime GetDateMandatory(int col, string format)
        {
            var v = GetDate(col, format);
            if (v is null)
            {
                if (col > Columns.Length)
                    throw new ParserException($"Missing column {col} on Line {LineNumber}", string.Empty, LineNumber, col);
                else
                    throw new ParserException($"Invalid date value {Columns[col]} at line {LineNumber}, col {col + 1} - expected format is {format}", Columns[col], LineNumber, col);

            }
            return v.Value;
        }


        public DateTime? GetDate(int col, string[] format)
        {
            if (Columns.Length > col)
            {
                if (DateTime.TryParseExact(Columns[col], format, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime result))
                {
                    return result;
                }
            }
            return null;
        }
        public DateTime GetDateMandatory(int col, string[] format)
        {
            var v = GetDate(col, format);
            if (v is null)
            {
                if (col > Columns.Length)
                    throw new ParserException($"Missing column {col} on Line {LineNumber}", string.Empty, LineNumber, col);
                else
                    throw new ParserException($"Invalid date value {Columns[col]} at line {LineNumber}, col {col + 1} - expected format is {string.Join('|',format)}", Columns[col], LineNumber, col);

            }
            return v.Value;
        }

        public TimeOnly? GetTime(int col, string format)
        {
            if (Columns.Length > col)
            {
                if (TimeOnly.TryParseExact(Columns[col], format, CultureInfo.CurrentCulture, DateTimeStyles.None, out TimeOnly result))
                {
                    return result;
                }
            }
            return null;
        }

        public TimeOnly GetTimeMandatory(int col, string format)
        {
            var time = GetTime(col, format);
            if (time is null )
            {
                if ( col > Columns.Length)
                    throw new ParserException($"Missing column {col} on Line {LineNumber}" , string.Empty , LineNumber , col  );
                else
                    throw new ParserException($"Invalid time value {Columns[col]} at line {LineNumber}, col {col+1}", Columns[col], LineNumber, col );
                
            }
            return time.Value;
        }




    }
        





}
