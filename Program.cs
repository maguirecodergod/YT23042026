using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HTT.BlazorWasm.App;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HTT Services
builder.Services.AddScoped<HTT.BlazorWasm.App.Contracts.IHTTThemeService, HTT.BlazorWasm.App.Services.HTTThemeService>();

// HTTP Client — base address from environment
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
