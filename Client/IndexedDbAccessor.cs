using Microsoft.JSInterop;

namespace MeterKloud
{
    public class IndexedDbAccessor : IAsyncDisposable
    {
        private Lazy<IJSObjectReference> _accessorJsRef = new();
        private readonly IJSRuntime _jsRuntime;

        public IndexedDbAccessor(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync()
        {
            await WaitForReference();
            await _accessorJsRef.Value.InvokeVoidAsync("initialize");
        }

        private async Task WaitForReference()
        {
            if (_accessorJsRef.IsValueCreated is false)
            {
                _accessorJsRef = new(await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "/js/IndexedDbAccessor.js"));
            }
        }
        

        public async ValueTask DisposeAsync()
        {
            if (_accessorJsRef.IsValueCreated)
            {
                await _accessorJsRef.Value.DisposeAsync();
            }
        }



        public async Task<T> GetValueAsync<T>(string collectionName, string id)
        {
            await WaitForReference();
            var result = await _accessorJsRef.Value.InvokeAsync<T>("get", collectionName, id);

            return result;
        }

        public async Task SetValueAsync<T>(string collectionName, T value)
        {
            await WaitForReference();
            await _accessorJsRef.Value.InvokeVoidAsync("set", collectionName, value);
        }


        //get all values from a collection
        public async Task<List<T>> GetAllValuesAsync<T>(string collectionName)
        {
            await WaitForReference();
            var result = await _accessorJsRef.Value.InvokeAsync<List<T>>("getAll", collectionName);

            return result;
        }

        // get range 
        public async Task<List<T>> GetRangeAsync<T>(string collectionName, string fromId, string toId)
        {
            await WaitForReference();
            var result = await _accessorJsRef.Value.InvokeAsync<List<T>>("getRange", collectionName, fromId, toId);

            return result;
        }

        // remove 
        public async Task RemoveValueAsync(string collectionName, string id)
        {
            await WaitForReference();
            await _accessorJsRef.Value.InvokeVoidAsync("remove", collectionName, id);
        }



    }
}
