namespace HTT.BlazorWasm.App.Components
{
    public class HTTLayoutComponentBase : LayoutComponentBase, IDisposable
    {
        [Inject] protected IJSRuntime JS { get; set; } = default!;
        [Inject] protected IHTTThemeService Theme { get; set; } = default!;
        [Inject] protected ILocalizationService L { get; set; } = default!;

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