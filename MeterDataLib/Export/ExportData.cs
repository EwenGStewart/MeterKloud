using System.Text;

namespace MeterDataLib.Export
{
    public static class ExportData
    {
        const int MaxDays = 366 * 4;
        private static readonly ChanelType[] StandardChannelTypes = new ChanelType[] { ChanelType.ActivePowerConsumption, ChanelType.ActivePowerGeneration, ChanelType.ReactivePowerConsumption, ChanelType.ReactivePowerGeneration };




        public static string Export(ExportOptions options)
        {


            if (options.Site != null)
            {
                options.Sites.Add(options.Site);
                options.Site = null;
            }

            if (options.Sites.Count() == 0)
            {
                var codes = options.SiteDays.Where(x => !string.IsNullOrWhiteSpace(x.SiteCode)).Select(x => x.SiteCode.ToUpperInvariant().Trim()).Distinct().ToList();
                if (codes.Count() > 0)
                {
                    foreach (var code in codes)
                    {
                        var site = new Site() { Code = code };
                        if (site != null)
                        {
                            options.Sites.Add(site);
                        }
                    }
                }
            }


            if (options.Sites.Count() == 0)
            {
                throw new ArgumentException("No sites specified or found");
            }



            DateTime minDate = options.SiteDays.Min(x => x.Date);
            DateTime maxDate = options.SiteDays.Max(x => x.Date);
            if (options.FromDate == null)
            {
                options.FromDate = minDate;
            }
            if (options.ToDate == null)
            {
                options.ToDate = maxDate;
            }
            if (options.FromDate > maxDate || options.ToDate < minDate)
            {
                throw new ArgumentException("No data in date range");
            }


            if (options.FromDate > options.ToDate)
            {
                throw new ArgumentException("Invalid date range");
            }


            if (options.ToDate.Value.Subtract(options.FromDate.Value).TotalDays > MaxDays)
            {
                throw new ArgumentException($"Date range exceeds max of {MaxDays} days");
            }


            var siteCodes = options.Sites.Select(x => x.Code.Trim().ToUpper()).Distinct().ToList();
            var exportableDays = options.SiteDays.Where(x => x.Date >= options.FromDate.Value && x.Date <= options.ToDate.Value && siteCodes.Contains(x.SiteCode.Trim().ToUpperInvariant())).ToList();
            if (exportableDays.Count() == 0)
            {
                throw new ArgumentException("No data in date range for specified sites can be exported");
            }

            options.SiteDays = exportableDays;

            if (options.ChannelTypes.Count() == 0)
            {
                options.ChannelTypes = [.. StandardChannelTypes];
            }

            var writer = new StringBuilder();

            switch (options.ExportType)
            {
                case ExportFormat.NEM12:
                    Nem12Exporter.ExportNem12(options, writer);
                    break;
                case ExportFormat.QuadrantCSV:
                    ExportQuadrantCSV(options, writer);
                    break;
                case ExportFormat.ColumnarCSV:
                    ExportColumnarCSV(options, writer);
                    break;
                case ExportFormat.RowCSV:
                    ExportRowCSV(options, writer);
                    break;
                default:
                    throw new NotImplementedException();
            }


            string result = writer.ToString();
            return result;
        }

    
      


        static void ExportQuadrantCSV(ExportOptions options, StringBuilder writer)
        {
            throw new NotImplementedException();
        }

        static void ExportColumnarCSV(ExportOptions options, StringBuilder writer)
        {
            throw new NotImplementedException();
        }

        static void ExportRowCSV(ExportOptions options, StringBuilder writer)
        {

            throw new NotImplementedException();
        }



      

    }


}