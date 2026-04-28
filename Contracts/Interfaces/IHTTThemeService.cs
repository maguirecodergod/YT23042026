namespace HTT.BlazorWasm.App.Contracts;

public interface IHTTThemeService
{
    bool IsDark { get; }
    CThemeType CurrentTheme { get; }
    
    /// <summary>
    /// Returns the CSS class to apply to the root layout wrapper.
    /// Value: "theme-dark" | "theme-light"
    /// </summary>
    string ThemeClass { get; }
    
    event Action OnThemeChanged;
    
    Task InitializeAsync();
    Task SetThemeAsync(CThemeType theme);
    Task ToggleThemeAsync();
}
