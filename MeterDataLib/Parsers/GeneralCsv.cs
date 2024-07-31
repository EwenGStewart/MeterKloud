using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Schema;

namespace MeterDataLib.Parsers
{
    internal class GeneralCsv : IParser
    {
        readonly CsvDefinition _definition;
        public GeneralCsv(CsvDefinition definition)
        {
            _definition = definition;
        }

        private static readonly string[] allowedMimeTypes = new string[] { "text/plain", "text/csv", "" };
        public static IParser? GetParser(Stream stream, string filename, string? mimeType)
        {
            mimeType ??= "";
            mimeType = mimeType.ToLower();
            if (!allowedMimeTypes.Contains(mimeType))
            {
                return null;
            }

            // GRAB THE FIRST 10 LINES OF THE FILE
            List<CsvLine> lines = new List<CsvLine>();
            stream.Seek(0, SeekOrigin.Begin);
            using var csvReader = new SimpleCsvReader(stream, filename);
            for (int i = 0; i < 10; i++)
            {
                CsvLine line = csvReader.Read();
                if ( line.Eof )  break;
                lines.Add(line);
            }
            if ( lines.Count == 0 ) return null;

            foreach (var definition  in CsvDefinitions)
            {
                if( definition.MatchesWith(lines))
                {
                    return new GeneralCsv(definition);
                }


            }


        }



        public ParserResult Parse(Stream stream, string filename)
        {
            throw new NotImplementedException();
        }


        public static List<CsvDefinition> CsvDefinitions = new List<CsvDefinition>()
        {
            KnownCsvFormats.ChannelDay
        };


    }


    public static class KnownCsvFormats
    {
        public static CsvDefinition ChannelDay = new()
        {
            Name = "MultiLine1",
            Records = new List<CsvRecordRule>
            {
                new() {
                    RecTypeName = "NMI",
                    Rules = new List<CsvColumnRule>
                    {
                        new() { Column = 0, CellType = CellType.StringConstant , Value="NMI"},
                        new() { Column = 1, CellType = CellType.SiteId, Min=1, Max=30    }
                    },
                    ChildBlocks = new List<CsvRecordBlockRule>
                    {
                        new() { RecTypeName = "Meter", MinInstances = 1, MaxInstances = null }
                    }
                },
                new() {
                    RecTypeName = "Meter",
                    Rules = new List<CsvColumnRule>
                    {
                        new() { Column = 0, CellType = CellType.StringConstant , Value="Stream ID"},
                        new() { Column = 1, CellType = CellType.MeterSerial, Min=1, Max=50 , IgnoreWord="Meter"    },
                        new() { Column = 3, CellType = CellType.Channel, Min=1, Max=2 ,  AemoChannelFLag=true    },
                        new() { Column = 4, CellType = CellType.Uom, Min=1, Max=50   },
                        new() { Column = 5, CellType = CellType.EquipmentType, Min=1, Max=50 , Map= new Dictionary<string, string>(){{"Consumption" , "Mains" }}}
                    },
                    ChildBlocks = new List<CsvRecordBlockRule>
                    {
                        new() { RecTypeName = "TimeZone", MinInstances = 1, MaxInstances = 1 },
                        new() { RecTypeName = "Period", MinInstances = 1, MaxInstances = 1 },
                        new() { RecTypeName = "Data", MinInstances = 1, MaxInstances = null },
                        new() { RecTypeName = "TotalForPeriod", MinInstances = 1, MaxInstances = 1 }
                    }
                },
                new() {
                    RecTypeName = "TimeZone",
                    Rules = new List<CsvColumnRule>
                    {
                        new() { Column = 0, CellType = CellType.StringConstant , Value="LOCAL TIME"},
                        new() { Column = 1, CellType = CellType.TimeZoneName, Min=1, Max=50 }
                    }
                },
                new() {
                    RecTypeName = "Period",
                    Rules = new List<CsvColumnRule>
                    {
                        new() { Column = 0, CellType = CellType.StringConstant , Value="Date/Time"    },
                        new() { Column = 1,RepeatForEachPeriod = true ,  CellType = CellType.PeriodStartTime, Format="h:mm"},
                        new() { Column = 1, CellType = CellType.StringConstant, Value="Quality"  } 
                    }
                },
                new() {
                    RecTypeName = "Data",
                    Rules = new List<CsvColumnRule>
                    {
                        new() { Column = 0, CellType = CellType.ReadDay , Format="yyyyMMdd"    },
                        new() { Column = 1, RepeatForEachPeriod = true , CellType = CellType.ReadValue,  } , 
                        new() { Column = 2, CellType = CellType.Quality, Min=1 , Max=3  },
                    }
                }

                new() {
                    RecTypeName = "TotalForPeriod",
                    Rules = new List<CsvColumnRule>
                    {
                        new() { Column = 0, CellType = CellType.StringConstant  , Value="Total for Period"   },
                    }
                }

            },
            ChildBlocks = new List<CsvRecordBlockRule>
            {
                new() { RecTypeName = "NMI", MinInstances = 1, MaxInstances = null }
            }
        };
    }



    public class  CsvDefinition
    {

        public string Name { get; set; } = string.Empty;    
        public List<CsvRecordRule> Records { get; set; } = new List<CsvRecordRule>();
        
        public List<CsvRecordBlockRule> ChildBlocks { get; set; } = new List<CsvRecordBlockRule>();


        bool MatchesWith( List<CsvLine> lines )
        {
            if ( lines.Count < 1 ) return false;
            List<string> recordType = new List<string>();
            foreach (var line in lines)
            {
                CsvRecordRule? rule = null; 
                foreach (var record in Records)
                {
                    if( line.ColCount < record.MinCols() ) continue;
                    int offset = 0; 
                    bool doesNotMatch = false;
                    foreach( var  colRule in record.Rules  )
                    {
                        int fromCol = colRule.Column + offset;
                        int toCol = colRule.Column + offset;
                        if ( colRule.RepeatForEachPeriod )
                        {
                            if (line.ColCount > 288)
                            {
                                toCol = colRule.Column + offset + 288 -1 ;
                                offset += 288;
                            }
                            else if (line.ColCount > 96)
                            {
                                toCol = colRule.Column + offset + 96 -1 ;
                                offset += 96;
                            }
                            else if (line.ColCount > 48 ) 
                            {
                                toCol = colRule.Column + offset + 24- 1;
                                offset += 48;
                            }

                            else if (line.ColCount > 24 ) 
                            {
                                toCol = colRule.Column + offset + 24 -1 ;
                                offset += 24;
                            }
                            else
                            {
                                doesNotMatch = true;
                                break;
                            }

                        }
                        for (int col = fromCol; col <= toCol; col++)
                        {
                            switch (colRule.CellType)
                            {
                                case CellType.StringConstant:
                                    if (line.GetStringUpper(offset + colRule.Column) != colRule.Value) doesNotMatch = true;
                                    break;
                                case CellType.ReadValue:
                                case CellType.Decimal:
                                    if (line.GetDecimalCol(colRule.Column) == null) doesNotMatch = true;
                                    break;
                                case CellType.DateTime:
                                case CellType.ReadDay:
                                    {
                                        string format = colRule.Format ?? "yyyyMMdd";
                                        if (line.GetDate(colRule.Column, format) == null) doesNotMatch = true;
                                    }
                                    break;
                                case CellType.PeriodStartTime:
                                    {
                                        string format = colRule.Format ?? "h:mm";
                                        if (line.GetDate(colRule.Column, format) == null) doesNotMatch = true;
                                    }
                                    break;
                            }
                        }
                    }
                    
                }

            }
        }


    }


    public class CsvColumnRule
    {
        public int line { get; set; } = 0; 
        public int Column { get; set; } = 0;

        public bool RepeatForEachPeriod { get; set; } = false;
         
        public CellType CellType { get; set; } = CellType.StringConstant;

        public string? Value { get; set; } = null;

        public string? Format { get; set; } = null;

        public int? Min { get; set; } = null;

        public int? Max { get; set; } = null;

        public string IgnoreWord { get; set; } = string.Empty;

        public bool AemoChannelFLag { get; set; } = false;

        public Dictionary<string, string> Map  { get; set; } = new Dictionary<string, string>();

    }


    public class CsvRecordRule 
    {
        public string RecTypeName { get; set; } = string.Empty;
        public List<CsvColumnRule> Rules { get; set; } = new List<CsvColumnRule>();

        public List<CsvRecordBlockRule> ChildBlocks { get; set; } = new List<CsvRecordBlockRule>();


        public int MinCols()
        {
            int offset = 0;
            int minCol = 0;
            foreach (var rule in Rules)
            {
                if (rule.Column + 1 < minCol) { minCol = rule.Column + 1; }
                if (rule.RepeatForEachPeriod) { offset = 24 + rule.Column; minCol = 0; }
            }
            return minCol + offset;
        }
    }

    public class CsvRecordBlockRule 
    {
        public string RecTypeName { get; set; } = string.Empty;
        public int MinInstances { get; set; } = 0;
        public int?MaxInstances { get; set; }
        

    }


    public enum CellType 
    {
        StringConstant = 0,
        SiteId ,
        MeterSerial ,
        RegisterId , 
        Channel ,
        Uom, 
        DateTime,
        Decimal ,
        EquipmentType ,
        TimeZoneName , 
        ReadDay , 
        PeriodStartTime ,
        Quality, 
        ReadValue, 

    }




}
