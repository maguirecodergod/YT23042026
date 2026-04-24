using System.Text.Json.Serialization;

namespace HTT.BlazorWasm.App.Components
{
    public partial class HTTToolTip : HTTComponentBase
    {
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public string Content { get; set; } = string.Empty;
        [Parameter] public string Text { get; set; } = string.Empty;
        [Parameter] public CPositionType Position { get; set; } = CPositionType.Top;
        [Parameter] public string Class { get; set; } = string.Empty;
        [Parameter] public string Style { get; set; } = string.Empty;
        [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object> AdditionalAttributes { get; set; } = new();

        protected bool _visible;
        protected string _activePosition = "top";
        protected ElementReference _tooltipRef;
        protected ElementReference _wrapperRef;
        protected ElementReference _arrowRef;

        protected async Task Show()
        {
            var displayContent = !string.IsNullOrEmpty(Content) ? Content : Text;
            if (string.IsNullOrEmpty(displayContent)) return;
            
            _visible = true;
            StateHasChanged();
            await Task.Delay(10);
            try
            {
                var finalPos = await JS.InvokeAsync<string>(
                    "httTooltip.update",
                    _wrapperRef,
                    _tooltipRef,
                    _arrowRef,
                    Position.ToString().ToLower(),
                    Theme.IsDark
                );
                
                _activePosition = finalPos;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Tooltip Error");
            }
        }

        protected void Hide()
        {
            _visible = false;
        }

        protected string GetPositionClass() => _activePosition;
    }
}