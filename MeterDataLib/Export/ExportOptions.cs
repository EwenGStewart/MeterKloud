using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Numerics;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Transactions;

namespace MeterDataLib.Export
{
    public enum ExportFormat
    {
        NEM12,
        // standard NEM12 Format 
        QuadrantCSV,
        // CSV format with columns for each each quadrant plus interval / demand columns 
        ColumnarCSV,
        RowCSV,
    }





    public record  struct ExportOptions
    {
        public ExportOptions()
        {
        }

        public Site? Site { get; set; }
        public List<Site> Sites { get; set; } = [];

        public IEnumerable<SiteDay> SiteDays { get; set; } = [];

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }



        public ExportFormat ExportType { get; set; } = ExportFormat.NEM12;

        public int? IntervalInMinutes { get; set; }

        public bool IncludeHeader { get; set; } = true;

        public bool IncludeSite { get; set; } = true;
        public bool IncludeMeter { get; set; } = false;
        public bool IncludeChannel { get; set; } = false;
        public List<ChanelType> ChannelTypes { get; set; } = [];


    }


}