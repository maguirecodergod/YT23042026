using HTT.BlazorWasm.App.Contracts;

namespace HTT.BlazorWasm.App.Layout
{
    public partial class MainLayout : HTTLayoutComponentBase, IAsyncDisposable
    {
        private CCollapseType _sidebarState = CCollapseType.Expanded;
        private bool _isMobile;
        private string _searchQuery = string.Empty;
        private string PageTitle => GetPageTitle();
        private DotNetObjectReference<MainLayout>? _objRef;

        private UserBaseModel _currentUser = new UserBaseModel
        {
            Id = Guid.NewGuid(),
            FullName = "Admin User",
            LastName = "Admin",
            Position = "System Administrator",
            Email = "admin@htt.com",
            PhoneNumber = "+1 234 567 890"
        };

        protected override async Task OnInitializedAsync()
        {
            await L.InitializeAsync();
            // Theme.InitializeAsync() is called in Program.cs before RunAsync()
            // so ThemeClass is already correct on the very first render — zero FOUC.
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _objRef = DotNetObjectReference.Create(this);
                await JS.InvokeVoidAsync("window.httLayout.initResizeListener", _objRef);
                var width = await JS.InvokeAsync<double>("window.httLayout.getWindowWidth");
                UpdateStateFromWidth(width);

                StateHasChanged();
            }
        }

        [JSInvokable]
        public void OnWindowResize(double width)
        {
            UpdateStateFromWidth(width);
            StateHasChanged();
        }

        private void UpdateStateFromWidth(double width)
        {
            _isMobile = width < 992;
            if (_isMobile) _sidebarState = CCollapseType.Hidden;
            else if (width < 1280) _sidebarState = CCollapseType.Collapsed;
            else _sidebarState = CCollapseType.Expanded;
        }

        private void ToggleSidebar()
        {
            _sidebarState = _sidebarState switch
            {
                CCollapseType.Expanded => CCollapseType.Collapsed,
                CCollapseType.Collapsed => CCollapseType.Hidden,
                CCollapseType.Hidden => CCollapseType.Expanded,
                _ => CCollapseType.Expanded
            };
            StateHasChanged();
        }

        private string GetToggleIcon() => _sidebarState switch
        {
            CCollapseType.Expanded => "bi-chevron-left",
            CCollapseType.Collapsed => "bi-list",
            CCollapseType.Hidden => "bi-layout-sidebar-inset",
            _ => "bi-list"
        };

        private void OnVisibleChanged(bool visible)
        {
            if (!visible) _sidebarState = CCollapseType.Hidden;
            else if (_sidebarState == CCollapseType.Hidden) _sidebarState = CCollapseType.Expanded;
            StateHasChanged();
        }

        private void OnCollapsedChanged(bool collapsed)
        {
            _sidebarState = collapsed ? CCollapseType.Collapsed : CCollapseType.Expanded;
            StateHasChanged();
        }

        private void CloseSidebar() => _sidebarState = CCollapseType.Hidden;

        private string GetPageTitle()
        {
            var path = new Uri(Nav.Uri).AbsolutePath.TrimStart('/');
            return path switch
            {
                "" => L["Layout.Sidebar.Items.Dashboard"],
                "analytics" => L["Layout.Sidebar.Items.Analytics"],
                "projects" => L["Layout.Sidebar.Items.Projects"],
                "reports" => L["Layout.Sidebar.Items.Reports"],
                "users" => L["Layout.Sidebar.Items.Users"],
                "billing" => L["Layout.Sidebar.Items.Billing"],
                "integrations" => L["Layout.Sidebar.Items.Integrations"],
                "audit" => L["Layout.Sidebar.Items.AuditLog"],
                "settings" => L["Layout.Sidebar.Items.Settings"],
                "profile" => L["Layout.Topbar.Profile"],
                _ => L["Layout.Title.Default"]
            };
        }

        public async ValueTask DisposeAsync()
        {
            _objRef?.Dispose();
        }
    }
}