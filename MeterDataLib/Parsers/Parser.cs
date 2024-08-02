using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeterDataLib.Parsers
{
    public  abstract class  Parser
    {
        public abstract bool CanParse(Stream stream, string filename, string? mimeType);
        public abstract ParserResult Parse(Stream stream , string filename );
        public abstract string Name { get; }

    }



}
