using System.Text;

namespace MeterDataLib.Export
{
    public static class ExportData
    {
        const int MaxDays = 366 * 4;
        private static readonly ChanelType[] StandardChannelTypes = new ChanelType[] { ChanelType.ActiveEnergyConsumption, ChanelType.ActiveEnergyGeneration, ChanelType.ReactiveEnergyConsumption, ChanelType.ReactiveEnergyGeneration };




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
                    QuadrantExporter.ExportQuadrantCSV(options, writer);
                    break;
                case ExportFormat.ColumnarCSV:
                     ColumnExporter.ExportColumnCSV(options, writer);
                    break;
                case ExportFormat.RowCSV:
                    RowExporter.ExportRowCSV(options, writer);
                    break;
                default:
                    throw new NotImplementedException();
            }


            string result = writer.ToString();
            return result;
        }

        public static string Site(SiteDay siteDay, ExportOptions options)
        {
            if (options.IncludeSite)

                if (string.IsNullOrEmpty(siteDay.SiteCode))
                {
                    return "UNKNOWN";
                }
                else
                {
                    return siteDay.SiteCode.Trim().ToUpperInvariant();
                }
            else
            {
                return "ALL";
            }
        }

        public static string Serial(ChannelDay channelDay, ExportOptions options)
        {
            if (options.IncludeMeter)

                if (string.IsNullOrEmpty(channelDay.MeterId))
                {
                    return "UNKNOWN";
                }
                else
                {
                    return channelDay.MeterId.Trim().ToUpperInvariant();
                }
            else
            {
                return "ALL";
            }
        }

        public static string ChannelNum(ChannelDay channelDay, ExportOptions options)
        {
            if (options.IncludeChannel)

                if (string.IsNullOrEmpty(channelDay.ChannelNumber))
                {
                    return "?";
                }
                else
                {
                    return channelDay.ChannelNumber.Trim().ToUpperInvariant();
                }
            else
            {
                return "*";
            }
        }

 


      

    }


}