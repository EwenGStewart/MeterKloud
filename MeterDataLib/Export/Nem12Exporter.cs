using System.Text;

namespace MeterDataLib.Export
{
    static class Nem12Exporter
    {
        static int[] ValidNem12Intervals = new int[] { 5, 15, 30 };
        static readonly string[] BannedChannels = new string[] { "KWH", "KVARH", "KVA", "PF", "KW", "DOLLARS" };
        public static void ExportNem12(ExportOptions options, StringBuilder writer)
        {
            if (options.IntervalInMinutes.HasValue)
            {
                if (ValidNem12Intervals.Contains(options.IntervalInMinutes.Value) == false)
                {
                    throw new ArgumentException("Invalid interval minutes for NEM12 export. Must be 5,15,30");
                }
            }


            if (options.IncludeHeader)
            {
                writer.AppendLine($"100,NEM12,{DateTime.Today:yyyyMMddHHmm},METERKLOUD,UNKNOWN");
            }

            // create the raw NEM12 records
            var records = options.SiteDays.SelectMany(x => x.Channels.Values, (sd, cd) => (sd, cd))
                .Where(x => options.ChannelTypes.Contains(x.cd.ChannelType))
                .Where( x=> !BannedChannels.Contains( x.cd.Channel.ToLowerInvariant() ))
                .Where(x => x.cd.Ignore == false)
                .Select(x => new Nem12_Record(x.sd, x.cd))
                .ToArray();

            FixAnyBadNMIs(records);
            FixAnyBadChannels(records);
            // fix any missing serial or register ids
            foreach (var record in records.Where(x => x.IsValidRegisterOrSerial() == false))
            {
                record.FixRegAndSerial();
            }

            // fix any intervals that are invalid or which dont match the requested interval 
            foreach (var record in records)
            {
                record.FixInterval(options.IntervalInMinutes);
            }

            // fix the configurations 
            foreach (var recordGroup in records.GroupBy(x => new { x.NMI, x.Day }))
            {
                var configString = string.Join("", recordGroup.Select(x => x.Channel).Distinct().Order());
                foreach (var record in recordGroup)
                {
                    record.NMIConfiguration = configString;
                }
            }
            // write out the records 
            foreach (var recordGroup in
                records.GroupBy(x => new { x.NMI, x.Channel, x.NMIConfiguration, x.MeterSerialNumber, x.Interval, x.RegisterID })
                .OrderBy(x => x.Key.NMI).ThenBy(x => x.Min(y => y.Day)).ThenBy(x => x.Key.Channel)
                )
            {
                var record = recordGroup.First();
                record.Write200Line(writer);
                foreach (var data in recordGroup)
                {
                    data.Write300Line(writer);
                }

            }


            if (options.IncludeHeader)
            {
                writer.AppendLine($"900");
            }


        }
        private static void FixAnyBadNMIs(Nem12_Record[] records)
        {
            int loopProtection = 1000;
            // Fix up the NMIs 
            while (records.Any(x => !x.IsValidNemNmi()) && loopProtection > 0)
            {
                // get the NMI 
                var firstBadNMi = records.First(x => !x.IsValidNemNmi()).NMI;
                var fixedValue = firstBadNMi;

                fixedValue = fixedValue.PadRight(10, '0');
                // replace any chars that are not 0-9 or A-Z  ( except I and O ) with a X 
                fixedValue = new string(fixedValue.Select(x => char.IsLetterOrDigit(x) && x != 'O' && x != 'I' ? x : 'X').ToArray());
                // now generate a unique name 
                int counter = 1;
                fixedValue = fixedValue[..(10 - counter.ToString().Trim().Length)] + counter.ToString().Trim();
                while (records.Any(x => x.NMI == fixedValue))
                {
                    counter++;
                    fixedValue = fixedValue[..(10 - counter.ToString().Trim().Length)] + counter.ToString().Trim();
                }
                foreach (var record in records)
                {
                    if (record.NMI == firstBadNMi)
                    {
                        record.NMI = fixedValue;
                    }
                }
                loopProtection--;
            }
        }

        private static void FixAnyBadChannels(Nem12_Record[] records)
        {
            int loopProtection = 1000;
            // Fix up the NMIs 
            while (records.Any(x => !x.IsValidChannel()) && loopProtection > 0)
            {
                // get the NMI 
                var firstBadChannel = records.First(x => !x.IsValidChannel());
                var oldChannel = firstBadChannel.Channel;

                var fixedValue = firstBadChannel.Channel;
                fixedValue = "O1";
                // now generate a unique name 
                int counter = 1;
                char[] chars = "123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
                fixedValue = fixedValue[0] + chars[counter].ToString();
                while (counter < chars.Length - 1 && records.Any(x => x.NMI == firstBadChannel.NMI && x.Channel == fixedValue))
                {
                    counter++;
                    fixedValue = fixedValue[0] + chars[counter].ToString();
                }
                if (counter == chars.Length) fixedValue = "XX";

                foreach (var record in records)
                {
                    if (record.NMI == firstBadChannel.NMI && record.Channel == oldChannel)
                    {
                        record.Channel = fixedValue;
                    }
                }
                loopProtection--;
            }
        }

        class Nem12_Record
        {
            readonly SiteDay _siteDay;
            readonly ChannelDay _channelDay;

            public Nem12_Record(SiteDay siteDay, ChannelDay channelDay)
            {
                _siteDay = siteDay;
                _channelDay = channelDay;
                NMI = siteDay.SiteCode.Trim().ToUpperInvariant();
                Channel = channelDay.Channel.Trim().ToUpperInvariant();
                RegisterID = channelDay.RegisterId.Trim().ToUpperInvariant();
                MeterSerialNumber = channelDay.MeterId.Trim().ToUpperInvariant();
                Uom = channelDay.UnitOfMeasure.ToSymbol();
                NMIConfiguration = "";
                Day = siteDay.Date;
                Interval = channelDay.IntervalMinutes;
                QualityMethod =          channelDay.OverallQuality == Quality.Unknown ? "A" 
                                       : channelDay.OverallQuality == Quality.Missing ? "A" 
                                       : channelDay.OverallQuality.ToShortString();
                ReasonCode = "";
                ReasonDescription = "";
                UpdateDateTime = channelDay.TimeStampUtc.AddHours(10);
                Data = channelDay.Readings;

            }

            public string NMI { get; set; } = string.Empty;
            public string Channel { get; set; } = string.Empty;
            public string RegisterID { get; set; } = string.Empty;
            public string MeterSerialNumber { get; set; } = string.Empty;
            public string Uom { get; set; } = string.Empty;
            public string NMIConfiguration { get; set; } = string.Empty;
            public DateTime Day { get; set; } = DateTime.MinValue;
            public decimal[] Data { get; set; } = Array.Empty<decimal>();

            public int Interval { get; set; } = 0;
            public string QualityMethod { get; set; } = string.Empty;
            public string ReasonCode { get; set; } = string.Empty;
            public string ReasonDescription { get; set; } = string.Empty;
            public DateTime UpdateDateTime { get; set; }


            public bool IsValidNemNmi()
            {
                if (NMI.Length != 10) return false;
                // validate that the nmi only contains the characters 0-9 and A-Z but not O or I
                return NMI.All(x => char.IsLetterOrDigit(x) && x != 'O' && x != 'I');
            }

            public bool IsValidChannel()
            {
                if (Channel.Length != 2) return false;
                // validate that the channel only contains the characters 0-9 and A-Z but not O or I
                return Channel.All(x => char.IsLetterOrDigit(x));
            }


            public bool IsValidRegisterOrSerial()
            {
                if (string.IsNullOrWhiteSpace(MeterSerialNumber) || string.IsNullOrWhiteSpace(RegisterID)) return false;
                if (MeterSerialNumber.Length > 15) return false;
                if (RegisterID.Length > 12) return false;
                return true;
            }


            public void FixRegAndSerial()
            {
                if (IsValidRegisterOrSerial()) return;
                if (string.IsNullOrWhiteSpace(MeterSerialNumber))
                {
                    MeterSerialNumber = _channelDay.ChannelNumberOrMeterName.Trim().ToUpperInvariant();
                }
                if (string.IsNullOrWhiteSpace(MeterSerialNumber))
                {
                    MeterSerialNumber = "METER" + Channel[^0];
                }
                if (string.IsNullOrWhiteSpace(RegisterID))
                {
                    RegisterID = Channel;
                }

            }


            public void FixInterval(int? interval)
            {
                interval ??= this.Interval;

                if (interval < 5) interval = 5;
                if (interval > 30) interval = 30;
                if (interval != 5 && interval != 15 && interval != 30)
                {
                    interval = 30;
                }

                int newLength = 60 * 24 / interval.Value;

                if (interval.Value == this.Interval && this.Data.Length == newLength) return;
                if (this.Data.Length == 0)
                {
                    Data = new decimal[newLength];
                    Interval = interval.Value;
                    return;
                }
                Data = ProfileHelpers.UnifyLength(Data, newLength, true);
                Interval = interval.Value;

            }

            private void FixQuality()
            {
                if (string.IsNullOrWhiteSpace(QualityMethod))
                {
                    QualityMethod = "E";
                }
                if (QualityMethod != "V" && QualityMethod != "A" && QualityMethod != "N" && QualityMethod.Length == 1)
                {
                    // do we have a global method 
                    var method = _channelDay.Metadata.Where(x => x.MeterDataName == MetaDataName.EstimationType).Select(x => x.Value).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(method) && method.Length <= 2)
                    {
                        method = method.PadLeft(2, '0');
                        QualityMethod += method[..2];
                    }
                }


            }

            private void FixReasonCode()
            {
                if (QualityMethod == "V")
                {
                    ReasonCode = string.Empty;
                    ReasonDescription = string.Empty;
                    return;
                }
                var strReason = _channelDay.Metadata.Where(x => x.MeterDataName == MetaDataName.ReasonCode).Select(x => x.Value).FirstOrDefault();
                var strDescription = _channelDay.Metadata.Where(x => x.MeterDataName == MetaDataName.ReasonDescription).Select(x => x.Value).FirstOrDefault();

                if (int.TryParse(strReason, out int code))
                {
                    if (code < 0 || code > 99) { code = 0; }
                    ReasonCode = code.ToString();
                }
                else
                {
                    ReasonCode = "0";
                }

                if (ReasonCode == "0" && string.IsNullOrEmpty(strDescription) && "ANE".Contains(QualityMethod[0]))
                {
                    ReasonCode = "";
                    ReasonDescription = "";
                    return;
                }
                if (ReasonCode == "0" && string.IsNullOrEmpty(strDescription))
                {
                    ReasonDescription = "Unknown reason";
                }

                ReasonDescription = (strDescription ?? string.Empty).Replace(',', ' ').Replace('\n', ' ').Replace('\r', ' ').Trim();

            }

            public void Write200Line(StringBuilder writer)
            {
                writer.AppendLine($"200,{NMI},{NMIConfiguration},{RegisterID},{Channel},{Channel},{MeterSerialNumber},{Uom},{Interval},");
            }

            public void Write300Line(StringBuilder writer)
            {
                writer.Append($"300,{Day:yyyyMMdd}");
                foreach (var value in Data)
                {
                    writer.Append($",{value}");
                }
                FixQuality();
                FixReasonCode();
                if (UpdateDateTime.Year < 2000)
                {
                    UpdateDateTime = DateTime.Now;
                }
                writer.AppendLine($",{QualityMethod},{ReasonCode},{ReasonDescription},{UpdateDateTime:yyyyMMddHHmmss}");
                // write out the partial quality methods 
                if (QualityMethod == "V")
                {
                    ProcessVariableQualityLine(writer);

                }
                string? transactionCode = _channelDay.Metadata.Where(x => x.MeterDataName == MetaDataName.TransactionCode).Select(x => x.Value).FirstOrDefault();
                string? serviceOrder = _channelDay.Metadata.Where(x => x.MeterDataName == MetaDataName.ServiceOrder).Select(x => x.Value).FirstOrDefault();
                IndexRead? startIndex = _channelDay.StartIndexRead ?? _channelDay.EndIndexRead;
                if (transactionCode != null || serviceOrder != null || startIndex != null)
                {
                    writer.Append($"500,{transactionCode},{serviceOrder}");
                    if (startIndex != null)
                    {
                        writer.Append($",{startIndex.Value.TimeStamp:yyyyMMddHHmmss},{startIndex.Value.ToString()}");
                    }
                    else
                    {
                        writer.Append(",,");
                    }
                    writer.AppendLine();
                }



            }

            private void ProcessVariableQualityLine(StringBuilder writer)
            {
                int originalLength = _channelDay.Readings.Length;
                Quality[] qualities = new Quality[originalLength];
                int[] reasonCode = new int[originalLength];
                string[] reasonDescription = new string[originalLength];
                string[] method = new string[originalLength];

                if (_channelDay.ReadQualities != null && _channelDay.ReadQualities.Length == qualities.Length)
                {
                    qualities = _channelDay.ReadQualities;
                }
                // process the metadata
                foreach (var meta in _channelDay.Metadata.Where(x => x.FromIndex > 0))
                {
                    if (meta.MeterDataName == MetaDataName.ReasonCode)
                    {
                        if (int.TryParse(meta.Value, out int code))
                        {
                            if (code >= 0 && code < 99)
                            {
                                for (int i = meta.FromIndex; i <= meta.ToIndex; i++)
                                {
                                    int index = i - 1;
                                    if (i >= 0 && i < originalLength)
                                    {
                                        reasonCode[i] = code;
                                    }
                                }
                            }
                        }
                    }
                    if (meta.MeterDataName == MetaDataName.ReasonDescription)
                    {
                        for (int i = meta.FromIndex; i <= meta.ToIndex; i++)
                        {
                            int index = i - 1;
                            if (i >= 0 && i < originalLength)
                            {
                                reasonDescription[i] = meta.Value;
                            }
                        }

                    }
                    if (meta.MeterDataName == MetaDataName.EstimationType)
                    {

                        for (int i = meta.FromIndex; i <= meta.ToIndex; i++)
                        {
                            int index = i - 1;
                            if (i >= 0 && i < originalLength)
                            {
                                method[i] = meta.Value;
                            }
                        }

                    }
                }
                // process the quality methods
                for (int i = 0; i < originalLength; i++)
                {
                    string quality = qualities[i].ToShortString();
                    method[i] ??= string.Empty;
                    reasonDescription[i] ??= string.Empty;
                    if (!"ANFSE".Contains(quality))
                    {
                        quality = "A";
                    }
                    if (!"AN".Contains(quality) && method[i].Length < 2)
                    {
                        method[i] = method[i].PadLeft(2, '0');
                    }
                    if (reasonCode[i] < 0 || reasonCode[i] > 99)
                    {
                        reasonCode[i] = 0;
                    }
                    if (string.IsNullOrEmpty(reasonDescription[i]) && !"AN".Contains(quality) && reasonCode[i] == 0)
                    {
                        reasonDescription[i] = "Unknown reason";
                    }
                }
                // now handle if the interval has been altered 
                if (originalLength != Data.Length)
                {
                    qualities = ProfileHelpers.UnifyQuality(qualities, Data.Length);
                    reasonCode = ProfileHelpers.UnifyInt(reasonCode, Data.Length);
                    reasonDescription = ProfileHelpers.UnifyString(reasonDescription, Data.Length);
                    method = ProfileHelpers.UnifyString(method, Data.Length);
                    // where data may have been lost as a result of the unification process
                    if (originalLength > Data.Length)
                    {
                        // just ensure rules around substitution method and reason are maintained 
                        for (int i = 0; i < Data.Length; i++)
                        {
                            string quality = qualities[i].ToShortString();
                            if (!"AN".Contains(quality) && method[i].Length < 2)
                            {
                                method[i] = method[i].PadLeft(2, '0');
                            }
                            if (string.IsNullOrEmpty(reasonDescription[i]) && !"ANE".Contains(quality) && reasonCode[i] == 0)
                            {
                                reasonDescription[i] = "Unknown reason";
                            }
                        }
                    }
                }

                // now we need to write out the 400 records 
                int startIndex = 0;

                while (startIndex < Data.Length)
                {
                    int endIndex = startIndex + 1;
                    while (endIndex < Data.Length && qualities[startIndex] == qualities[endIndex]
                                                    && reasonCode[startIndex] == reasonCode[endIndex]
                                                    && reasonDescription[startIndex] == reasonDescription[endIndex]
                                                    && method[startIndex] == method[endIndex])
                    {
                        endIndex++;
                    }
                    endIndex--;
                    int startInterval = startIndex + 1;
                    int endInterval = endIndex + 1;
                    string qualityMethod = qualities[startIndex].ToShortString();
                    if (!"AN".Contains(qualityMethod))
                    {
                        qualityMethod = qualityMethod + method[startIndex];
                    }
                    string strReasonCode = reasonCode[startIndex].ToString();
                    if (strReasonCode == "0" && string.IsNullOrEmpty(reasonDescription[startIndex]) && "ANE".Contains(qualityMethod[0]))
                    {
                        strReasonCode = "";
                    }
                    var strReasonDescription = reasonDescription[startIndex].Replace(',', ' ').Replace('\n', ' ').Replace('\r', ' ').Trim();

                    writer.AppendLine($"400,{startInterval},{endInterval},{qualityMethod},{strReasonCode},{strReasonDescription}");
                    startIndex = endIndex + 1;
                }
            }
        }
    }


}