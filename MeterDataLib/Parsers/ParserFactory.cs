﻿using ExcelDataReader;
using System;
using System.Data.SqlTypes;
using System.IO.Compression;
using System.Reflection;

namespace MeterDataLib.Parsers
{
    public static class ParserFactory
    {
      
        //static List<Parser> parsers = new List<Parser>() {   new CsvMultiLine1(), new CsvPowerPal()
        //    , new CsvSingleLine7()
        //    , new CsvByChannel()
        //    , new CsvSingleLineMultiColPeriod()
        //    , new CsvSingleLineMultiColPeriod2()
        //    , new CsvSingleLinePeakOffPeakDateNumber()
        //    , new CsvSingleLineSimpleEBQK()
        //    , new CsvSingleLineSimpleEBKvaPF()
        //};

        static readonly List<IParser> Parsers = [ new Nem12(),new CsvByChannel()
        , new CsvSingleLine7() ,new CsvPowerPal() ,new CsvPowerPal() , new CsvMultiLine1()
            , new CsvSingleLineMultiColPeriod() , new CsvSingleLineMultiColPeriod2()
            , new CsvSingleLinePeakOffPeakDateNumber(), new CsvSingleLineSimpleEBKvaPF()
            , new CsvSingleLineSimpleEBQK()
            , new CsvRedEnergyFormat()
            , new CsvEnosiFormat()
        ];




        static readonly string[] ExcelMimeTypes =
        [
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "application/msexcel",
            "application/vnd.ms-excel"
        ];

        static readonly string[] ExcelExtensions =
        [
            ".xls" , ".xlsx"
        ];


        public static async Task<ParserResult> ParseAsync(Stream stream, string filename, string? mimeType, Func<ParserResult, Task>? CallBack = null, CancellationToken? cancellationToken = null  , ParserResult? parserResult = null  )
        {
            cancellationToken ??= new CancellationToken();
            cancellationToken.Value.ThrowIfCancellationRequested();
            ParserResult result = parserResult ??  new ParserResult() { FileName = filename};
            result.InProgress = true;
            result.Progress = "Parse Started";
            try
            {
                mimeType ??= string.Empty;
                await ParseGeneralAsync(result, stream, filename, mimeType, CallBack, cancellationToken.Value);

            }
            
            catch (OperationCanceledException )
            {
                result.LogMessages.Add(new FileLogMessage("[5ES3JI]:  Cancelled", Microsoft.Extensions.Logging.LogLevel.Error, filename, 0, 0));
            }

            catch (Exception ex)
            {
                result.LogMessages.Add(new FileLogMessage($"[VDKA7C]: {ex.Message}", Microsoft.Extensions.Logging.LogLevel.Error, filename, 0, 0));
            }
            result.InProgress = false;
            result.Progress = "Parse Completed";
            return result;

        }

        static async Task ParseGeneralAsync(ParserResult result, Stream stream, string filename, string? mimeType, Func<ParserResult, Task>? callBack, CancellationToken cancellationToken)
        {
            mimeType ??= string.Empty;
            if (IsExcel(filename, mimeType))
            {
                await ParseExcelAsync(result, stream, filename, mimeType, callBack, cancellationToken);
            }
            else if (IsZipFile(filename, mimeType))
            {
                await ParseZipAsync(result, stream, filename, mimeType, callBack, cancellationToken);
            }
            else
            {
                await ParseCsvAsync(result, stream, filename, mimeType, callBack, cancellationToken);
            }
        }


        static bool CodePagesRegistered = false;
        static async Task ParseExcelAsync(ParserResult result, Stream stream, string filename, string? mimeType, Func<ParserResult, Task>? callBack  , CancellationToken cancellationToken )
        {

            if (!CodePagesRegistered)
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                CodePagesRegistered = true;
            }

            result.Progress = "load excel to memory";
            if (callBack != null) { await callBack(result); }
            using MemoryStream ms = new();
            await stream.CopyToAsync(ms , cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            result.Progress = "process excel files";
            if (callBack != null) { await callBack(result); }
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            using var edr = ExcelReaderFactory.CreateReader(ms);
            int sheet = 0;
            StringWriter stringWriter = new();
            do
            {
                sheet++;
                int line = 0;
                while (edr.Read() && !cancellationToken.IsCancellationRequested)
                {
                    line++;

                    if (timer.ElapsedMilliseconds > 100)
                    {
                        result.Progress = $"reading excel row {line}";
                        timer.Restart();
                        if (callBack != null)
                        {
                            await callBack(result);
                        }
                    }

                    int prevCol = 0;
                    for (int i = 0; i < edr.FieldCount; i++)
                    {
                        var value = edr.GetValue(i);
                        if (value != null)
                        {
                            string strValue = value?.ToString() ?? string.Empty;
                            if (!string.IsNullOrEmpty(strValue))
                            {
                                strValue.Replace(",", "|");
                                if (i > 0)
                                {
                                    while (prevCol < i)
                                    {
                                        stringWriter.Write(",");
                                        prevCol++;
                                    }
                                }
                                stringWriter.Write(strValue);
                            }
                        }
                    }
                    stringWriter.WriteLine();
                }


                result.Progress = $"reading csv";
                timer.Restart();
                if (callBack != null)
                {
                    await callBack(result);
                }

                var data = stringWriter.ToString();
                if (!string.IsNullOrEmpty(data))
                {
                    result.LogMessages.Add(new FileLogMessage($"Processing Sheet {sheet}", Microsoft.Extensions.Logging.LogLevel.Information, filename, 0, 0));
                    using Stream strStream = StringToStream.GenerateStreamFromString(data);
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await ParseCsvAsync(result, strStream, filename, mimeType, callBack, cancellationToken);
                    }

                    catch (OperationCanceledException)
                    {
                        return;
                    }

                    catch (Exception ex)
                    {
                        result.LogMessages.Add(new FileLogMessage($"[Y6TKZ4] Error processing sheet {sheet} - {ex.Message}", Microsoft.Extensions.Logging.LogLevel.Error, filename, 0, 0));
                    }
                }
            } while (edr.NextResult());
        }

        private static async Task ParseZipAsync(ParserResult result, Stream stream, string filename, string? mimeType, Func<ParserResult, Task>? callBack  , CancellationToken cancellationToken  )
        {
            cancellationToken.ThrowIfCancellationRequested();

            result.Progress = "Unzip to memory";
            if (callBack != null) { await callBack(result); }
            using MemoryStream ms = new();
            await stream.CopyToAsync(ms, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            result.Progress = "process zip files";
            if (callBack != null) { await callBack(result); }

            using var zip = new ZipArchive(ms, ZipArchiveMode.Read);
            foreach (var entry in zip.Entries)
            {
                using var entryStream = entry.Open();

                cancellationToken.ThrowIfCancellationRequested();

                result.LogMessages.Add(new FileLogMessage($"Processing file {entry.Name}", Microsoft.Extensions.Logging.LogLevel.Information, filename, 0, 0));
                string entryMimeType = MimeTypeHelper.GetMimeType(entry.Name);

                try
                {
                    await ParseGeneralAsync(result, entryStream, entry.FullName, entryMimeType, callBack, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                   return;
                }
                catch (Exception ex)
                {
                    result.LogMessages.Add(new FileLogMessage($"[GWLIAU] Error processing file {entry.Name} - {ex.Message}", Microsoft.Extensions.Logging.LogLevel.Error, filename, 0, 0));
                }
            }
        }




        private static async Task ParseCsvAsync(ParserResult result, Stream stream, string filename, string? mimeType, Func<ParserResult, Task>? CallBack  , CancellationToken? cancellationToken  )
        {


            cancellationToken?.ThrowIfCancellationRequested();

            using var csv = new SimpleCsvReader(stream, filename , cancellationToken);
            // read the first 10 lines 
            var first10Lines = await csv.ReadBufferedAsync(10);
            if (first10Lines.Count < 2)
            {
                result.LogMessages.Add(new FileLogMessage($"[KTRBY1]: Less than 2 lines in file - file is empty or not csv", Microsoft.Extensions.Logging.LogLevel.Error, filename, 0, 0));
                result.ParserName = "none";
                return;

            }
            var parser = Parsers.FirstOrDefault(x => x.CanParse(first10Lines));
            if (parser == null)
            {
                result.LogMessages.Add(new FileLogMessage($"[I4GCND]: No parser found for that file format", Microsoft.Extensions.Logging.LogLevel.Error, filename, 0, 0));
                result.ParserName = "none";
                return;
            }

            result.ParserName = parser.Name;
            result.LogMessages.Add(new FileLogMessage($"[0I16MU]: Parser  {parser.Name} found.", Microsoft.Extensions.Logging.LogLevel.Debug, filename, 0, 0));
            await parser.Parse(csv, result, CallBack , cancellationToken);


        }








        private static bool IsExcel(string filename, string mimeType)
        {

            // avoid FireFox issue where csv associated with excel 
            if ( filename.ToLowerInvariant().EndsWith(".csv"))
            {
                return false;
            }
            else if (!string.IsNullOrWhiteSpace(mimeType))
            {
                if (ExcelMimeTypes.Contains(mimeType?.ToLowerInvariant() ?? string.Empty))
                {
                    return true;
                }
                return false;
            }
            else
            {
                if (ExcelExtensions.Contains(Path.GetExtension(filename).ToLower()))
                { return true; }
                return false;
            }
        }

        static readonly string[] ZipMimeTypes =
        [
            "application/x-zip-compressed"
        ];

        static readonly string[] ZipExtensions =
        [
            ".zip"
        ];

        private static bool IsZipFile(string filename, string mimeType)
        {

            if (!string.IsNullOrWhiteSpace(mimeType))
            {
                if (ZipMimeTypes.Contains(mimeType?.ToLowerInvariant() ?? string.Empty))
                {
                    return true;
                }
                return false;
            }
            else
            {
                if (ZipExtensions.Contains(Path.GetExtension(filename).ToLower()))
                { return true; }
                return false;
            }
        }

    }


}
