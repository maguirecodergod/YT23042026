using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace HTT.BlazorWasm.App.Components.Core.DataGrid
{
    public enum HTTGridDisplayMode
    {
        Table,
        Card
    }

    public partial class HTTGrid<TItem> : HTTComponentBase
    {
        [Parameter] public IEnumerable<TItem>? Items { get; set; }
        [Parameter] public Func<HTTGridQuery, Task<HTTGridResult<TItem>>>? LoadData { get; set; }
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public RenderFragment<TItem>? RowTemplate { get; set; }
        [Parameter] public RenderFragment<TItem>? CardTemplate { get; set; }
        [Parameter] public RenderFragment<TItem>? DetailTemplate { get; set; }
        [Parameter] public HTTGridDisplayMode DisplayMode { get; set; } = HTTGridDisplayMode.Table;
        [Parameter] public int CardMinWidth { get; set; } = 300;
        [Parameter] public bool MultiSelect { get; set; }
        [Parameter] public bool Selectable { get; set; } = true;
        [Parameter] public bool Virtualize { get; set; } = true;
        [Parameter] public float ItemSize { get; set; } = 48f;
        [Parameter] public EventCallback<TItem> OnRowClick { get; set; }
        [Parameter] public string Id { get; set; } = Guid.NewGuid().ToString("N");

        private List<int> _pageSizeOptions = new() { 10, 20, 50, 100 };

        private List<HTTColumn<TItem>> _columns = new();
        private HTTGridState<TItem> _state = new();
        private List<TItem> _viewItems = new();
        private int _totalCount;
        private bool _isLoading;
        private Virtualize<TItem>? _virtualizeComponent;

        public void AddColumn(HTTColumn<TItem> column)
        {
            if (!_columns.Contains(column))
            {
                _columns.Add(column);
                StateHasChanged();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await RefreshAsync();
        }

        public async Task RefreshAsync()
        {
            if (LoadData != null)
            {
                _isLoading = true;
                StateHasChanged();

                var query = new HTTGridQuery
                {
                    PageIndex = _state.PageIndex,
                    PageSize = _state.PageSize,
                    Sorts = _state.Sorts,
                    Filters = _state.Filters
                };

                var result = await LoadData(query);
                _viewItems = result.Items?.ToList() ?? new List<TItem>();
                _totalCount = result.TotalCount;

                _isLoading = false;
            }
            else if (Items != null)
            {
                ApplyLocalState();
            }

            StateHasChanged();
        }

        private void ApplyLocalState()
        {
            var queryable = Items?.AsQueryable();

            // Apply Filters (Placeholder for complex filtering)
            // Apply Sorting
            if (_state.Sorts.Any())
            {
                var firstSort = _state.Sorts.First();
                // This would need dynamic expression building for local sorting
                // Simplified for now
            }

            _totalCount = queryable?.Count() ?? 0;
            _viewItems = queryable?.Skip(_state.PageIndex * _state.PageSize).Take(_state.PageSize).ToList() ?? new List<TItem>();
        }

        private async Task OnPageSizeChanged(int size)
        {
            _state.PageSize = size;
            _state.PageIndex = 0; // Reset to first page
            await RefreshAsync();
        }

        private async Task OnPageChanged(int index)
        {
            var totalPages = (int)Math.Ceiling((double)_totalCount / _state.PageSize);
            if (index < 0 || index >= totalPages && totalPages > 0) return;

            _state.PageIndex = index;
            await RefreshAsync();
        }

        private IEnumerable<int> GetVisiblePages()
        {
            var totalPages = (int)Math.Ceiling((double)_totalCount / _state.PageSize);
            var current = _state.PageIndex;
            var pages = new List<int>();

            if (totalPages <= 7)
            {
                for (int i = 0; i < totalPages; i++) pages.Add(i);
            }
            else
            {
                pages.Add(0);
                if (current > 3) pages.Add(-1); // Ellipsis

                int start = Math.Max(1, current - 2);
                int end = Math.Min(totalPages - 2, current + 2);

                for (int i = start; i <= end; i++) pages.Add(i);

                if (current < totalPages - 4) pages.Add(-1); // Ellipsis
                pages.Add(totalPages - 1);
            }

            return pages;
        }

        private async Task SortBy(HTTColumn<TItem> column)
        {
            if (!column.Sortable) return;

            var field = column.GetFieldName();
            var existingSort = _state.Sorts.FirstOrDefault(s => s.Field == field);

            if (existingSort != null)
            {
                if (existingSort.Direction == SortDirection.Ascending)
                    existingSort.Direction = SortDirection.Descending;
                else
                    _state.Sorts.Remove(existingSort);
            }
            else
            {
                _state.Sorts.Clear(); // Single sort for now
                _state.Sorts.Add(new SortDescriptor { Field = field, Direction = SortDirection.Ascending });
            }

            await RefreshAsync();
        }

        private async Task ToggleRowSelection(TItem item)
        {
            if (!Selectable) return;

            if (MultiSelect)
            {
                if (_state.SelectedItems.Contains(item))
                    _state.SelectedItems.Remove(item);
                else
                    _state.SelectedItems.Add(item);
            }
            else
            {
                _state.SelectedItems.Clear();
                _state.SelectedItems.Add(item);
            }

            await OnRowClick.InvokeAsync(item);
        }

        private void ToggleRowExpansion(TItem item)
        {
            if (_state.ExpandedRows.Contains(item))
                _state.ExpandedRows.Remove(item);
            else
                _state.ExpandedRows.Add(item);
        }

        private string GetGridTemplateColumns()
        {
            var columns = _columns.Where(c => c.Visible).Select(c => string.IsNullOrEmpty(c.Width) ? "1fr" : c.Width).ToList();
            if (Selectable && MultiSelect)
            {
                columns.Insert(0, "48px");
            }
            return string.Join(" ", columns);
        }

        private string GetHeaderClass(HTTColumn<TItem> column)
        {
            var classes = new List<string> { "htt-grid__header-cell" };
            if (column.Sortable) classes.Add("htt-grid__header-cell--sortable");

            var sort = _state.Sorts.FirstOrDefault(s => s.Field == column.GetFieldName());
            if (sort != null)
            {
                classes.Add(sort.Direction == SortDirection.Ascending ? "htt-grid__header-cell--sorted-asc" : "htt-grid__header-cell--sorted-desc");
            }

            return string.Join(" ", classes);
        }
    }
}