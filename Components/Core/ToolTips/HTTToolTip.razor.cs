namespace HTT.BlazorWasm.App.Components
{
    public partial class HTTToolTip : HTTComponentBase
    {
        /// <summary>
        /// Tooltip child content
        /// </summary>
        [Parameter] public RenderFragment? ChildContent { get; set; }
        /// <summary>
        /// Tooltip content
        /// </summary>
        [Parameter] public string Content { get; set; } = string.Empty;
        /// <summary>
        /// Tooltip preferred position
        /// </summary>
        [Parameter] public CPositionType Position { get; set; } = CPositionType.Top;

        protected bool _visible;
        protected string PositionClass = "top";
        protected string _inlineStyle = string.Empty;

        protected ElementReference _tooltipRef;
        protected ElementReference _wrapperRef;

        protected async Task Show()
        {
            _visible = true;
            await Task.Delay(1);

            var result = await JS.InvokeAsync<PositionResult>(
                "httTooltip.calculatePosition",
                _wrapperRef,
                _tooltipRef,
                Position.ToString().ToLower()
            );

            PositionClass = result.Position;
            _inlineStyle = $"top:{result.Top}px; left:{result.Left}px;";
            StateHasChanged();
        }

        protected void Hide()
        {
            _visible = false;
        }

        public class PositionResult
        {
            public string Position { get; set; } = "top";
            public double Top { get; set; }
            public double Left { get; set; }
        }
    }
}