using AzureStaticWebApps.Blazor.Authentication;
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
builder.Services.AddScoped(sp => new HttpClient { BaseAddress=new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddStaticWebAppsAuthentication();



await builder.Build().RunAsync();





