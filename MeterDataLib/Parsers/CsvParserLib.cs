using Microsoft.Extensions.Logging;
using System;

namespace MeterDataLib.Parsers
{
    static internal class CsvParserLib  
    {
  
        internal static readonly string[] allowedMimeTypes = ["text/plain", "text/csv"];
        internal static bool ValidateMime(string? mimeType)
        {
            mimeType ??= "";
            mimeType = mimeType.ToLower();
            if (!allowedMimeTypes.Contains(mimeType))
            {
                return false;
            }
            return true;
        }

          


        //internal static List<CsvLine> GetFirstXLines(Stream stream ,string filename,  int maxLines = 10 )
        //{ 

        //    // GRAB THE FIRST 10 LINES OF THE FILE
        //    List<CsvLine> lines = new List<CsvLine>();
            

        //    if( stream.CanSeek && stream.Position > 0 ) stream.Seek(0, SeekOrigin.Begin);
        //    using var csvReader = new SimpleCsvReader(stream, filename);
        //    for (int i = 0; i < maxLines; i++)
        //    {
        //        CsvLine line = csvReader.Read();
        //        if (line.Eof) break;
        //        lines.Add(line);
        //    }
        //    return lines;
        //}

        static readonly string[] NonIgnoredChannels = ["E", "Q", "B", "K"];
        internal static void UpdateFromAemoChannel( ChannelDay channelDay)
        {

            string channel = channelDay.Channel.ToUpper();

            switch(channel)
            {
                case "KWH":
                    channelDay.ChannelType = ChanelType.ActiveEnergyConsumption;
                    channelDay.UnitOfMeasure = UnitOfMeasure.kWh;
                    channelDay.RegisterId = channelDay.Channel;
                    channelDay.MeterId = "ALL";
                    channelDay.IsNet = true;
                    return; 
                    
                case "KW":
                    channelDay.ChannelType = ChanelType.RealPower;
                    channelDay.UnitOfMeasure = UnitOfMeasure.kW;
                    channelDay.RegisterId = channelDay.Channel;
                    channelDay.MeterId = "ALL";
                    channelDay.IsNet = true;
                    return;

                case "KVA":
                    channelDay.ChannelType = ChanelType.ApparentPower;
                    channelDay.UnitOfMeasure = UnitOfMeasure.kVA;
                    channelDay.RegisterId = channelDay.Channel;
                    channelDay.MeterId = "ALL";
                    channelDay.IsNet = true;
                    return;

                case "KVAH":
                    channelDay.ChannelType = ChanelType.ReactivePower;
                    channelDay.UnitOfMeasure = UnitOfMeasure.kVAr;
                    channelDay.RegisterId = channelDay.Channel;
                    channelDay.MeterId = "ALL";
                    channelDay.IsNet = true;
                    return;

                case "KVARH":
                    channelDay.ChannelType = ChanelType.ReactiveEnergyConsumption;
                    channelDay.UnitOfMeasure = UnitOfMeasure.kVArh;
                    channelDay.RegisterId = channelDay.Channel;
                    channelDay.MeterId = "ALL";
                    channelDay.IsNet = true;
                    return;
                default:
                    break;
            }



            if ( channel.Length != 2 ) return;
            string channelType = channel.Substring(0, 1);
            string channelNumber = channel.Substring(1, 1);
            
            if (NonIgnoredChannels.Contains(channelType) == false)
            {
                channelDay.Ignore = true;
            }


            channelDay.ChannelNumber = channelNumber;



            switch (channelType)
            {
                case "E":
                    channelDay.ChannelType = ChanelType.ActiveEnergyConsumption;
                    break;
                case "Q":
                    channelDay.ChannelType = ChanelType.ReactiveEnergyConsumption;
                    break;
                case "B":
                    channelDay.ChannelType = ChanelType.ActiveEnergyGeneration;
                    break;
                case "K":
                    channelDay.ChannelType = ChanelType.ReactiveEnergyGeneration;
                    break;

                case "D":
                    channelDay.ChannelType = ChanelType.ActiveEnergyConsumption;
                    channelDay.IsAvg = true;
                    break;
                case "P":
                    channelDay.ChannelType = ChanelType.ReactiveEnergyConsumption;
                    channelDay.IsAvg = true;
                    break;
                case "A":
                    channelDay.ChannelType = ChanelType.ActiveEnergyGeneration;
                    channelDay.IsAvg = true;
                    break;
                case "J":
                    channelDay.ChannelType = ChanelType.ReactiveEnergyGeneration;
                    channelDay.IsAvg = true;
                    break;


                case "F":
                    channelDay.ChannelType = ChanelType.ActiveEnergyConsumption;
                    channelDay.IsCheck = true;
                    break;
                case "R":
                    channelDay.ChannelType = ChanelType.ReactiveEnergyConsumption;
                    channelDay.IsCheck = true;
                    break;
                case "C":
                    channelDay.ChannelType = ChanelType.ActiveEnergyGeneration;
                    channelDay.IsCheck = true;
                    break;

                case "L":
                    channelDay.ChannelType = ChanelType.ReactiveEnergyGeneration;
                    channelDay.IsCheck = true;
                    break;

                case "N":
                    channelDay.ChannelType = ChanelType.ActiveEnergyConsumption;
                    channelDay.IsNet = true;
                    break;

                case "X":
                    channelDay.ChannelType = ChanelType.ReactiveEnergyConsumption;
                    channelDay.IsNet = true;
                    break;

                case "S":
                    channelDay.ChannelType = ChanelType.ApparentPowerConsumption;
                    channelDay.IsAvg = true;
                    channelDay.IsNet = true;
                    break;

                case "T":
                    channelDay.ChannelType = ChanelType.ApparentPowerConsumption;
                    channelDay.IsAvg = false;
                    channelDay.IsNet = true;
                    break;

                case "U":
                    channelDay.ChannelType = ChanelType.ApparentPowerConsumption;
                    channelDay.IsCheck = true;
                    break;

                case "G":
                    channelDay.ChannelType = ChanelType.PowerFactor;
                    break;

                case "H":
                    channelDay.ChannelType = ChanelType.Other;
                    break;

                case "Y":
                    channelDay.ChannelType = ChanelType.Other;
                    channelDay.IsCheck = true;
                    break;

                case "M":
                    channelDay.ChannelType = ChanelType.Other;
                    break;

                case "W":
                    channelDay.ChannelType = ChanelType.Other;
                    channelDay.IsCheck = true;
                    break;

                case "V":
                    if (channelDay.UnitOfMeasure == UnitOfMeasure.V)
                    {
                        channelDay.ChannelType = ChanelType.Volts;
                    }
                    else if (channelDay.UnitOfMeasure == UnitOfMeasure.A)
                    {
                        channelDay.ChannelType = ChanelType.Current;
                    }
                    else
                    {
                        channelDay.ChannelType = ChanelType.Volts;
                    }
                    break;

                case "Z":
                    if (channelDay.UnitOfMeasure == UnitOfMeasure.V)
                    {
                        channelDay.ChannelType = ChanelType.Volts;
                    }
                    else if (channelDay.UnitOfMeasure == UnitOfMeasure.A)
                    {
                        channelDay.ChannelType = ChanelType.Current;
                    }
                    else
                    {
                        channelDay.ChannelType = ChanelType.Current;
                    }
                    channelDay.IsCheck = true;
                    break;

                default:
                    channelDay.ChannelType = ChanelType.Other;
                    break;
            }
            if (channelDay.UnitOfMeasure ==  UnitOfMeasure.Other)
            {
                switch( channelDay.ChannelType)
                {
                    case ChanelType.ActiveEnergyConsumption:
                    case ChanelType.ActiveEnergyGeneration:
                        channelDay.UnitOfMeasure = UnitOfMeasure.kWh;
                        break;
                    case ChanelType.ReactiveEnergyConsumption:
                    case ChanelType.ReactiveEnergyGeneration:
                        channelDay.UnitOfMeasure = UnitOfMeasure.kVArh;
                        break;

                    case ChanelType.PowerFactor:
                        channelDay.UnitOfMeasure = UnitOfMeasure.pf;
                        break;

                    case ChanelType.Volts:
                        channelDay.UnitOfMeasure = UnitOfMeasure.V;
                        break;
                    case ChanelType.Current:
                        channelDay.UnitOfMeasure = UnitOfMeasure.A;
                        break;
                }
            }




        }



        internal static (decimal mult, bool isValid)  UpdateUom(ChannelDay channelDay , string uom )
        {
            (decimal mult, bool isValid) result = ( 1m, true);
            uom ??= string.Empty;
            var uomEnum = UnitOfMeasureExtensions.ToUom(uom);
            if (uomEnum == UnitOfMeasure.Other)
            {
                result.mult = 1;
                result.isValid = false;
                channelDay.UnitOfMeasure = uomEnum;
                channelDay.OriginalUnitOfMeasureSymbol = uom;
                channelDay.OriginalUnitOfMeasure = UnitOfMeasure.Other;
            }
            else
            {

                var (stdUnitOfMeasure, stdConversionFactor) = uomEnum.ToStandardUnit();
                if (stdUnitOfMeasure != uomEnum || stdConversionFactor != 1m)
                {
                    channelDay.UnitOfMeasure = stdUnitOfMeasure;
                    result.mult  = stdConversionFactor;
                    result.isValid = true;
                    channelDay.OriginalUnitOfMeasureSymbol = uom;
                    channelDay.OriginalUnitOfMeasure = uomEnum;
                }
                else
                {
                    channelDay.UnitOfMeasure = stdUnitOfMeasure;
                    result.mult  = 1m;
                    result.isValid = true;
                    channelDay.OriginalUnitOfMeasureSymbol = null;
                    channelDay.OriginalUnitOfMeasure = null;
                }
            }
            return result;

        }



        internal static void UpdateQuality(ChannelDay channelDay, string qualityMethod)
        {
            qualityMethod = qualityMethod?.ToUpper()?.Trim() ?? string.Empty;
            string quality = qualityMethod.Length > 0 ? qualityMethod.Substring(0, 1) : string.Empty;
            var qualityCode = QualityExtensions.ToQuality(quality);
            channelDay.OverallQuality = qualityCode;
            if (qualityMethod.Length > 1)
            {

                string estimationType = qualityMethod.Substring(1);
                channelDay.Metadata.Add(new MeterDataInfo(MetaDataName.EstimationType, estimationType));
            }



        }

    }


 


}
