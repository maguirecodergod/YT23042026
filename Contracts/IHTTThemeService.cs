namespace HTT.BlazorWasm.App.Contracts;

public interface IHTTThemeService
{
    bool IsDark { get; }
    event Action OnThemeChanged;
    Task InitializeAsync();
    Task SetThemeAsync(bool isDark);
    Task ToggleThemeAsync();
}
