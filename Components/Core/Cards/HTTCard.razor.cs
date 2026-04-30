using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Text;

namespace HTT.BlazorWasm.App.Components
{
    /// <summary>
    /// An enterprise-grade, reusable Card component.
    /// Supports composition, loading state, collapsibility, and design system integration.
    /// </summary>
    public partial class HTTCard : HTTComponentBase
    {
        private readonly string _titleId = $"htt-card-title-{Guid.NewGuid():N}";

        /// <summary>
        /// Simple text title for the card header.
        /// Ignored if <see cref="Header"/> is provided.
        /// </summary>
        [Parameter] public string? Title { get; set; }

        /// <summary>
        /// Main content of the card (equivalent to Body).
        /// </summary>
        [Parameter] public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Custom header content. Overrides <see cref="Title"/>.
        /// </summary>
        [Parameter] public RenderFragment? Header { get; set; }

        /// <summary>
        /// Custom body content. Overrides <see cref="ChildContent"/>.
        /// </summary>
        [Parameter] public RenderFragment? Body { get; set; }

        /// <summary>
        /// Custom footer content.
        /// </summary>
        [Parameter] public RenderFragment? Footer { get; set; }

        /// <summary>
        /// Actions rendered in the top right corner of the header.
        /// </summary>
        [Parameter] public RenderFragment? Actions { get; set; }

        /// <summary>
        /// Toggles the card border. Default is true.
        /// </summary>
        [Parameter] public bool Bordered { get; set; } = true;

        /// <summary>
        /// Applies an elevation effect on hover.
        /// </summary>
        [Parameter] public bool Hoverable { get; set; }

        /// <summary>
        /// Makes the entire card clickable, adding a cursor pointer and click event support.
        /// </summary>
        [Parameter] public bool Clickable { get; set; }

        /// <summary>
        /// Displays a skeleton loader instead of the actual content.
        /// </summary>
        [Parameter] public bool Loading { get; set; }

        /// <summary>
        /// Allows the card to be collapsed by the user.
        /// </summary>
        [Parameter] public bool Collapsible { get; set; }

        /// <summary>
        /// Controls the collapsed state.
        /// </summary>
        [Parameter] public bool Collapsed { get; set; }

        /// <summary>
        /// Callback fired when the collapsed state changes.
        /// </summary>
        [Parameter] public EventCallback<bool> CollapsedChanged { get; set; }

        /// <summary>
        /// Configures the card for a grid layout.
        /// </summary>
        [Parameter] public bool Grid { get; set; }

        /// <summary>
        /// Optional specific width for the card.
        /// </summary>
        [Parameter] public string? Width { get; set; }

        /// <summary>
        /// Optional specific height for the card.
        /// </summary>
        [Parameter] public string? Height { get; set; }

        /// <summary>
        /// Custom CSS classes applied to the main container.
        /// </summary>
        [Parameter] public string? Class { get; set; }

        /// <summary>
        /// Custom inline styles applied to the main container.
        /// </summary>
        [Parameter] public string? Style { get; set; }

        /// <summary>
        /// Event triggered when a clickable card is clicked.
        /// </summary>
        [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object>? AdditionalAttributes { get; set; }

        protected string CssClass => BuildCssClass();
        protected string CssStyle => BuildCssStyle();

        private string BuildCssClass()
        {
            var sb = new StringBuilder("htt-card");

            if (Bordered) sb.Append(" htt-card--bordered");
            if (Hoverable) sb.Append(" htt-card--hoverable");
            if (Clickable) sb.Append(" htt-card--clickable");
            if (Loading) sb.Append(" htt-card--loading");
            if (Grid) sb.Append(" htt-card--grid");
            
            if (!string.IsNullOrWhiteSpace(Class))
                sb.Append($" {Class}");

            return sb.ToString();
        }

        private string BuildCssStyle()
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(Width)) sb.Append($"width: {Width}; ");
            if (!string.IsNullOrWhiteSpace(Height)) sb.Append($"height: {Height}; ");

            if (!string.IsNullOrWhiteSpace(Style))
                sb.Append(Style);

            return sb.ToString();
        }

        protected async Task HandleClick(MouseEventArgs args)
        {
            if (Clickable && OnClick.HasDelegate)
            {
                await OnClick.InvokeAsync(args);
            }
        }

        protected async Task ToggleCollapse()
        {
            if (Collapsible)
            {
                Collapsed = !Collapsed;
                if (CollapsedChanged.HasDelegate)
                {
                    await CollapsedChanged.InvokeAsync(Collapsed);
                }
            }
        }
    }
}
