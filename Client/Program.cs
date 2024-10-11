using AzureStaticWebApps.Blazor.Authentication;
using Blazored.LocalStorage;
using MeterDataLib.Storage;
using MeterKloud;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Caching.Memory;
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

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IndexedDbAccessor>();
builder.Services.AddSingleton<IMeterDataStore,IndexDbMeterDataStore>();
builder.Services.AddSingleton<MeterDataStorageManager>();
builder.Services.AddSingleton<MeterKloudClientApi>();
builder.Services.AddBlazoredLocalStorageAsSingleton();

var host = builder.Build();
await using  (var scope = host.Services.CreateAsyncScope())
{

    var api = scope.ServiceProvider.GetRequiredService<MeterKloudClientApi>();
    await api.InitApi();

}


await host.RunAsync();





