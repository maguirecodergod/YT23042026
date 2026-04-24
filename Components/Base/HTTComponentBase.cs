using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using HTT.BlazorWasm.App.Contracts;

namespace HTT.BlazorWasm.App.Components
{
    public class HTTComponentBase : ComponentBase, IDisposable
    {
        [Inject] protected IJSRuntime JS { get; set; } = default!;
        [Inject] protected ILogger<HTTComponentBase> Logger { get; set; } = default!;
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