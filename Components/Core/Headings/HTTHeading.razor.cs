using System.Text;

namespace HTT.BlazorWasm.App.Components
{
    public partial class HTTHeading : HTTComponentBase
    {   
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public CHeadingType Type { get; set; } = CHeadingType.H1;
        [Parameter] public string? Class { get; set; }
        [Parameter] public string? Style { get; set; }
        [Parameter] public string? ToolTip { get; set; }
        [Parameter] public CPositionType ToolTipPosition { get; set; } = CPositionType.Top;
        [Parameter] public CToolTipShapeType ToolTipShape { get; set; } = CToolTipShapeType.Rounded;

        protected string CssClass => BuildCssClass();
        
        private string BuildCssClass()
        {
            var sb = new StringBuilder();
            sb.Append("htt-heading");
            sb.Append($" htt-heading-{Type.ToString().ToLower()}");
            
            if (!string.IsNullOrWhiteSpace(Class))
                sb.Append($" {Class}");

            return sb.ToString().Trim();
        }
    }
}