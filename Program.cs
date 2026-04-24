var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HTTP Client — base address from environment
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// HTT Services
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddScoped<IHTTThemeService, HTTThemeService>();

await builder.Build().RunAsync();
