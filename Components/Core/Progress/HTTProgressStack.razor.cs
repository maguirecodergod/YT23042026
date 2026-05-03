using System.Text;

namespace HTT.BlazorWasm.App.Components
{
    /// <summary>
    /// A multi-segment progress bar component.
    /// Useful for showing distribution of data (e.g. storage usage, status breakdown).
    /// </summary>
    public partial class HTTProgressStack : HTTComponentBase
    {
        /// <summary>
        /// List of segments to render.
        /// </summary>
        [Parameter] public List<HTTProgressSegment> Segments { get; set; } = new();

        /// <summary>
        /// Maximum total value. Defaults to sum of segments if not provided.
        /// </summary>
        [Parameter] public double? Max { get; set; }

        /// <summary>
        /// Size of the stack.
        /// </summary>
        [Parameter] public CButtonSize Size { get; set; } = CButtonSize.Medium;

        /// <summary>
        /// Custom CSS class.
        /// </summary>
        [Parameter] public string? Class { get; set; }

        /// <summary>
        /// Custom style.
        /// </summary>
        [Parameter] public string? Style { get; set; }

        protected string CssClass => BuildCssClass();

        private double TotalValue => Segments.Sum(s => s.Value);
        private double ActualMax => Max ?? Math.Max(TotalValue, 100);

        protected string GetSegmentWidth(double value)
        {
            if (ActualMax <= 0) return "0%";
            return $"{(value / ActualMax * 100).ToString("0.##")}%";
        }

        private string BuildCssClass()
        {
            var sb = new StringBuilder("htt-progress-stack");
            sb.Append($" htt-progress-stack--{Size.ToString().ToLower()}");

            if (!string.IsNullOrWhiteSpace(Class))
                sb.Append($" {Class}");

            return sb.ToString().Trim();
        }
    }
}
