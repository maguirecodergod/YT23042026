using System.Text;
namespace HTT.BlazorWasm.App.Components
{
    /// <summary>
    /// An enterprise-grade Label component for forms and UI elements.
    /// Supports icons, required indicators, localization, and informative tooltips.
    /// </summary>
    public partial class HTTLabel : HTTComponentBase
    {
        /// <summary>
        /// The text content of the label. Used when <see cref="ChildContent"/> is null.
        /// </summary>
        [Parameter] public string? Text { get; set; }

        /// <summary>
        /// The ID of the form element this label is associated with (for accessibility).
        /// </summary>
        [Parameter] public string? For { get; set; }

        /// <summary>
        /// If true, displays a required indicator (red asterisk).
        /// </summary>
        [Parameter] public bool Required { get; set; }

        /// <summary>
        /// The size variant of the label. Defaults to <see cref="CSpacingType.MD"/>.
        /// </summary>
        [Parameter] public CSpacingType Size { get; set; } = CSpacingType.MD;

        /// <summary>
        /// Optional Bootstrap Icon class (e.g., "bi-person") to display before the text.
        /// </summary>
        [Parameter] public string? Icon { get; set; }

        /// <summary>
        /// Optional informative text to display in a dedicated info icon next to the label.
        /// </summary>
        [Parameter] public string? Info { get; set; }

        /// <summary>
        /// Optional tooltip text for the entire label element.
        /// </summary>
        [Parameter] public string? ToolTip { get; set; }

        /// <summary>
        /// The position of the tooltip. Defaults to Top.
        /// </summary>
        [Parameter] public CPositionType ToolTipPosition { get; set; } = CPositionType.Top;

        /// <summary>
        /// The shape of the tooltip. Defaults to Rounded.
        /// </summary>
        [Parameter] public CToolTipShapeType ToolTipShape { get; set; } = CToolTipShapeType.Rounded;

        /// <summary>
        /// Custom CSS class to apply to the label element.
        /// </summary>
        [Parameter] public string? Class { get; set; }

        /// <summary>
        /// Custom inline style to apply to the label element.
        /// </summary>
        [Parameter] public string? Style { get; set; }

        /// <summary>
        /// Custom child content for complex label layouts.
        /// </summary>
        [Parameter] public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Captures additional attributes passed to the label element.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object>? AdditionalAttributes { get; set; }

        /// <summary>
        /// Computed CSS class for the label element.
        /// </summary>
        protected string CssClass => BuildCssClass();

        private string BuildCssClass()
        {
            var sb = new StringBuilder();
            sb.Append("htt-label");
            sb.Append($" htt-label--{Size.ToString().ToLower()}");

            if (Required)
                sb.Append(" htt-label--required");

            if (!string.IsNullOrWhiteSpace(Class))
                sb.Append($" {Class}");

            return sb.ToString().Trim();
        }
    }
}