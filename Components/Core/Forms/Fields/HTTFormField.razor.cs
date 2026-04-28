using System.Text;

namespace HTT.BlazorWasm.App.Components
{
    /// <summary>
    /// A high-level form field component that orchestrates Label, Input, and Validation.
    /// Supports multiple layouts, validation states, and descriptive help text.
    /// </summary>
    public partial class HTTFormField : HTTComponentBase
    {
        /// <summary>
        /// The text for the field label.
        /// </summary>
        [Parameter] public string? Label { get; set; }

        /// <summary>
        /// The ID of the input element this field manages. Used for label association.
        /// </summary>
        [Parameter] public string? LabelFor { get; set; }

        /// <summary>
        /// If true, displays a required indicator on the label.
        /// </summary>
        [Parameter] public bool Required { get; set; }

        /// <summary>
        /// Optional icon to display in the label.
        /// </summary>
        [Parameter] public string? LabelIcon { get; set; }

        /// <summary>
        /// Optional info text to display in the label's tooltip.
        /// </summary>
        [Parameter] public string? LabelInfo { get; set; }

        /// <summary>
        /// Descriptive text displayed below the input.
        /// </summary>
        [Parameter] public string? HelpText { get; set; }

        /// <summary>
        /// Validation error message. If set, the field will enter an error state.
        /// </summary>
        [Parameter] public string? Error { get; set; }

        /// <summary>
        /// Layout orientation of the label relative to the input. Defaults to Column.
        /// </summary>
        [Parameter] public CDirectionType Orientation { get; set; } = CDirectionType.Column;

        /// <summary>
        /// Overall size of the field elements. Defaults to MD.
        /// </summary>
        [Parameter] public CSpacingType Size { get; set; } = CSpacingType.MD;

        /// <summary>
        /// Custom CSS class for the field container.
        /// </summary>
        [Parameter] public string? Class { get; set; }

        /// <summary>
        /// Custom inline style for the field container.
        /// </summary>
        [Parameter] public string? Style { get; set; }

        /// <summary>
        /// The input or control content to be managed by this field.
        /// </summary>
        [Parameter] public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Captures additional attributes for the field container.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object>? AdditionalAttributes { get; set; }

        protected string CssClass => BuildCssClass();

        private string BuildCssClass()
        {
            var sb = new StringBuilder();
            sb.Append("htt-form-field");
            sb.Append($" htt-form-field--{Orientation.ToString().ToLower()}");
            sb.Append($" htt-form-field--{Size.ToString().ToLower()}");
            
            if (!string.IsNullOrEmpty(Error))
                sb.Append(" htt-form-field--error");

            if (!string.IsNullOrWhiteSpace(Class))
                sb.Append($" {Class}");

            return sb.ToString().Trim();
        }
    }
}