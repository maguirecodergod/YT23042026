namespace HTT.BlazorWasm.App.Services
{
    /// <summary>
    /// Theme service using the CSS-class pattern.
    /// 
    /// Architecture (learned from LHA reference project):
    ///   - Theme state is resolved from localStorage in Program.cs 
    ///     BEFORE host.RunAsync() — so the very first Blazor render
    ///     already has the correct ThemeClass.
    ///   - Theme is applied as a CSS class ("theme-dark" / "theme-light")
    ///     on the root wrapper div in MainLayout, NOT via data-* attributes
    ///     on the html element.
    ///   - No JS interop is needed for theme application — zero FOUC by design.
    ///   - The only JS used is localStorage read (via IJSRuntime) during init.
    /// </summary>
    internal sealed class HTTThemeService : IHTTThemeService, IDisposable
    {
        private readonly IJSRuntime _js;
        private CThemeType _currentTheme = CThemeType.System;
        private bool _isSystemDark = false;
        private DotNetObjectReference<HTTThemeService>? _dotNetRef;

        public HTTThemeService(IJSRuntime js) => _js = js;

        public CThemeType CurrentTheme => _currentTheme;

        public bool IsDark => _currentTheme switch
        {
            CThemeType.Dark => true,
            CThemeType.Light => false,
            _ => _isSystemDark
        };

        public string ThemeClass => IsDark ? "theme-dark" : "theme-light";

        public event Action? OnThemeChanged;

        public async Task InitializeAsync()
        {
            try
            {
                // 1. Detect OS preference
                _isSystemDark = await _js.InvokeAsync<bool>("httTheme.getSystemPreference");

                // 2. Listen for OS-level theme changes
                _dotNetRef = DotNetObjectReference.Create(this);
                await _js.InvokeVoidAsync("httTheme.initThemeListener", _dotNetRef);

                // 3. Load saved theme from localStorage
                var saved = await _js.InvokeAsync<string>("localStorage.getItem", "htt-theme");
                _currentTheme = Enum.TryParse<CThemeType>(saved, true, out var parsed)
                    ? parsed
                    : CThemeType.System;

                // No DOM manipulation needed — ThemeClass is read by Blazor components
                // during their first render, which happens AFTER this method completes.
                OnThemeChanged?.Invoke();
            }
            catch { /* Fallback to default — ThemeClass will return based on _isSystemDark */ }
        }

        [JSInvokable]
        public void OnSystemThemeChanged(bool isDark)
        {
            _isSystemDark = isDark;
            if (_currentTheme == CThemeType.System)
            {
                OnThemeChanged?.Invoke();
            }
        }

        public async Task SetThemeAsync(CThemeType theme)
        {
            if (_currentTheme == theme) return;

            _currentTheme = theme;
            await _js.InvokeVoidAsync("localStorage.setItem", "htt-theme", theme.ToString().ToLower());

            OnThemeChanged?.Invoke();
        }

        public async Task ToggleThemeAsync()
        {
            var nextTheme = _currentTheme switch
            {
                CThemeType.System => CThemeType.Light,
                CThemeType.Light => CThemeType.Dark,
                CThemeType.Dark => CThemeType.System,
                _ => CThemeType.System
            };
            await SetThemeAsync(nextTheme);
        }

        public void Dispose()
        {
            _dotNetRef?.Dispose();
        }
    }
}
