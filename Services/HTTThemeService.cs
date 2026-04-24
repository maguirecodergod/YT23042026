namespace HTT.BlazorWasm.App.Services
{
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

        public event Action? OnThemeChanged;

        public async Task InitializeAsync()
        {
            try
            {
                // 1. Get initial system preference
                _isSystemDark = await _js.InvokeAsync<bool>("httTheme.getSystemPreference");

                // 2. Setup listener for system theme changes
                _dotNetRef = DotNetObjectReference.Create(this);
                await _js.InvokeVoidAsync("httTheme.initThemeListener", _dotNetRef);

                // 3. Load saved theme preference
                var saved = await _js.InvokeAsync<string>("localStorage.getItem", "htt-theme");
                if (Enum.TryParse<CThemeType>(saved, true, out var theme))
                {
                    _currentTheme = theme;
                }
                else
                {
                    _currentTheme = CThemeType.System;
                }

                await ApplyThemeToDomAsync();
            }
            catch { /* Fallback to default */ }
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

            await ApplyThemeToDomAsync();
            OnThemeChanged?.Invoke();
        }

        public async Task ToggleThemeAsync()
        {
            // Rotation: System -> Light -> Dark -> System
            var nextTheme = _currentTheme switch
            {
                CThemeType.System => CThemeType.Light,
                CThemeType.Light => CThemeType.Dark,
                CThemeType.Dark => CThemeType.System,
                _ => CThemeType.System
            };
            await SetThemeAsync(nextTheme);
        }

        private async Task ApplyThemeToDomAsync()
        {
            var value = _currentTheme == CThemeType.System ? "system" :
                        (_currentTheme == CThemeType.Dark ? "dark" : "light");

            await _js.InvokeVoidAsync("httTheme.applyTheme", value);
        }

        public void Dispose()
        {
            _dotNetRef?.Dispose();
        }
    }
}
