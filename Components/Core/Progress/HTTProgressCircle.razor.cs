using System.Text;

namespace HTT.BlazorWasm.App.Components
{
    /// <summary>
    /// A premium radial progress component using SVG.
    /// Supports smooth stroke animations and indeterminate states.
    /// </summary>
    public partial class HTTProgressCircle : HTTComponentBase
    {
        /// <summary>
        /// Current progress value.
        /// </summary>
        [Parameter] public double Value { get; set; }

        /// <summary>
        /// Maximum value. Defaults to 100.
        /// </summary>
        [Parameter] public double Max { get; set; } = 100;

        /// <summary>
        /// Size of the circle in pixels.
        /// </summary>
        [Parameter] public int SizePx { get; set; } = 64;

        /// <summary>
        /// Thickness of the stroke.
        /// </summary>
        [Parameter] public int Thickness { get; set; } = 6;

        /// <summary>
        /// Color variant.
        /// </summary>
        [Parameter] public CButtonVariant Variant { get; set; } = CButtonVariant.Primary;

        /// <summary>
        /// If true, displays an infinite rotation animation.
        /// </summary>
        [Parameter] public bool IsIndeterminate { get; set; }

        /// <summary>
        /// Custom CSS class.
        /// </summary>
        [Parameter] public string? Class { get; set; }

        /// <summary>
        /// Custom style.
        /// </summary>
        [Parameter] public string? Style { get; set; }

        protected string CssClass => BuildCssClass();
        
        protected int Radius => (SizePx - Thickness) / 2;
        protected int Circumference => (int)(2 * Math.PI * Radius);
        protected double DashOffset => IsIndeterminate ? 0 : Circumference - (CalculatePercentage() / 100 * Circumference);

        private double CalculatePercentage()
        {
            if (Max <= 0) return 0;
            return Math.Clamp((Value / Max) * 100, 0, 100);
        }

        private string BuildCssClass()
        {
            var sb = new StringBuilder("htt-progress-circle");
            sb.Append($" htt-progress-circle--{Variant.ToString().ToLower()}");
            
            if (IsIndeterminate) sb.Append(" htt-progress-circle--indeterminate");

            if (!string.IsNullOrWhiteSpace(Class))
                sb.Append($" {Class}");

            return sb.ToString().Trim();
        }
    }
}
