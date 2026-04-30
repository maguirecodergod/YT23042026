using System.Text;
using HTT.BlazorWasm.App.Services;

namespace HTT.BlazorWasm.App.Components
{
    public partial class HTTSidebar : HTTComponentBase, IAsyncDisposable
    {
        [Inject] private IPermissionService PermissionService { get; set; } = default!;

        #region Parameters
        [Parameter] public string? Id { get; set; }
        [Parameter] public CPositionType Position { get; set; } = CPositionType.Left;

        [Parameter] public bool Visible { get; set; } = true;
        [Parameter] public EventCallback<bool> VisibleChanged { get; set; }

        [Parameter] public string Size { get; set; } = "280px";
        [Parameter] public string? MinSize { get; set; }
        [Parameter] public string? MaxSize { get; set; }

        [Parameter] public bool Resizable { get; set; }
        [Parameter] public bool Overlay { get; set; }
        [Parameter] public bool Fixed { get; set; }
        [Parameter] public bool Collapsible { get; set; }
        [Parameter] public bool Collapsed { get; set; }
        [Parameter] public EventCallback<bool> CollapsedChanged { get; set; }

        [Parameter] public string? PersistenceId { get; set; }
        [Parameter] public string? RequiredPermission { get; set; }

        [Parameter] public RenderFragment? Header { get; set; }
        [Parameter] public RenderFragment? Body { get; set; }
        [Parameter] public RenderFragment? Footer { get; set; }
        [Parameter] public RenderFragment? ChildContent { get; set; }

        [Parameter] public string? Class { get; set; }
        [Parameter] public string? Style { get; set; }
        #endregion

        #region State
        private string _currentSize = string.Empty;
        private bool _isResizing;
        private bool _hasPermission = true;
        private DotNetObjectReference<HTTSidebar>? _dotNetRef;
        private IJSObjectReference? _module;
        private IJSObjectReference? _jsInstance;
        private ElementReference _sidebarElement;

        protected string CssClass => BuildCssClass();
        protected string CssStyle => BuildCssStyle();
        #endregion

        protected override async Task OnInitializedAsync()
        {
            _currentSize = Size;
            
            await CheckPermissions();

            if (!string.IsNullOrEmpty(PersistenceId))
            {
                var savedSize = await JS.InvokeAsync<string?>("localStorage.getItem", $"htt-sidebar-size-{PersistenceId}");
                if (!string.IsNullOrEmpty(savedSize)) _currentSize = savedSize;

                var savedCollapsed = await JS.InvokeAsync<string?>("localStorage.getItem", $"htt-sidebar-collapsed-{PersistenceId}");
                if (!string.IsNullOrEmpty(savedCollapsed)) Collapsed = bool.Parse(savedCollapsed);
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            await CheckPermissions();
        }

        private async Task CheckPermissions()
        {
            if (!string.IsNullOrEmpty(RequiredPermission))
            {
                _hasPermission = await PermissionService.HasPermissionAsync(RequiredPermission);
            }
            else
            {
                _hasPermission = true;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && Resizable)
            {
                _dotNetRef = DotNetObjectReference.Create(this);
                _module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/components/sidebar.js");
                _jsInstance = await _module.InvokeAsync<IJSObjectReference>("initResizer", _dotNetRef, _sidebarElement, Position.ToString());
            }
        }

        private async Task ToggleCollapse()
        {
            Collapsed = !Collapsed;
            await CollapsedChanged.InvokeAsync(Collapsed);

            if (!string.IsNullOrEmpty(PersistenceId))
            {
                await JS.InvokeVoidAsync("localStorage.setItem", $"htt-sidebar-collapsed-{PersistenceId}", Collapsed.ToString().ToLower());
            }
        }

        [JSInvokable]
        public async Task CloseOverlay()
        {
            if (Overlay)
            {
                Visible = false;
                await VisibleChanged.InvokeAsync(false);
                StateHasChanged();
            }
        }

        [JSInvokable]
        public async Task UpdateSize(string newSize)
        {
            _currentSize = newSize;
            StateHasChanged();

            if (!string.IsNullOrEmpty(PersistenceId))
            {
                await JS.InvokeVoidAsync("localStorage.setItem", $"htt-sidebar-size-{PersistenceId}", _currentSize);
            }
        }

        private string BuildCssClass()
        {
            var sb = new StringBuilder("htt-sidebar");
            sb.Append($" htt-sidebar-{Position.ToString().ToLower()}");
            
            // Standard state classes for theme.css animation engine
            if (Visible)
            {
                sb.Append(" is-visible open");
                if (Collapsed) sb.Append(" collapsed is-collapsed");
                else sb.Append(" expanded");
            }
            else
            {
                sb.Append(" hidden");
            }

            if (Overlay) sb.Append(" is-overlay");
            if (Fixed) sb.Append(" is-fixed");
            if (Resizable) sb.Append(" is-resizable");
            if (_isResizing) sb.Append(" is-resizing");

            if (!string.IsNullOrWhiteSpace(Class)) sb.Append($" {Class}");
            
            return sb.ToString();
        }

        private string BuildCssStyle()
        {
            var styles = new List<string>();
            
            bool isVertical = Position == CPositionType.Left || Position == CPositionType.Right;
            string sizeVar = isVertical ? "--htt-sidebar-width" : "--htt-sidebar-height";
            
            styles.Add($"{sizeVar}: {_currentSize}");
            
            if (!string.IsNullOrEmpty(MinSize)) styles.Add($"--htt-sidebar-min-size: {MinSize}");
            if (!string.IsNullOrEmpty(MaxSize)) styles.Add($"--htt-sidebar-max-size: {MaxSize}");

            if (!string.IsNullOrWhiteSpace(Style)) styles.Add(Style);

            return string.Join("; ", styles);
        }

        public async ValueTask DisposeAsync()
        {
            if (_jsInstance != null)
            {
                await _jsInstance.InvokeVoidAsync("dispose");
                await _jsInstance.DisposeAsync();
            }

            if (_module != null)
            {
                await _module.DisposeAsync();
            }

            _dotNetRef?.Dispose();
        }
    }
}
