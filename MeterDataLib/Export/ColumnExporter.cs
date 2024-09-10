using System.Text;

namespace MeterDataLib.Export
{
    internal static class ColumnExporter
    {

        public static async Task ExportColumnCSV(ExportOptions options, StringBuilder writer , CancellationToken? cancellationToken)
        {

            var grouped = options.SiteDays
                .SelectMany(x => x.Channels.Values, (sd, cd) => new { sd, cd })
                .GroupBy(x => new {  SiteCode= ExportData.Site( x.sd , options)  , x.sd.Date, x.cd.ChannelNumber, x.cd.Channel, x.cd.IntervalMinutes, x.cd.ChannelType })
                .OrderBy(x => x.Key.SiteCode)
                .ThenBy(x => x.Key.Date)
                .ThenBy(x => x.Key.ChannelNumber)
                .ThenBy(x => x.Key.Channel)
                .Where(x => x.Key.Date >= options.FromDate && x.Key.Date <= options.ToDate)
                .ToList();

            await Task.Yield(); cancellationToken?.ThrowIfCancellationRequested();

            if (options.IncludeHeader)
            {
                if (options.IncludeSite) { writer.Append("Site,"); }
                writer.Append("Date");
                if (options.IncludeMeter) { writer.Append(",Meter"); }
                writer.AppendLine(",Channel,Value,Quality");
            }

            foreach (var item in grouped)
            {


                var intervalMinutes = options.IntervalInMinutes ?? item.Key.IntervalMinutes;
                if (intervalMinutes < 1 || intervalMinutes > 1440)
                {
                    intervalMinutes = 30;
                }
                if (1440 % intervalMinutes != 0)
                {
                    if (intervalMinutes < 5)
                    {
                        intervalMinutes = 5;
                    }
                    else if (intervalMinutes < 15)
                    {
                        intervalMinutes = 15;
                    }
                    else if (intervalMinutes < 30)
                    {
                        intervalMinutes = 30;
                    }
                    else if (intervalMinutes < 60)
                    {
                        intervalMinutes = 60;
                    }
                }

                int intervals = 1440 / intervalMinutes;

                var readings = item.First().cd.Readings;

                var qualities = item.First().cd.QualityArray();
                if (readings.Length == 0)
                {
                    continue;
                }
                if (readings.Length != intervals)
                {
                    readings = ProfileHelpers.UnifyLength(readings, intervals, true);
                    qualities = ProfileHelpers.UnifyQuality(qualities, intervals);
                }


                for (int i = 0; i < intervals; i++)
                {
                    var readDate = item.Key.Date.AddMinutes(i * intervalMinutes);
                    var value = readings[i];
                    var quality = qualities[i];
                    if (options.IncludeSite)
                    {
                        writer.Append($"{item.Key.SiteCode},");
                    }

                    writer.Append($"{readDate:yyyy-MM-dd HH:mm:ss}");
                    if (options.IncludeMeter)
                    {
                        writer.Append($",{item.First().cd.MeterId}");
                    }
                    writer.AppendLine($",{item.Key.Channel},{value},{quality.ToShortString()}");
                    await Task.Yield(); cancellationToken?.ThrowIfCancellationRequested();
                }
            }

        }
    }
}