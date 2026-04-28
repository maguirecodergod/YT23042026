using System.Text;
namespace HTT.BlazorWasm.App.Components
{
    /// <summary>
    /// A container component for grouping related form fields.
    /// Supports titles, descriptions, icons, and collapsible sections.
    /// </summary>
    public partial class HTTFormGroup : HTTComponentBase
    {
        /// <summary>
        /// The title of the form group.
        /// </summary>
        [Parameter] public string? Title { get; set; }

        /// <summary>
        /// Optional description or sub-title for the group.
        /// </summary>
        [Parameter] public string? Description { get; set; }

        /// <summary>
        /// Optional Bootstrap Icon class for the group header.
        /// </summary>
        [Parameter] public string? Icon { get; set; }

        /// <summary>
        /// If true, the group can be expanded or collapsed by the user.
        /// </summary>
        [Parameter] public bool Collapsible { get; set; }

        /// <summary>
        /// The current collapse state. Defaults to Expanded.
        /// </summary>
        [Parameter] public bool IsCollapsed { get; set; }

        /// <summary>
        /// If true, displays the group within a bordered card. Defaults to true.
        /// </summary>
        [Parameter] public bool Bordered { get; set; } = true;

        /// <summary>
        /// Custom CSS class for the group container.
        /// </summary>
        [Parameter] public string? Class { get; set; }

        /// <summary>
        /// Custom inline style for the group container.
        /// </summary>
        [Parameter] public string? Style { get; set; }

        /// <summary>
        /// The fields or content inside the group.
        /// </summary>
        [Parameter] public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Captures additional attributes for the group container.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object>? AdditionalAttributes { get; set; }

        protected string CssClass => BuildCssClass();

        private string BuildCssClass()
        {
            var sb = new StringBuilder();
            sb.Append("htt-form-group");
            
            if (Bordered)
                sb.Append(" htt-form-group--bordered");
            
            if (Collapsible)
                sb.Append(" htt-form-group--collapsible");

            if (IsCollapsed)
                sb.Append(" htt-form-group--collapsed");

            if (!string.IsNullOrWhiteSpace(Class))
                sb.Append($" {Class}");

            return sb.ToString().Trim();
        }

        protected void ToggleCollapse()
        {
            if (Collapsible)
            {
                IsCollapsed = !IsCollapsed;
                StateHasChanged();
            }
        }
    }
}