using HTT.BlazorWasm.App.Helpers;
using Microsoft.JSInterop;

namespace HTT.BlazorWasm.App.Components
{
    public partial class HTTUserPicker : HTTComponentBase
    {
        [Parameter] public List<UserBaseModel> Users { get; set; } = new();
        [Parameter] public EventCallback<List<UserBaseModel>> UsersChanged { get; set; }
        [Parameter] public List<UserBaseModel> SelectedUsers { get; set; } = new();
        [Parameter] public EventCallback<List<UserBaseModel>> SelectedUsersChanged { get; set; }

        [Parameter] public bool Multiple { get; set; }
        [Parameter] public string? Placeholder { get; set; }
        [Parameter] public bool AllowClear { get; set; } = true;
        [Parameter] public bool Disabled { get; set; }
        [Parameter] public bool Loading { get; set; }
        [Parameter] public string Class { get; set; } = string.Empty;
        [Parameter] public string Style { get; set; } = string.Empty;

        // Pagination Parameters
        [Parameter] public bool EnablePagination { get; set; } = false;
        [Parameter] public int PageSize { get; set; } = 10;
        [Parameter] public int MaxTagsToShow { get; set; } = 3;
        [Parameter] public bool ShowSelectedPanel { get; set; } = false;
        [Parameter] public Func<UserPickerLoadRequest, Task<UserPickerLoadResponse>>? LoadData { get; set; }

        private bool _isOpen;
        private string _searchText = string.Empty;
        private List<UserBaseModel> _filteredUsers = new();
        private List<UserBaseModel> _selectedUsers = new();
        private int _activeIndex = -1;
        private bool _isLoading;
        private bool _isFetchingMore;
        private int _currentPage = 1;
        private bool _hasMore = true;

        private ElementReference _pickerContainer;
        private HTTInput<string> _searchInputComponent = default!;
        private ElementReference _listContainer;
        private IJSObjectReference? _jsModule;
        private DotNetObjectReference<HTTUserPicker>? _dotNetHelper;

        protected override void OnInitialized()
        {
            _selectedUsers = SelectedUsers ?? new List<UserBaseModel>();
            _filteredUsers = Users;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _jsModule = await JS.InvokeAsync<IJSObjectReference>("import", "./Components/Core/Forms/Selectors/HTTUserPicker/HTTUserPicker.razor.js");
                _dotNetHelper = DotNetObjectReference.Create(this);
                await _jsModule.InvokeVoidAsync("initializeClickOutside", _pickerContainer, _dotNetHelper);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _dotNetHelper?.Dispose();
            _jsModule?.DisposeAsync();
        }

        protected override void OnParametersSet()
        {
            if (SelectedUsers != null && !SelectedUsers.SequenceEqual(_selectedUsers))
            {
                _selectedUsers = SelectedUsers.ToList();
            }

            if (!EnablePagination)
            {
                FilterUsers();
            }
            else
            {
                _filteredUsers = Users;
            }

            _isLoading = Loading;
        }

        private string BuildCssClass()
        {
            return new HTTClassBuilder("htt-user-picker")
                .AddClass("is-open", _isOpen)
                .AddClass("is-multiple", Multiple)
                .AddClass("is-disabled", Disabled)
                .AddClass(Class)
                .Build();
        }

        private async Task ToggleDropdown()
        {
            if (Disabled) return;

            _isOpen = !_isOpen;
            if (_isOpen)
            {
                if (EnablePagination && LoadData != null && !Users.Any())
                {
                    await RefreshDataAsync();
                }

                if (EnablePagination && _jsModule != null)
                {
                    // Delay to ensure list is rendered
                    await Task.Delay(100);
                    await _jsModule.InvokeVoidAsync("initializeScrollPaging", _listContainer, _dotNetHelper);
                }

                _activeIndex = -1;
                _searchText = string.Empty;
                if (!EnablePagination) _filteredUsers = Users;
                
                await Task.Yield();
                try
                {
                    if (_searchInputComponent != null)
                        await _searchInputComponent.FocusAsync();
                }
                catch { /* Ignore if input not ready */ }
            }
        }

        private async Task RefreshDataAsync()
        {
            if (LoadData == null) return;

            _isLoading = true;
            _currentPage = 1;
            _hasMore = true;

            try
            {
                var response = await LoadData(new UserPickerLoadRequest
                {
                    SearchText = _searchText,
                    PageIndex = _currentPage,
                    PageSize = PageSize
                });

                Users = response.Items;
                await UsersChanged.InvokeAsync(Users);
                _filteredUsers = Users;
                _hasMore = Users.Count < response.TotalCount;
            }
            finally
            {
                _isLoading = false;
            }
        }

        [JSInvokable]
        public async Task LoadMoreItems()
        {
            if (_isFetchingMore || !_hasMore || LoadData == null) return;

            _isFetchingMore = true;
            StateHasChanged();

            try
            {
                _currentPage++;
                var response = await LoadData(new UserPickerLoadRequest
                {
                    SearchText = _searchText,
                    PageIndex = _currentPage,
                    PageSize = PageSize
                });

                if (response.Items.Any())
                {
                    Users.AddRange(response.Items);
                    await UsersChanged.InvokeAsync(Users);
                    _filteredUsers = Users;
                    _hasMore = Users.Count < response.TotalCount;
                }
                else
                {
                    _hasMore = false;
                }
            }
            finally
            {
                _isFetchingMore = false;
                StateHasChanged();
            }
        }

        [JSInvokable]
        public void CloseDropdown()
        {
            _isOpen = false;
            StateHasChanged();
        }

        private async Task HandleSearchInput(string? value)
        {
            _searchText = value ?? string.Empty;
            
            if (EnablePagination && LoadData != null)
            {
                await RefreshDataAsync();
            }
            else
            {
                FilterUsers();
            }

            _activeIndex = -1;
            StateHasChanged();
        }

        private void FilterUsers()
        {
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                _filteredUsers = Users;
            }
            else
            {
                var query = _searchText.ToLower();
                _filteredUsers = Users.Where(u =>
                    (u.FullName?.ToLower().Contains(query) ?? false) ||
                    (u.Email?.ToLower().Contains(query) ?? false) ||
                    (u.UserName?.ToLower().Contains(query) ?? false)
                ).ToList();
            }
        }

        private bool IsAllSelected => _filteredUsers.Any() && _filteredUsers.All(u => _selectedUsers.Any(s => s.Id == u.Id));
        private bool IsSomeSelected => !IsAllSelected && _filteredUsers.Any(u => _selectedUsers.Any(s => s.Id == u.Id));

        private async Task ToggleAll()
        {
            if (Disabled || !Multiple) return;

            if (IsAllSelected)
            {
                await UnselectAll();
            }
            else
            {
                await SelectAll();
            }
        }

        private async Task SelectUser(UserBaseModel user)
        {
            if (Disabled) return;

            if (Multiple)
            {
                var existing = _selectedUsers.FirstOrDefault(u => u.Id == user.Id);
                if (existing != null)
                {
                    _selectedUsers.Remove(existing);
                }
                else
                {
                    _selectedUsers.Add(user);
                }
            }
            else
            {
                _selectedUsers.Clear();
                _selectedUsers.Add(user);
                _isOpen = false;
            }

            await SelectedUsersChanged.InvokeAsync(_selectedUsers);
            StateHasChanged();
        }

        private async Task SelectAll()
        {
            if (Disabled || !Multiple) return;

            foreach (var user in _filteredUsers)
            {
                if (!_selectedUsers.Any(u => u.Id == user.Id))
                {
                    _selectedUsers.Add(user);
                }
            }

            await SelectedUsersChanged.InvokeAsync(_selectedUsers);
            StateHasChanged();
        }

        private async Task UnselectAll()
        {
            if (Disabled || !Multiple) return;

            foreach (var user in _filteredUsers)
            {
                var existing = _selectedUsers.FirstOrDefault(u => u.Id == user.Id);
                if (existing != null)
                {
                    _selectedUsers.Remove(existing);
                }
            }

            await SelectedUsersChanged.InvokeAsync(_selectedUsers);
            StateHasChanged();
        }

        private async Task RemoveUser(UserBaseModel user)
        {
            _selectedUsers.Remove(user);
            await SelectedUsersChanged.InvokeAsync(_selectedUsers);
            StateHasChanged();
        }

        private async Task ClearSelection()
        {
            _selectedUsers.Clear();
            await SelectedUsersChanged.InvokeAsync(_selectedUsers);
            StateHasChanged();
        }

        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (Disabled) return;

            if (e.Key == "ArrowDown")
            {
                if (!_isOpen) await ToggleDropdown();
                else _activeIndex = (_activeIndex + 1) % Math.Max(1, _filteredUsers.Count);
            }
            else if (e.Key == "ArrowUp")
            {
                if (!_isOpen) await ToggleDropdown();
                else _activeIndex = (_activeIndex - 1 + _filteredUsers.Count) % Math.Max(1, _filteredUsers.Count);
            }
            else if (e.Key == "Enter" && _activeIndex >= 0 && _activeIndex < _filteredUsers.Count)
            {
                await SelectUser(_filteredUsers[_activeIndex]);
            }
            else if (e.Key == "Escape")
            {
                _isOpen = false;
            }
        }

        private string GetAvatarText(UserBaseModel user)
        {
            if (user == null) return "U";

            if (!string.IsNullOrEmpty(user.LastName))
            {
                return user.LastName.Substring(0, 1).ToUpper();
            }
            if (!string.IsNullOrEmpty(user.FullName))
            {
                var parts = user.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    return parts.Last().Substring(0, 1).ToUpper();
                }
            }
            if (!string.IsNullOrEmpty(user.FirstName))
            {
                return user.FirstName.Substring(0, 1).ToUpper();
            }
            if (!string.IsNullOrEmpty(user.UserName))
            {
                return user.UserName.Substring(0, 1).ToUpper();
            }

            return "U";
        }
    }
}
