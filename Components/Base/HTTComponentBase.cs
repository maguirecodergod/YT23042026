namespace HTT.BlazorWasm.App.Components
{
    public class HTTComponentBase : ComponentBase, IDisposable
    {
        [Inject] protected IJSRuntime JS { get; set; } = default!;
        [Inject] protected ILogger<HTTComponentBase> Logger { get; set; } = default!;
        [Inject] protected IHTTThemeService Theme { get; set; } = default!;
        [Inject] protected ILocalizationService L { get; set; } = default!;
        [Inject] protected IHTTToastService Toast { get; set; } = default!;
        [Inject] protected NavigationManager Navigation { get; set; } = default!;

        protected override void OnInitialized()
        {
            Theme.OnThemeChanged += StateHasChanged;
            L.OnLanguageChanged += StateHasChanged;
        }

        public virtual void Dispose()
        {
            Theme.OnThemeChanged -= StateHasChanged;
            L.OnLanguageChanged -= StateHasChanged;
            GC.SuppressFinalize(this);
        }
    }
}