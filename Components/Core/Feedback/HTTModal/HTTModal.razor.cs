using System.Text;

namespace HTT.BlazorWasm.App.Components
{
    public partial class HTTModal : HTTComponentBase
    {
        #region Parameters

        [Parameter] public bool Visible { get; set; }
        [Parameter] public EventCallback<bool> VisibleChanged { get; set; }

        [Parameter] public string? Title { get; set; }
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public RenderFragment? Body { get; set; }
        [Parameter] public RenderFragment? Footer { get; set; }

        [Parameter] public bool Closable { get; set; } = true;
        [Parameter] public bool MaskClosable { get; set; } = true;
        [Parameter] public bool ShowMask { get; set; } = true;

        [Parameter] public bool Centered { get; set; } = true;
        [Parameter] public bool Fullscreen { get; set; }
        [Parameter] public string? Width { get; set; }
        [Parameter] public bool DestroyOnClose { get; set; } = true;
        [Parameter] public bool Loading { get; set; }

        [Parameter] public string? Class { get; set; }
        [Parameter] public string? Style { get; set; }

        #endregion

        private bool _isVisible;
        private bool _shouldRender;
        private readonly string _modalId = $"modal-{Guid.NewGuid():N}";
        private IJSObjectReference? _jsModule;

        protected override async Task OnParametersSetAsync()
        {
            if (Visible != _isVisible)
            {
                _isVisible = Visible;
                if (_isVisible)
                {
                    _shouldRender = true;
                    await OnOpen();
                }
                else
                {
                    await OnClose();
                    if (DestroyOnClose) _shouldRender = false;
                }
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _jsModule = await JS.InvokeAsync<IJSObjectReference>("import", "./Components/Core/Feedback/HTTModal/HTTModal.razor.js");
            }
        }

        private async Task OnOpen()
        {
            if (_jsModule != null)
            {
                await _jsModule.InvokeVoidAsync("onModalOpen", _modalId);
            }
        }

        private async Task OnClose()
        {
            if (_jsModule != null)
            {
                await _jsModule.InvokeVoidAsync("onModalClose", _modalId);
            }
        }

        private async Task CloseAsync()
        {
            Visible = false;
            await VisibleChanged.InvokeAsync(false);
            StateHasChanged();
        }

        private async Task HandleMaskClick()
        {
            if (MaskClosable && !Loading)
            {
                await CloseAsync();
            }
        }

        private string BuildWrapperClass()
        {
            var sb = new StringBuilder("htt-modal-wrapper");
            if (!string.IsNullOrEmpty(Theme.ThemeClass)) sb.Append($" {Theme.ThemeClass}");
            if (_isVisible) sb.Append(" is-visible");
            if (Centered) sb.Append(" is-centered");
            if (Fullscreen) sb.Append(" is-fullscreen");
            if (!string.IsNullOrEmpty(Class)) sb.Append($" {Class}");
            return sb.ToString();
        }

        private string BuildContentStyle()
        {
            if (Fullscreen) return string.Empty;
            return !string.IsNullOrEmpty(Width) ? $"width: {Width};" : string.Empty;
        }

        public override void Dispose()
        {
            if (_isVisible && _jsModule != null)
            {
                _ = _jsModule.InvokeVoidAsync("onModalClose", _modalId);
            }
            base.Dispose();
        }
    }
}
