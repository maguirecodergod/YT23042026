var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HTTP Client — base address from environment
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// HTT Services
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddScoped<IHTTThemeService, HTTThemeService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IHTTModalService, HTTModalService>();
builder.Services.AddScoped<IHTTToastService, HTTToastService>();

var host = builder.Build();

// ═══ Pre-boot initialization ═══
// Theme MUST be initialized before RunAsync() so the very first Blazor
// render already has the correct ThemeClass. This eliminates FOUC entirely.
var themeService = host.Services.GetRequiredService<IHTTThemeService>();
await themeService.InitializeAsync();

await host.RunAsync();
