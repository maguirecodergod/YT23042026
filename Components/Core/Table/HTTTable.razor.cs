namespace HTT.BlazorWasm.App.Components
{
    public partial class HTTTable<TItem> : HTTComponentBase
    {
        [Parameter] public IEnumerable<TItem>? Items { get; set; }
        [Parameter] public Func<TableState<TItem>, Task<(IEnumerable<TItem> Items, int TotalCount)>>? LoadDataAsync { get; set; }

        [Parameter] public RenderFragment? ColumnsFragment { get; set; }

        [Parameter] public List<HTTColumn<TItem>> Columns { get; set; } = new();
        private List<HTTColumn<TItem>> _childColumns = new();

        internal void AddColumn(HTTColumn<TItem> column)
        {
            if (!_childColumns.Contains(column))
            {
                _childColumns.Add(column);

                // Initialize state from column parameters if not already in state
                if (!_state.ColumnVisibility.ContainsKey(column.Id))
                    _state.ColumnVisibility[column.Id] = column.Visible;

                if (!_state.ColumnWidths.ContainsKey(column.Id) && !string.IsNullOrEmpty(column.Width))
                    _state.ColumnWidths[column.Id] = column.Width;

                StateHasChanged();
            }
        }

        private IEnumerable<HTTColumn<TItem>> EffectiveColumns => _childColumns.Any() ? _childColumns : Columns;

        [Parameter] public Func<TItem, object>? KeySelector { get; set; }

        [Parameter] public RenderFragment? ActionsTemplate { get; set; }
        [Parameter] public RenderFragment<TItem>? RowActionsTemplate { get; set; }
        [Parameter] public RenderFragment? EmptyTemplate { get; set; }
        [Parameter] public RenderFragment? LoadingTemplate { get; set; }
        [Parameter] public RenderFragment? SidebarFilterTemplate { get; set; }
        [Parameter] public RenderFragment? SidebarFooterTemplate { get; set; }

        [Parameter] public CTableSelectionMode SelectionMode { get; set; } = CTableSelectionMode.None;
        [Parameter] public bool EnableSidebarFilter { get; set; } = false;
        [Parameter] public bool EnableVirtualization { get; set; } = false;
        [Parameter] public bool ShowPagination { get; set; } = true;
        [Parameter] public List<int> PageSizeOptions { get; set; } = new() { 10, 20, 50, 100 };

        [Parameter] public EventCallback<TItem> OnRowClick { get; set; }
        [Parameter] public EventCallback<TItem> OnRowDoubleClick { get; set; }
        [Parameter] public EventCallback<TableSortState> OnSortChanged { get; set; }
        [Parameter] public EventCallback<int> OnPageChanged { get; set; }
        [Parameter] public EventCallback<Dictionary<string, object>> OnFilterChanged { get; set; }
        [Parameter] public EventCallback<HashSet<TItem>> OnSelectionChanged { get; set; }

        [Parameter] public string Style { get; set; } = string.Empty;
        [Parameter] public string Id { get; set; } = Guid.NewGuid().ToString("N");

        [Parameter] public string SearchText { get; set; } = string.Empty;
        [Parameter] public EventCallback<string> SearchTextChanged { get; set; }
        [Parameter] public string SearchPlaceholder { get; set; } = "Search...";
        [Parameter] public bool ShowSearch { get; set; } = true;

        [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }

        private TableState<TItem> _state = new();
        private IEnumerable<TItem> _currentItems = Enumerable.Empty<TItem>();
        private bool _isLoading = false;
        private bool _showColumnSelector = false;
        private bool _showSidebar = false;
        private string _radioGroupName = Guid.NewGuid().ToString("N");

        public bool IsLoading => _isLoading;
        public int PageIndex => _state.PageIndex;
        public int PageSize => _state.PageSize;
        public int TotalCount => _state.TotalCount;
        public int TotalPages => (int)Math.Ceiling((double)_state.TotalCount / _state.PageSize);
        public IEnumerable<TItem> CurrentItems => _currentItems;
        public IEnumerable<HTTColumn<TItem>> VisibleColumns => EffectiveColumns.Where(c => IsColumnVisible(c));

        private bool IsColumnVisible(HTTColumn<TItem> col)
        {
            if (_state.ColumnVisibility.TryGetValue(col.Id, out var visible))
                return visible;
            return col.Visible;
        }

        private string? GetColumnWidth(HTTColumn<TItem> col)
        {
            if (_state.ColumnWidths.TryGetValue(col.Id, out var width))
                return width;
            return col.Width;
        }

        public bool IsAllSelected => _currentItems.Any() && _currentItems.All(i => _state.SelectedItems.Contains(i));
        public bool IsIndeterminate => _currentItems.Any() && _state.SelectedItems.Any() && !_currentItems.All(i => _state.SelectedItems.Contains(i));

        protected override async Task OnInitializedAsync()
        {
            if (PageSizeOptions.Any() && !_state.PageSize.Equals(PageSizeOptions.First()))
            {
                _state.PageSize = PageSizeOptions.First();
            }
            await RefreshAsync();
        }

        public async Task RefreshAsync()
        {
            _isLoading = true;
            await InvokeAsync(StateHasChanged);

            try
            {
                if (LoadDataAsync != null)
                {
                    var result = await LoadDataAsync(_state);
                    _currentItems = result.Items ?? Enumerable.Empty<TItem>();
                    _state.TotalCount = result.TotalCount;
                }
                else if (Items != null)
                {
                    ApplyLocalState();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error refreshing table data");
            }
            finally
            {
                _isLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        private void ApplyLocalState()
        {
            var query = Items?.AsQueryable();
            if (query == null)
            {
                _currentItems = Enumerable.Empty<TItem>();
                _state.TotalCount = 0;
                return;
            }

            // Local Sorting
            if (_state.Sorts.Any())
            {
                var sort = _state.Sorts.First();
                var col = EffectiveColumns.FirstOrDefault(c => c.Id == sort.ColumnId);
                if (col != null && col.CompiledField != null)
                {
                    if (sort.Direction == SortDirection.Ascending)
                        query = query.OrderBy(col.CompiledField).AsQueryable();
                    else if (sort.Direction == SortDirection.Descending)
                        query = query.OrderByDescending(col.CompiledField).AsQueryable();
                }
            }

            _state.TotalCount = query.Count();

            // Local Pagination
            if (ShowPagination && !EnableVirtualization)
            {
                _currentItems = query.Skip((_state.PageIndex - 1) * _state.PageSize).Take(_state.PageSize).ToList();
            }
            else
            {
                _currentItems = query.ToList();
            }
        }

        #region Actions

        private void ToggleSidebar()
        {
            _showSidebar = !_showSidebar;
        }

        private void ToggleColumnSelector()
        {
            _showColumnSelector = !_showColumnSelector;
        }

        private void ToggleColumnVisibility(HTTColumn<TItem> col, object? isVisible)
        {
            if (isVisible is bool b)
            {
                _state.ColumnVisibility[col.Id] = b;
                StateHasChanged();
            }
        }

        private async Task SortColumn(HTTColumn<TItem> col)
        {
            if (!col.Sortable) return;

            var existingSort = _state.Sorts.FirstOrDefault(s => s.ColumnId == col.Id);
            if (existingSort != null)
            {
                if (existingSort.Direction == SortDirection.Ascending)
                    existingSort.Direction = SortDirection.Descending;
                else
                    _state.Sorts.Remove(existingSort);
            }
            else
            {
                _state.Sorts.Clear(); // Support single sort by default
                _state.Sorts.Add(new TableSortState
                {
                    ColumnId = col.Id,
                    FieldName = col.GetFieldName() ?? string.Empty,
                    Direction = SortDirection.Ascending
                });
            }

            if (OnSortChanged.HasDelegate)
                await OnSortChanged.InvokeAsync(_state.Sorts.FirstOrDefault());

            await RefreshAsync();
        }

        private SortDirection GetSortDirection(HTTColumn<TItem> col)
        {
            var sort = _state.Sorts.FirstOrDefault(s => s.ColumnId == col.Id);
            return sort?.Direction ?? SortDirection.None;
        }

        #endregion

        #region Resizing

        private bool _isResizing = false;
        private HTTColumn<TItem>? _resizingColumn;
        private double _startMouseX;
        private double _startColumnWidth;

        private void StartResize(MouseEventArgs e, HTTColumn<TItem> col)
        {
            _isResizing = true;
            _resizingColumn = col;
            _startMouseX = e.ClientX;

            // Try to parse existing width or default to a reasonable value
            var currentWidth = GetColumnWidth(col);
            if (string.IsNullOrEmpty(currentWidth) || !currentWidth.EndsWith("px"))
                _startColumnWidth = 150;
            else
                double.TryParse(currentWidth.Replace("px", ""), out _startColumnWidth);
        }

        private void HandleResize(MouseEventArgs e)
        {
            if (!_isResizing || _resizingColumn == null) return;

            var diffX = e.ClientX - _startMouseX;
            var newWidth = Math.Max(50, _startColumnWidth + diffX); // min 50px

            _state.ColumnWidths[_resizingColumn.Id] = $"{newWidth}px";
            StateHasChanged();
        }

        private void StopResize(MouseEventArgs e)
        {
            _isResizing = false;
            _resizingColumn = null;
        }

        #endregion

        #region Styling & Formatting

        private string GetThClass(HTTColumn<TItem> col)
        {
            var classes = new List<string>();
            if (col.Sortable) classes.Add("htt-table-th-sortable");
            if (col.Align == CColumnAlign.Center) classes.Add("text-center");
            if (col.Align == CColumnAlign.Right) classes.Add("text-end");

            if (col.Fixed == CColumnFixedPosition.Left) classes.Add("htt-table-th-fixed-left");
            if (col.Fixed == CColumnFixedPosition.Right) classes.Add("htt-table-th-fixed-right");

            return string.Join(" ", classes);
        }

        private string GetThStyle(HTTColumn<TItem> col)
        {
            var styles = new List<string>();
            var width = GetColumnWidth(col);
            if (!string.IsNullOrEmpty(width)) styles.Add($"width: {width};");
            return string.Join(" ", styles);
        }

        private string GetTdClass(HTTColumn<TItem> col)
        {
            var classes = new List<string>();
            if (col.Align == CColumnAlign.Center) classes.Add("text-center");
            if (col.Align == CColumnAlign.Right) classes.Add("text-end");

            if (col.Fixed == CColumnFixedPosition.Left) classes.Add("htt-table-td-fixed-left");
            if (col.Fixed == CColumnFixedPosition.Right) classes.Add("htt-table-td-fixed-right");

            return string.Join(" ", classes);
        }

        private string GetTdStyle(HTTColumn<TItem> col)
        {
            var styles = new List<string>();
            // Fixed columns usually need calculated Left/Right depending on previous columns
            // For simple setup, we use sticky positioning but z-index matters
            return string.Join(" ", styles);
        }

        private string FormatValue(HTTColumn<TItem> col, TItem item)
        {
            if (col.CompiledField == null) return string.Empty;

            var val = col.CompiledField(item);
            if (val == null) return string.Empty;

            if (!string.IsNullOrEmpty(col.Format) && val is IFormattable formattable)
            {
                return formattable.ToString(col.Format, null);
            }

            return val.ToString() ?? string.Empty;
        }

        public string GetRowKey(TItem item)
        {
            if (item == null) return Guid.NewGuid().ToString("N");
            if (KeySelector != null) return KeySelector(item).ToString() ?? Guid.NewGuid().ToString("N");
            return item.GetHashCode().ToString();
        }

        private int GetTotalColumnCount()
        {
            int count = VisibleColumns.Count();
            if (SelectionMode != CTableSelectionMode.None) count++;
            if (RowActionsTemplate != null) count++;
            return count;
        }

        #endregion

        #region Pagination

        private async Task ChangePage(int newPage)
        {
            if (newPage < 1 || newPage > TotalPages) return;
            _state.PageIndex = newPage;

            if (OnPageChanged.HasDelegate)
                await OnPageChanged.InvokeAsync(newPage);

            await RefreshAsync();
        }

        private async Task OnPageSizeChange(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out int newSize))
            {
                await OnPageSizeChangeInternal(newSize);
            }
        }

        private async Task OnPageSizeChangeInternal(int newSize)
        {
            _state.PageSize = newSize;
            _state.PageIndex = 1;
            await RefreshAsync();
        }

        private IEnumerable<int> GetPaginationNumbers()
        {
            var pages = new List<int>();
            int current = PageIndex;
            int total = TotalPages;

            if (total <= 7)
            {
                for (int i = 1; i <= total; i++) pages.Add(i);
            }
            else
            {
                pages.Add(1);
                if (current > 3) pages.Add(-1); // ...

                int start = Math.Max(2, current - 1);
                int end = Math.Min(total - 1, current + 1);

                for (int i = start; i <= end; i++) pages.Add(i);

                if (current < total - 2) pages.Add(-1); // ...
                pages.Add(total);
            }

            return pages;
        }

        #endregion

        #region Selection

        private bool IsRowSelected(TItem item) => _state.SelectedItems.Contains(item);

        private async Task ToggleRowSelection(TItem item, object? isCheckedObj)
        {
            bool isChecked = false;
            if (isCheckedObj is bool b) isChecked = b;
            else if (isCheckedObj?.ToString()?.ToLower() == "true") isChecked = true;

            if (SelectionMode == CTableSelectionMode.Single)
            {
                _state.SelectedItems.Clear();
                if (isChecked) _state.SelectedItems.Add(item);
            }
            else if (SelectionMode == CTableSelectionMode.Multiple)
            {
                if (isChecked) _state.SelectedItems.Add(item);
                else _state.SelectedItems.Remove(item);
            }

            if (OnSelectionChanged.HasDelegate)
                await OnSelectionChanged.InvokeAsync(_state.SelectedItems);
        }

        private async Task ToggleSelectAll(ChangeEventArgs e)
        {
            if (SelectionMode != CTableSelectionMode.Multiple) return;

            bool isChecked = e.Value is bool b && b;
            if (isChecked)
            {
                foreach (var item in _currentItems)
                    _state.SelectedItems.Add(item);
            }
            else
            {
                foreach (var item in _currentItems)
                    _state.SelectedItems.Remove(item);
            }

            if (OnSelectionChanged.HasDelegate)
                await OnSelectionChanged.InvokeAsync(_state.SelectedItems);
        }

        #endregion

        #region Events & Filters

        private async Task HandleRowClick(TItem item)
        {
            if (OnRowClick.HasDelegate)
                await OnRowClick.InvokeAsync(item);
        }

        private async Task HandleRowDoubleClick(TItem item)
        {
            if (OnRowDoubleClick.HasDelegate)
                await OnRowDoubleClick.InvokeAsync(item);
        }

        private async Task ApplyFilters()
        {
            _showSidebar = false;
            _state.PageIndex = 1;

            if (OnFilterChanged.HasDelegate)
                await OnFilterChanged.InvokeAsync(_state.Filters);

            await RefreshAsync();
        }

        private async Task ResetFilters()
        {
            _state.Filters.Clear();
            await ApplyFilters();
        }

        private async Task OnSearchChanged(string value)
        {
            SearchText = value;
            _state.PageIndex = 1;

            if (SearchTextChanged.HasDelegate)
                await SearchTextChanged.InvokeAsync(value);

            // Map built-in search to filters if needed
            _state.Filters["SearchText"] = value;

            await RefreshAsync();
        }

        #endregion
    }
}
