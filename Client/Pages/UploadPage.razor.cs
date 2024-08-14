using MeterDataLib.Parsers;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using System.Collections.ObjectModel;
using System.Threading;


namespace MeterKloud.Pages
{
    public partial class UploadPage
    {
        private enum internalState
        {
            GetFiles,
            UploadFiles,
            Completed
        }
        internalState _state = internalState.GetFiles;


        ObservableCollection<MeterDataFile> _items = new ObservableCollection<MeterDataFile>();

        UploadPageGetFiles UploadPageGetFiles { get; set; } = default!;


        private CancellationTokenSource? _tokenSource = null;
      
        private CancellationToken? _cancellationToken;



        bool IsParsing => _state == internalState.UploadFiles;
        bool CancelButtonDisabled => !IsParsing || (_cancellationToken?.IsCancellationRequested ?? false);
        bool CanViewSites => ! ( _cancellationToken?.IsCancellationRequested ?? false ) && ( _state == internalState.Completed ) && ( _items.Any(x=>x.Days > 0 ));

        bool NoErrorsAutoView => CanViewSites && (_items.All(x => x.Success));


        bool UploadMOreDisabled => !(_state == internalState.Completed);
        bool ViewDataDisabled => !(CanViewSites) ; 


        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await Api.InitApi();
        }

        async Task UploadMoreFiles(MouseEventArgs mouseEventArgs)
        {
            _state = internalState.GetFiles;
            await UploadPageGetFiles.ClearAsync();
        }

    

        async Task OnFilesSelected(IReadOnlyList<IBrowserFile> files)
        {
            _items = new ObservableCollection<MeterDataFile>(files.Select(f => new MeterDataFile(f)));
            _state = internalState.UploadFiles;
            _tokenSource = new CancellationTokenSource();
            _cancellationToken = _tokenSource.Token;
            try
            {
                await InvokeAsync(StateHasChanged);
                foreach (MeterDataFile file in _items)
                {
                    
                    try
                    {

                        await file.Parse(UpdateProgress, _cancellationToken.Value , Api );
               
                    }
                    catch (OperationCanceledException)
                    {
                        file.AddException("Operation Cancelled");
                    }   


                    catch (Exception ex)
                    {
                        file.AddException(ex);
                    }

                    try
                    {
                        if (file.ParserResult == null)
                        {
                             
                            file.AddException(new Exception("[K79MSS] No parser result"));
                        }
                        else if (file.ParserResult.TotalSiteDays == 0)
                        {
                             
                            file.AddException(new Exception("[HVQVYM] No data"));
                             

                        }
                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[JTUDF7] Error parsing {file.FileName}");
                        Console.WriteLine(ex);
                        file.AddException(ex);
                    }
                    await InvokeAsync(StateHasChanged);
                }

            }
            finally
            {
                _state = internalState.Completed;
                _tokenSource.Dispose();
                _tokenSource = null;
            }

        }



        void CancelUpload( MouseEventArgs mouseEventArgs)
        {
                _state = internalState.Completed;
                _tokenSource?.Cancel();
        }


        async Task UpdateProgress(ParserResult result) => await InvokeAsync(StateHasChanged);


        async Task GetSiteCode(MeterDataFile file)
        {
            try
            {
                if (!(file.ParserResult != null && file.Parsed && file.Sites > 0 && file.HasUnknownSites))
                {
                    return;
                }
                var options = new DialogOptions { CloseOnEscapeKey = true };
                var diagParams = new DialogParameters<UploadPageDialogGetSiteName>
                {
                    { "Filename", file.FileName }
                };
                IDialogReference diagRef = await DialogService.ShowAsync<UploadPageDialogGetSiteName>("Name Site", diagParams, options);

                var siteName = await diagRef.GetReturnValueAsync<string>();

                if (!string.IsNullOrWhiteSpace(siteName))
                {
                    Console.WriteLine($"Setting site name to {siteName}");
                    file.ParserResult?.SetUnknownSiteName(siteName);
                    await InvokeAsync(StateHasChanged);
                }

            }
            catch (Exception ex)
            {
                file.ParserResult.AddException(ex);
            }
        }


        async Task ShowLog(MeterDataFile file)
        {
            var options = new DialogOptions { CloseOnEscapeKey = true, FullWidth = true, Position = DialogPosition.TopCenter, MaxWidth = MaxWidth.ExtraLarge };
            var diagParams = new DialogParameters<UploadPageDialogShowLog>
            {
                { "Filename", file.FileName },
                { "LogMessages", file.ParserResult?.LogMessages }
            };
            IDialogReference diagRef = await DialogService.ShowAsync<UploadPageDialogShowLog>("Log", diagParams, options);

        }


    }
}