using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MeterDataLib.Parsers
{
    //public abstract class Parser
    //{

    //    public virtual bool CsvBased => false;
    //    public abstract bool CanParseType(string mimeType);

    //    public abstract bool CanParse(Stream stream, string filename, string? mimeType);

    //    protected abstract bool CanParseInternal(Stream stream, string filename, string? mimeType);
    //    public abstract ParserResult Parse(Stream stream , string filename );
    //    public abstract string Name { get; }

    //}


    //public abstract class CsvParser : Parser
    //{
    //    protected internal SimpleCsvReader? SimpleCsvReader = null;
    //    public override bool CsvBased => true;

    //    internal void OpenCsvParser(Stream stream, string filename)
    //    {
    //        SimpleCsvReader = new SimpleCsvReader(stream, filename);
    //    }

    //    public override bool CanParseType(string mimeType)
    //    {
    //        return CsvParserLib.ValidateMime(mimeType);
    //    }

    //    public  override bool CanParse(Stream stream, string filename, string? mimeType)
    //    {

    //        mimeType??= string.Empty;
    //        mimeType = mimeType.ToLowerInvariant();
    //        if (string.IsNullOrEmpty(mimeType))
    //        {
    //            mimeType =  "text/csv";
    //        }
    //        if ( ! CanParseType(mimeType))
    //        {
    //            return false;
    //        }


            



    //    protected abstract bool VerifyHeader(List<CsvLine> header);








    //}




}
