namespace HTT.BlazorWasm.App.Contracts
{
    /// <summary>
    /// Represents a single segment within an HTTProgressStack.
    /// </summary>
    public class HTTProgressSegment
    {
        /// <summary>
        /// The value of this segment. The total value of all segments determines the bar's full state.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Color variant for this segment.
        /// </summary>
        public CButtonVariant Variant { get; set; } = CButtonVariant.Primary;

        /// <summary>
        /// Optional localization key for the label.
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// Optional localization key for the tooltip.
        /// </summary>
        public string? ToolTip { get; set; }
    }
}
