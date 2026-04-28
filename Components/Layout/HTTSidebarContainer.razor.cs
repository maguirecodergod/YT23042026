using System.Text;

namespace HTT.BlazorWasm.App.Components
{
    public partial class HTTSidebarContainer : HTTComponentBase
    {
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public string? Class { get; set; }
        [Parameter] public string? Style { get; set; }

        protected string CssClass => BuildCssClass();
        protected string CssStyle => BuildCssStyle();

        private string BuildCssClass()
        {
            var sb = new StringBuilder("htt-sidebar-layout-root");
            if (!string.IsNullOrWhiteSpace(Class)) sb.Append($" {Class}");
            return sb.ToString();
        }

        private string BuildCssStyle()
        {
            return Style ?? string.Empty;
        }
    }
}
