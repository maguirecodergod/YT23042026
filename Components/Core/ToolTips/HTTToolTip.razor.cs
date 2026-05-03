namespace HTT.BlazorWasm.App.Components
{
    public partial class HTTToolTip : HTTComponentBase
    {
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public RenderFragment? Trigger { get; set; }
        [Parameter] public string Text { get; set; } = string.Empty;
        [Parameter] public CPositionType Position { get; set; } = CPositionType.Top;
        [Parameter] public CToolTipShapeType Shape { get; set; } = CToolTipShapeType.Rounded;
        [Parameter] public string Class { get; set; } = string.Empty;
        [Parameter] public string Style { get; set; } = string.Empty;
        [Parameter] public bool Plain { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object> AdditionalAttributes { get; set; } = new();

        protected string ToolTipCssClass => BuildToolTipClass();

        private string BuildToolTipClass()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("htt-tooltip");
            sb.Append($" htt-tooltip-{Shape.ToString().ToLower()}");
            if (Plain) sb.Append(" htt-tooltip--plain");
            if (_visible) sb.Append(" visible");
            return sb.ToString();
        }

        protected bool _visible;
        protected string _activePosition = "top";
        protected ElementReference _tooltipRef;
        protected ElementReference _wrapperRef;
        protected ElementReference _arrowRef;

        private System.Threading.Timer? _hideTimer;

        protected async Task Show()
        {
            _hideTimer?.Dispose();
            _hideTimer = null;

            var hasDisplayContent = ChildContent != null || !string.IsNullOrEmpty(Text);
            if (!hasDisplayContent) return;

            if (!_visible)
            {
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
        }

        public void HideImmediate()
        {
            _hideTimer?.Dispose();
            _hideTimer = null;
            _visible = false;
            StateHasChanged();
        }

        protected void Hide()
        {
            _hideTimer?.Dispose();
            _hideTimer = new System.Threading.Timer(_ =>
            {
                InvokeAsync(() =>
                {
                    _visible = false;
                    StateHasChanged();
                });
            }, null, 150, System.Threading.Timeout.Infinite);
        }

        public override void Dispose()
        {
            _hideTimer?.Dispose();
            _ = JS.InvokeVoidAsync("httTooltip.hide", _tooltipRef);
            base.Dispose();
        }

        protected string GetPositionClass() => _activePosition;
    }
}