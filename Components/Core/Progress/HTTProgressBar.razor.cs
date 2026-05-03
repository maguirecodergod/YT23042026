using System.Text;

namespace HTT.BlazorWasm.App.Components
{
    /// <summary>
    /// A premium progress bar component following HTT design system.
    /// Supports multiple modes: Line, Striped, Animated, Gradient, Buffered, Stepped.
    /// </summary>
    public partial class HTTProgressBar : HTTComponentBase
    {
        /// <summary>
        /// Current progress value.
        /// </summary>
        [Parameter] public double Value { get; set; }

        /// <summary>
        /// Maximum value for calculation. Defaults to 100.
        /// </summary>
        [Parameter] public double Max { get; set; } = 100;

        /// <summary>
        /// Secondary value for buffered mode.
        /// </summary>
        [Parameter] public double BufferValue { get; set; }

        /// <summary>
        /// If true, displays an infinite loading animation.
        /// </summary>
        [Parameter] public bool IsIndeterminate { get; set; }

        /// <summary>
        /// Visual type of the progress bar.
        /// </summary>
        [Parameter] public CHTTProgressType ProgressType { get; set; } = CHTTProgressType.Line;

        /// <summary>
        /// Size of the bar. Reuses CButtonSize.
        /// </summary>
        [Parameter] public CButtonSize Size { get; set; } = CButtonSize.Medium;

        /// <summary>
        /// Color variant. Reuses CButtonVariant.
        /// </summary>
        [Parameter] public CButtonVariant Variant { get; set; } = CButtonVariant.Primary;

        /// <summary>
        /// Optional label to display.
        /// </summary>
        [Parameter] public string? Label { get; set; }

        /// <summary>
        /// If true, displays the percentage label.
        /// </summary>
        [Parameter] public bool ShowPercentage { get; set; }

        /// <summary>
        /// Custom CSS class.
        /// </summary>
        [Parameter] public string? Class { get; set; }

        /// <summary>
        /// Custom style.
        /// </summary>
        [Parameter] public string? Style { get; set; }

        protected string CssClass => BuildCssClass();
        protected string ProgressWidth => IsIndeterminate ? "100%" : $"{CalculatePercentage(Value)}%";
        protected string BufferWidth => $"{CalculatePercentage(BufferValue)}%";

        private double CalculatePercentage(double val)
        {
            if (Max <= 0) return 0;
            var pct = (val / Max) * 100;
            return Math.Clamp(pct, 0, 100);
        }

        private string BuildCssClass()
        {
            var sb = new StringBuilder("htt-progress-bar");
            sb.Append($" htt-progress-bar--{Variant.ToString().ToLower()}");
            sb.Append($" htt-progress-bar--{Size.ToString().ToLower()}");
            sb.Append($" htt-progress-bar--{ProgressType.ToString().ToLower()}");

            if (IsIndeterminate) sb.Append(" htt-progress-bar--indeterminate");
            
            if (!string.IsNullOrWhiteSpace(Class))
                sb.Append($" {Class}");

            return sb.ToString().Trim();
        }

        protected override bool ShouldRender()
        {
            // Optimization for rapid value changes
            return true; 
        }
    }
}
