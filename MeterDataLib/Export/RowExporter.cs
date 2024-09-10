using System.Text;
using System.Xml;

namespace MeterDataLib.Export
{
    internal static class RowExporter
    {

        public static async Task ExportRowCSV(ExportOptions options, StringBuilder writer, CancellationToken? cancellationToken)
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
                writer.Append(",Channel,Quality");
            }

            var intervalMinutes = options.IntervalInMinutes ??  options.SiteDays.SelectMany(x => x.Channels.Values).Min(y => y.IntervalMinutes);
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
            for (int i = 1; i <= intervals; i++)
            {
                writer.Append($",P{i}");
            }
            writer.AppendLine();

            foreach (var item in grouped)
            {


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

                if (options.IncludeSite)
                {
                    writer.Append($"{item.Key.SiteCode},");
                }
                var readDate = item.Key.Date;
                var quality =  item.First().cd.OverallQuality;

                writer.Append($"{readDate:yyyy-MM-dd}");
                if (options.IncludeMeter)
                {
                    writer.Append($",{item.First().cd.MeterId}");
                }
                writer.Append($",{item.Key.Channel},{quality.ToShortString()}");
                for (int i = 0; i < intervals; i++)
                {
                    
                    var value = readings[i];
                    writer.Append($",{value}");
                }
                writer.AppendLine();
                await Task.Yield(); cancellationToken?.ThrowIfCancellationRequested();
            }

        }
    }
}