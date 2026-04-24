using HTT.BlazorWasm.App.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace HTT.BlazorWasm.App.Components
{
    public class HTTLayoutComponentBase : LayoutComponentBase, IDisposable
    {
        [Inject] protected IJSRuntime JS { get; set; } = default!;
        [Inject] protected IHTTThemeService Theme { get; set; } = default!;

        protected override void OnInitialized()
        {
            Theme.OnThemeChanged += StateHasChanged;
        }

        public virtual void Dispose()
        {
            Theme.OnThemeChanged -= StateHasChanged;
            GC.SuppressFinalize(this);
        }
    }
}