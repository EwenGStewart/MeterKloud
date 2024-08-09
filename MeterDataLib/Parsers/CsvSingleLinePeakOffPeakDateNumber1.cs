using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MeterDataLib.Parsers
{


    public class CsvSingleLinePeakOffPeakDateNumber : IParser
    {
        public   string Name => "SingleLineWith_PK_OP_DateNumber";

        public bool CanParse(List<CsvLine> lines)
        {
 
            if (lines.Count < 2) return false;

            // CHECK HEADER ROW
            if (
                    lines[0].ColCount == 11
                && lines[1].ColCount == 11
                && lines[0].GetStringUpper(0) == "NMI"
                && lines[0].GetStringUpper(1) == "DATE"
                && lines[0].GetStringUpper(2) == "TIME"
                && lines[0].GetStringUpper(3) == "KWH"
                && lines[0].GetStringUpper(4) == "KVARH"
                && lines[0].GetStringUpper(5) == "KW"
                && lines[0].GetStringUpper(6) == "KVA"
                && lines[0].GetStringUpper(7) == "PF"
                && lines[0].GetStringUpper(8) == "PEAK_KWH"
                && lines[0].GetStringUpper(9) == "OFF_PEAK_KWH"
                && lines[0].GetStringUpper(10) == "DATENUMBER"
                && lines[1].GetDate(1, new string[] { "d/M/yyyy", "d/MM/yyyy" }) != null
                && lines[1].GetTime(2, "h:mm:ss tt") != null
                )
                return true;
            return false;
        }
        async Task IParser.Parse(SimpleCsvReader reader, ParserResult result, Func<ParserResult, Task>? callBack)
        {
            string filename = reader.Filename;
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            var records = new List<DataLine>();
            try
            {
                {
                    CsvLine line;
                    // skip the first line 
                    await reader.ReadAsync();



                    while ((line = await reader.ReadAsync()).Eof == false)
                    {
                        try
                        {
                            if (timer.ElapsedMilliseconds > 100)
                            {
                                result.Progress = $"reading line {line.LineNumber}";
                                timer.Restart();
                                if (callBack != null)
                                {
                                    await callBack(result);
                                }
                            }

                            string nmi = line.GetStringUpperMandatory(0, 10, 11);
                            DateTime readDate = line.GetDateMandatory(1, new string[] { "d/M/yyyy", "d/MM/yyyy" });
                            var readTime = line.GetTimeMandatory(2, "h:mm:ss tt");
                            decimal kwh = line.GetDecimalMandatory(3);
                            decimal kvarh = line.GetDecimalMandatory(4);
                            decimal kw = line.GetDecimalMandatory(5);
                            decimal kva = line.GetDecimalMandatory(6);
                            decimal pf = line.GetDecimalCol(7) ?? 0;
                            decimal? peakKwh = line.GetDecimalCol(8);
                            decimal? offPeakKwh = line.GetDecimalCol(9);
                            int dateNumber = line.GetIntCol(10) ?? 0;
                            var record = new DataLine(nmi, readDate, readTime, kwh, kvarh, kw, kva, pf, peakKwh, offPeakKwh, dateNumber, reader.LineNumber);
                            records.Add(record);

                        }
                        catch (ParserException ex)
                        {
                            result.LogMessages.Add(new FileLogMessage(ex.Message, Microsoft.Extensions.Logging.LogLevel.Error, filename, reader.LineNumber, ex.Col));
                            if (result.Errors > 100)
                            {
                                throw new Exception("Too many errors");
                            }
                        }
                        catch (Exception ex)
                        {
                            result.LogMessages.Add(new FileLogMessage(ex.Message, Microsoft.Extensions.Logging.LogLevel.Critical, filename, reader.LineNumber, 0));
                            if (result.Errors > 100)
                            {
                                throw new Exception("Too many errors");
                            }
                        }
                    }
                }

                int[] allowedIntervals = new int[] { 1, 5, 15, 30, 60 };
                var list = records.GroupBy(x => new { Nmi = x.Nmi, ReadDate = x.EndDateTimeInclusive.Date })
                                            .Where(x => x.Count() > 1)
                                            .OrderBy(x => x.Key.Nmi)
                                            .ThenBy(x => x.Key.ReadDate).ToList();
                int counterTotal = list.Count;
                int counter = 0;

                foreach (var siteDayGroup in list )
                {
                    counter++;
                    if (timer.ElapsedMilliseconds > 100)
                    {
                        int percentage = counter * 100 / counterTotal;
                        result.Progress = $"parsing {percentage}%";
                        timer.Restart();
                        if (callBack != null)
                        {
                            await callBack(result);
                        }
                    }

                    var siteDay = new SiteDay()
                    {
                        SiteCode = siteDayGroup.Key.Nmi,
                        Date = siteDayGroup.Key.ReadDate,
                        Channels = new Dictionary<string, ChannelDay>(),
                    };
                    result.SitesDays.Add(siteDay);

                    var read1 = siteDayGroup.OrderBy(x => x.EndDateTimeExclusive).First();
                    var read2 = siteDayGroup.OrderBy(x => x.EndDateTimeExclusive).Skip(1).First();
                    var interval = (int)(read2.EndDateTimeExclusive.Subtract(read1.EndDateTimeExclusive).TotalMinutes);

                    if (!allowedIntervals.Contains(interval))
                    {
                        result.LogMessages.Add(new FileLogMessage($"Invalid interval [{interval}] between reads found ", Microsoft.Extensions.Logging.LogLevel.Error, filename, read1.LineNumber, 0));
                        continue;
                    }

                    int expectedPeriods = 60 * 24 / interval;

                    ChannelDay eChannel = new ChannelDay()
                    {
                        Channel = $"E1",
                        IntervalMinutes = interval,
                        ChannelNumber = "1",
                        ChannelType = ChanelType.ActivePowerConsumption,
                        Ignore = false,
                        Readings = new decimal[expectedPeriods],
                        TimeOfUseClasses = new TimeOfUseClass[expectedPeriods],
                        TimeStampUtc = DateTime.UtcNow,
                        MeterId = "all",
                        RegisterId = $"E1",
                        SourceFile = filename,
                        UnitOfMeasure = UnitOfMeasure.kWh,
                        OverallQuality = Quality.Actual,
                        Total = 0,
                        IsAvg = false,
                        IsNet = true,
                        IsCheck = false
                    };
                    siteDay.Channels.Add(eChannel.Channel, eChannel);

                    ChannelDay qChannel = new ChannelDay()
                    {
                        Channel = $"Q1",
                        IntervalMinutes = interval,
                        ChannelNumber = "1",
                        ChannelType = ChanelType.ReactivePowerConsumption,
                        Ignore = false,
                        Readings = new decimal[expectedPeriods],
                        TimeOfUseClasses = new TimeOfUseClass[expectedPeriods],
                        TimeStampUtc = DateTime.UtcNow,
                        MeterId = "all",
                        RegisterId = $"Q1",
                        SourceFile = filename,
                        UnitOfMeasure = UnitOfMeasure.kVArh,
                        OverallQuality = Quality.Actual,
                        Total = 0,
                        IsAvg = false,
                        IsNet = true,
                        IsCheck = false
                    };
                    siteDay.Channels.Add(qChannel.Channel, qChannel);

                    ChannelDay power = new ChannelDay()
                    {
                        Channel = $"KW",
                        IntervalMinutes = interval,
                        ChannelNumber = "1",
                        ChannelType = ChanelType.RealPower,
                        Ignore = false,
                        Readings = new decimal[expectedPeriods],
                        TimeOfUseClasses = new TimeOfUseClass[expectedPeriods],
                        TimeStampUtc = DateTime.UtcNow,
                        MeterId = "all",
                        RegisterId = $"KW",
                        SourceFile = filename,
                        UnitOfMeasure = UnitOfMeasure.kW,
                        OverallQuality = Quality.Actual,
                        Total = 0,
                        IsAvg = false,
                        IsNet = true,
                        IsCheck = false
                    };
                    siteDay.Channels.Add(power.Channel, power);

                    ChannelDay kva = new ChannelDay()
                    {
                        Channel = $"KVA",
                        IntervalMinutes = interval,
                        ChannelNumber = "1",
                        ChannelType = ChanelType.ApparentPower,
                        Ignore = false,
                        Readings = new decimal[expectedPeriods],
                        TimeOfUseClasses = new TimeOfUseClass[expectedPeriods],
                        TimeStampUtc = DateTime.UtcNow,
                        MeterId = "all",
                        RegisterId = $"KVA",
                        SourceFile = filename,
                        UnitOfMeasure = UnitOfMeasure.kVA,
                        OverallQuality = Quality.Actual,
                        Total = 0,
                        IsAvg = false,
                        IsNet = true,
                        IsCheck = false
                    };
                    siteDay.Channels.Add(kva.Channel, kva);

                    ChannelDay pf = new ChannelDay()
                    {
                        Channel = $"PF",
                        IntervalMinutes = interval,
                        ChannelNumber = "1",
                        ChannelType = ChanelType.PowerFactor,
                        Ignore = false,
                        Readings = new decimal[expectedPeriods],
                        TimeOfUseClasses = new TimeOfUseClass[expectedPeriods],
                        TimeStampUtc = DateTime.UtcNow,
                        MeterId = "all",
                        RegisterId = $"PF",
                        SourceFile = filename,
                        UnitOfMeasure = UnitOfMeasure.pf,
                        OverallQuality = Quality.Actual,
                        Total = 0,
                        IsAvg = false,
                        IsNet = true,
                        IsCheck = false
                    };
                    siteDay.Channels.Add(pf.Channel, pf);






                    foreach (var record in siteDayGroup)
                    {
                        record.Interval = interval;
                        int index = (record.StartDateTime.Hour * 60 + record.StartDateTime.Minute) / interval;
                        if (index < 0 || index >= eChannel.Readings.Length)
                        {
                            result.LogMessages.Add(new FileLogMessage($"Invalid index period found  {index} ", LogLevel.Error, filename, record.LineNumber));
                            continue;
                        }

                        eChannel.Readings[index] = record.Kwh;
                        eChannel.TimeOfUseClasses[index] = record.Tou;
                        qChannel.Readings[index] = record.Kvarh;
                        qChannel.TimeOfUseClasses[index] = record.Tou;
                        power.Readings[index] = record.Kw;
                        power.TimeOfUseClasses[index] = record.Tou;
                        kva.Readings[index] = record.Kva;
                        kva.TimeOfUseClasses[index] = record.Tou;
                        pf.Readings[index] = record.Pf;
                        pf.TimeOfUseClasses[index] = record.Tou;


                    }
                    eChannel.Total = eChannel.Readings.Sum();
                    qChannel.Total = qChannel.Readings.Sum();
                    power.Total = power.Readings.Max();
                    kva.Total = kva.Readings.Max();
                    pf.Total = pf.Readings.Max();
                }

            }
            catch (Exception ex)
            {
                result.LogMessages.Add(new FileLogMessage(ex.Message, Microsoft.Extensions.Logging.LogLevel.Critical, filename, 0, 0));
            }

            return  ;
        }


        record DataLine(string Nmi, DateTime DateTime, TimeOnly Time, decimal Kwh, decimal Kvarh, decimal Kw, decimal Kva, decimal Pf, decimal? PeakKwh, decimal? OffPeakKwh, int? DateNumber, int LineNumber)
        {
            public DateTime EndDateTimeExclusive => DateTime.Add(Time.ToTimeSpan());
            public DateTime EndDateTimeInclusive => EndDateTimeExclusive.Subtract(new TimeSpan(0, 1, 0));
            public int Interval { get; set; }

            public DateTime StartDateTime => EndDateTimeExclusive.Subtract(new TimeSpan(0, Interval, 0));

            public TimeOfUseClass Tou => PeakKwh.HasValue ? TimeOfUseClass.Peak : TimeOfUseClass.Offpeak;

        }






    }
}
