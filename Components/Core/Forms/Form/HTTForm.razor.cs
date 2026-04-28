using System.Text;
using Microsoft.AspNetCore.Components.Forms;

namespace HTT.BlazorWasm.App.Components
{
    /// <summary>
    /// An enterprise-grade form component that wraps EditForm and provides integrated validation.
    /// Supports model binding, validation handling, and consistent styling.
    /// </summary>
    public partial class HTTForm : HTTComponentBase
    {
        /// <summary>
        /// The model bound to the form for validation.
        /// </summary>
        [Parameter] public object? Model { get; set; }

        /// <summary>
        /// Callback invoked when the form is submitted and validation passes.
        /// </summary>
        [Parameter] public EventCallback<EditContext> OnValidSubmit { get; set; }

        /// <summary>
        /// Callback invoked when the form is submitted and validation fails.
        /// </summary>
        [Parameter] public EventCallback<EditContext> OnInvalidSubmit { get; set; }

        /// <summary>
        /// Custom CSS class for the form container.
        /// </summary>
        [Parameter] public string? Class { get; set; }

        /// <summary>
        /// Custom inline style for the form container.
        /// </summary>
        [Parameter] public string? Style { get; set; }

        /// <summary>
        /// The fields and content inside the form.
        /// </summary>
        [Parameter] public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Captures additional attributes for the form element.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object>? AdditionalAttributes { get; set; }

        protected string CssClass => BuildCssClass();

        private string BuildCssClass()
        {
            var sb = new StringBuilder();
            sb.Append("htt-form");

            if (!string.IsNullOrWhiteSpace(Class))
                sb.Append($" {Class}");

            return sb.ToString().Trim();
        }
    }
}