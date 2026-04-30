using Microsoft.AspNetCore.Components;
using System.Text;

namespace HTT.BlazorWasm.App.Components
{
    public partial class HTTBreadcrumb : HTTComponentBase
    {
        [Parameter] public IEnumerable<HTTBreadcrumbItem>? Items { get; set; }
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public string SeparatorIcon { get; set; } = "bi bi-chevron-right";
        [Parameter] public RenderFragment? SeparatorTemplate { get; set; }
        [Parameter] public string? Class { get; set; }
        [Parameter] public string? Style { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object>? AdditionalAttributes { get; set; }

        protected string CssClass => BuildCssClass();

        private string BuildCssClass()
        {
            var sb = new StringBuilder();
            sb.Append("htt-breadcrumb-container");

            if (!string.IsNullOrWhiteSpace(Class))
                sb.Append($" {Class}");

            return sb.ToString().Trim();
        }
    }
}
