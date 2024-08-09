using AzureStaticWebApps.Blazor.Authentication;
using MeterDataLib.Storage;
using MeterKloud;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();


//builder.Services.AddAuthorizationCore();
//builder.Services.AddAuthenticationCore();
//builder.Services.AddScoped<AuthenticationStateProvider, MyServerAuthenticationStateProvider>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddStaticWebAppsAuthentication();
Console.WriteLine("Adding AddMemoryCache");
builder.Services.AddMemoryCache();
Console.WriteLine("Adding IndexedDbAccessor");
builder.Services.AddScoped<IndexedDbAccessor>();
Console.WriteLine("Adding IndexDbMeterDataStore");
builder.Services.AddScoped<IMeterDataStore,IndexDbMeterDataStore>();
Console.WriteLine("Adding MeterDataStorageManager");
builder.Services.AddScoped<MeterDataStorageManager>();
Console.WriteLine("build");
var host = builder.Build();

await using  (var scope = host.Services.CreateAsyncScope())
{
    await using (var indexedDB = scope.ServiceProvider.GetService<IndexedDbAccessor>())
    {

        if (indexedDB is not null)
        {
            await indexedDB.InitializeAsync();
        }
    }
}


await host.RunAsync();





