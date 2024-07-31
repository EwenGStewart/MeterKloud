namespace MeterDataLib.Parsers
{
    public static class StreamExtensions
    {
       public static string ReadNonBlankLine(this StreamReader sr)
       {
            while ( sr.EndOfStream == false)
            {
                var line = sr.ReadLine();
                if (string.IsNullOrWhiteSpace(line) == false)
                {

                    line = line.Trim();
                    // remove all spaces from the line
                    line = line.Replace(" ", "");
                    // detect if a line contains only , characters skip it
                    if (line.All(c => c == ','))
                    {
                        continue;
                    }
                    return line;
                }
            }
            return string.Empty;
       }

       public static bool  IsCsvLine( this string line)
       {
           return line.Contains(",");
       }
 



    }



}
