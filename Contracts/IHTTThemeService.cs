namespace HTT.BlazorWasm.App.Contracts;

public interface IHTTThemeService
{
    bool IsDark { get; }
    CThemeType CurrentTheme { get; }
    event Action OnThemeChanged;
    
    Task InitializeAsync();
    Task SetThemeAsync(CThemeType theme);
    Task ToggleThemeAsync();
}
