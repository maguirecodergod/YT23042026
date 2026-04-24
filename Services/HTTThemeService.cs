namespace HTT.BlazorWasm.App.Services;

internal sealed class HTTThemeService : IHTTThemeService
{
    private readonly IJSRuntime _js;
    private bool _isDark = true;

    public HTTThemeService(IJSRuntime js) => _js = js;

    public bool IsDark => _isDark;
    public event Action? OnThemeChanged;

    public async Task InitializeAsync()
    {
        try
        {
            var saved = await _js.InvokeAsync<string>(identifier: "localStorage.getItem", args: new[] { "htt-theme" });
            if (!string.IsNullOrEmpty(saved))
            {
                _isDark = saved == "dark";
            }
            else
            {
                // Optional: Check browser preference
                var prefersDark = await _js.InvokeAsync<bool>("eval", "window.matchMedia('(prefers-color-scheme: dark)').matches");
                _isDark = prefersDark;
            }
            await ApplyThemeToDomAsync();
        }
        catch { /* Fallback to default */ }
    }

    public async Task SetThemeAsync(bool isDark)
    {
        if (_isDark == isDark) return;
        _isDark = isDark;
        await _js.InvokeVoidAsync("localStorage.setItem", "htt-theme", isDark ? "dark" : "light");
        await ApplyThemeToDomAsync();
        OnThemeChanged?.Invoke();
    }

    public async Task ToggleThemeAsync() => await SetThemeAsync(!_isDark);

    private async Task ApplyThemeToDomAsync()
    {
        var theme = _isDark ? "dark" : "light";
        // Update both to be safe
        await _js.InvokeVoidAsync("eval", $"document.documentElement.setAttribute('data-theme', '{theme}')");
        await _js.InvokeVoidAsync("eval", $"document.documentElement.setAttribute('data-bs-theme', '{theme}')");
    }
}
