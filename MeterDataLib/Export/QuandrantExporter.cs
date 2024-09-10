using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MeterDataLib.Export
{
    internal static class QuadrantExporter
    {

        public static async Task ExportQuadrantCSV(ExportOptions options, StringBuilder writer, CancellationToken? cancellationToken)
        {

            var grouped = options.SiteDays
                .SelectMany(x => x.Channels.Values, (sd, cd) => new { sd, cd })
                .GroupBy(x => new { x.sd.SiteCode, x.sd.Date, ChannelNum = ExportData.ChannelNum(x.cd, options), Serial = ExportData.Serial(x.cd, options) })
                .OrderBy(x => x.Key.SiteCode)
                .ThenBy(x => x.Key.Date)
                .ThenBy(x => x.Key.ChannelNum)
                .ThenBy(x => x.Key.Serial)
                .Where(x => x.Key.Date >= options.FromDate && x.Key.Date <= options.ToDate)
                .Where(x => x.Any(y => y.cd.Ignore == false && options.ChannelTypes.Contains(y.cd.ChannelType) && y.cd.IntervalMinutes > 0 && y.cd.Readings.Length != 0))
                .ToList();

            await Task.Yield(); cancellationToken?.ThrowIfCancellationRequested();

            if (options.IncludeHeader)
            {
                writer.AppendLine("Site,Date,Channel,Serial,Interval, Consumed kWh,Generated kWh, Consumed kVArh,Generated kVArh,kW,kVA,pf,Quality");
            }

            foreach (var item in grouped)
            {

                var channels = item.Select(x => x.cd).Where(x => x.Ignore == false && options.ChannelTypes.Contains(x.ChannelType) && x.IntervalMinutes > 0 && x.Readings.Length != 0);
                var intervalMinutes = options.IntervalInMinutes ?? channels.Min(y => y.IntervalMinutes);
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
                EnergyQuadrant[] quadrants = new EnergyQuadrant[intervals];
                for (int i = 0; i < intervals; i++)
                {
                    quadrants[i] = new EnergyQuadrant() { IntervalMinutes = intervalMinutes, ReadingDateTime = item.Key.Date.AddMinutes(i * intervalMinutes), ChannelNumber = item.Key.ChannelNum, Meter = item.Key.Serial, Quality = Quality.Unknown };

                }
                foreach (var channel in channels.GroupBy(x => new { x.Channel, x.IntervalMinutes, x.ChannelType }))
                {
                    var readings = channel.First().Readings;

                    var qualities = channel.First().QualityArray();
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
                        switch (channel.Key.ChannelType)
                        {
                            case ChanelType.ActiveEnergyConsumption:
                                quadrants[i].ActiveEnergyConsumption_kWh += readings[i];
                                if ((int)quadrants[i].Quality < (int)(qualities[i]))
                                {
                                    quadrants[i].Quality = qualities[i];
                                }
                                break;
                            case ChanelType.ActiveEnergyGeneration:
                                quadrants[i].ActiveEnergyGeneration_kWh += readings[i];
                                if ((int)quadrants[i].Quality < (int)(qualities[i]))
                                {
                                    quadrants[i].Quality = qualities[i];
                                }
                                break;
                            case ChanelType.ReactiveEnergyConsumption:
                                quadrants[i].ReactiveEnergyConsumption_kVArh += readings[i];
                                break;
                            case ChanelType.ReactiveEnergyGeneration:
                                quadrants[i].ReactiveEnergyGeneration_kVArh += readings[i];
                                break;
                        }
                    }
                }
                for (int i = 0; i < intervals; i++)
                {
                    var quad = quadrants[i];
                    //Site,Date,Channel,Serial,Interval, Consumed kWh,Generated kWh, Consumed kVArh,Generated kVArh,kW,kVA,pf,Quality; 
                    writer.AppendLine($"{item.Key.SiteCode},{quad.ReadingDateTime:yyyy-MM-dd HH:mm:ss},{item.Key.ChannelNum},{item.Key.Serial},{quad.IntervalMinutes},{quad.ActiveEnergyConsumption_kWh:0.000},{quad.ActiveEnergyGeneration_kWh:0.000},{quad.ReactiveEnergyConsumption_kVArh:0.000},{quad.ReactiveEnergyGeneration_kVArh:0.000},{quad.RealPowerConsumption_kW:0.000},{quad.ApparentPower_kVA:0.000},{quad.PowerFactor:0.000},{quad.Quality.ToShortString()}");
                    await Task.Yield(); cancellationToken?.ThrowIfCancellationRequested();
                }
            }
        }

    }
}