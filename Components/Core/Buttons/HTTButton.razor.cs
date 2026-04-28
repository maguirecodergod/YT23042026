using System.Text;

namespace HTT.BlazorWasm.App.Components
{
    /// <summary>
    /// An enterprise-grade button component with async handling, permissions, and theme support.
    /// Follows the project's design system and naming conventions.
    /// </summary>
    public partial class HTTButton : HTTComponentBase
    {
        [Inject] protected IPermissionService PermissionService { get; set; } = default!;

        /// <summary>
        /// Text to display on the button. Ignored if ChildContent is provided.
        /// </summary>
        [Parameter] public string? Text { get; set; }

        /// <summary>
        /// Custom content to render inside the button.
        /// </summary>
        [Parameter] public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Callback for click events. If async, auto-manages loading state.
        /// </summary>
        [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }

        /// <summary>
        /// Visual variant of the button.
        /// </summary>
        [Parameter] public CButtonVariant Variant { get; set; } = CButtonVariant.Primary;

        /// <summary>
        /// Size of the button.
        /// </summary>
        [Parameter] public CButtonSize Size { get; set; } = CButtonSize.Medium;

        /// <summary>
        /// Manually disable the button.
        /// </summary>
        [Parameter] public bool Disabled { get; set; }

        /// <summary>
        /// Manually set the loading state.
        /// </summary>
        [Parameter] public bool Loading { get; set; }

        /// <summary>
        /// CSS class for a Bootstrap icon or similar.
        /// </summary>
        [Parameter] public string? Icon { get; set; }

        /// <summary>
        /// Native HTML button type (button, submit, reset).
        /// </summary>
        [Parameter] public CButtonType Type { get; set; } = CButtonType.Button;

        /// <summary>
        /// If true, button stretches to 100% width.
        /// </summary>
        [Parameter] public bool Block { get; set; }

        /// <summary>
        /// If true, applies destructive action styling.
        /// </summary>
        [Parameter] public bool Danger { get; set; }

        /// <summary>
        /// Forces the button to be square and centered, even if ChildContent is provided.
        /// Useful for buttons with badges/dots.
        /// </summary>
        [Parameter] public bool IconOnly { get; set; }

        /// <summary>
        /// Tooltip text displayed on hover.
        /// </summary>
        [Parameter] public string? ToolTip { get; set; }

        /// <summary>
        /// Position of the tooltip relative to the button.
        /// </summary>
        [Parameter] public CPositionType ToolTipPosition { get; set; } = CPositionType.Top;

        /// <summary>
        /// Visual shape of the tooltip.
        /// </summary>
        [Parameter] public CToolTipShapeType ToolTipShape { get; set; } = CToolTipShapeType.Rounded;

        /// <summary>
        /// Permission key required to interact with this button.
        /// </summary>
        [Parameter] public string? Permission { get; set; }

        /// <summary>
        /// Message for a JS confirmation dialog before action.
        /// </summary>
        [Parameter] public string? ConfirmMessage { get; set; }

        /// <summary>
        /// Time in ms to prevent repeat clicks.
        /// </summary>
        [Parameter] public int DebounceInterval { get; set; } = 300;

        /// <summary>
        /// Custom CSS class.
        /// </summary>
        [Parameter] public string? Class { get; set; }

        /// <summary>
        /// Custom inline style.
        /// </summary>
        [Parameter] public string? Style { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object>? AdditionalAttributes { get; set; }

        private bool _internalLoading;
        private bool _hasPermission = true;
        private DateTime _lastClickTime = DateTime.MinValue;

        protected override async Task OnParametersSetAsync()
        {
            if (!string.IsNullOrEmpty(Permission))
            {
                _hasPermission = await PermissionService.HasPermissionAsync(Permission);
            }
        }

        protected async Task HandleClick(MouseEventArgs args)
        {
            if (IsButtonDisabled)
                return;

            // Debounce check
            if ((DateTime.Now - _lastClickTime).TotalMilliseconds < DebounceInterval)
                return;

            _lastClickTime = DateTime.Now;

            // Confirmation check
            if (!string.IsNullOrEmpty(ConfirmMessage))
            {
                var confirmed = await JS.InvokeAsync<bool>("confirm", ConfirmMessage);
                if (!confirmed) return;
            }

            if (OnClick.HasDelegate)
            {
                try
                {
                    _internalLoading = true;
                    StateHasChanged();
                    await OnClick.InvokeAsync(args);
                }
                finally
                {
                    _internalLoading = false;
                    StateHasChanged();
                }
            }
        }

        protected string CssClass => BuildCssClass();

        private string BuildCssClass()
        {
            var sb = new StringBuilder("htt-btn");
            sb.Append($" htt-btn--{Variant.ToString().ToLower()}");
            sb.Append($" htt-btn--{Size.ToString().ToLower()}");

            if (Block) sb.Append(" htt-btn--block");
            if (Danger) sb.Append(" htt-btn--danger");
            if (IsLoading) sb.Append(" htt-btn--loading");
            if (IsButtonDisabled) sb.Append(" htt-btn--disabled");
            if (IsIconOnly) sb.Append(" htt-btn--icon-only");

            if (!string.IsNullOrWhiteSpace(Class))
                sb.Append($" {Class}");

            return sb.ToString().Trim();
        }

        protected bool IsButtonDisabled => Disabled || Loading || _internalLoading || !_hasPermission;
        protected bool IsLoading => Loading || _internalLoading;
        protected bool IsIconOnly => IconOnly || (!string.IsNullOrEmpty(Icon) && string.IsNullOrEmpty(Text) && (ChildContent == null || !HasVisibleContent(ChildContent)));

        private bool HasVisibleContent(RenderFragment? fragment)
        {
            // Simplified check: if IconOnly is manually set, we force it.
            // Otherwise, we check if it's really just an icon.
            return fragment != null;
        }
    }
}
