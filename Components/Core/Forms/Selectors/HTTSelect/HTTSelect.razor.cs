using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Linq.Expressions;
using System.Text;

namespace HTT.BlazorWasm.App.Components
{
    /// <summary>
    /// A production-grade, enterprise-level reusable select component.
    /// Supports generic types, async loading, remote search, multi-select, and validation.
    /// </summary>
    /// <typeparam name="TItem">The type of the item in the data source.</typeparam>
    /// <typeparam name="TValue">The type of the bound value.</typeparam>
    public partial class HTTSelect<TItem, TValue> : HTTComponentBase
    {
        #region Parameters

        /// <summary>
        /// The bound value.
        /// </summary>
        [Parameter] public TValue? Value { get; set; }

        /// <summary>
        /// Callback when the value changes.
        /// </summary>
        [Parameter] public EventCallback<TValue?> ValueChanged { get; set; }

        /// <summary>
        /// Expression to identify the bound property for validation.
        /// </summary>
        [Parameter] public Expression<Func<TValue?>>? ValueExpression { get; set; }

        /// <summary>
        /// Static list of items to display.
        /// </summary>
        [Parameter] public IEnumerable<TItem>? Items { get; set; }

        /// <summary>
        /// Function to load data asynchronously.
        /// </summary>
        [Parameter] public Func<Task<IEnumerable<TItem>>>? LoadData { get; set; }

        /// <summary>
        /// Function for remote search.
        /// </summary>
        [Parameter] public Func<string, Task<IEnumerable<TItem>>>? SearchFunc { get; set; }

        /// <summary>
        /// Mapping function for item text.
        /// </summary>
        [Parameter] public Func<TItem, string> TextSelector { get; set; } = item => item?.ToString() ?? string.Empty;

        /// <summary>
        /// Mapping function for item value.
        /// </summary>
        [Parameter] public Func<TItem, object> ValueSelector { get; set; } = item => item!;

        /// <summary>
        /// Function to determine if an item should be disabled.
        /// </summary>
        [Parameter] public Func<TItem, bool>? DisabledSelector { get; set; }

        /// <summary>
        /// Placeholder text when no item is selected.
        /// </summary>
        [Parameter] public string Placeholder { get; set; } = "Select option...";

        /// <summary>
        /// If true, enables search filtering.
        /// </summary>
        [Parameter] public bool Searchable { get; set; }

        /// <summary>
        /// If true, allows multiple selections.
        /// </summary>
        [Parameter] public bool Multiple { get; set; }

        /// <summary>
        /// If true, the component is disabled.
        /// </summary>
        [Parameter] public bool Disabled { get; set; }

        /// <summary>
        /// If true, shows a clear button.
        /// </summary>
        [Parameter] public bool AllowClear { get; set; } = true;

        /// <summary>
        /// Overall size of the component.
        /// </summary>
        [Parameter] public CSpacingType Size { get; set; } = CSpacingType.MD;

        /// <summary>
        /// Custom CSS class.
        /// </summary>
        [Parameter] public bool ShowArrow { get; set; } = true;
        [Parameter] public string? Class { get; set; }

        /// <summary>
        /// Custom inline style.
        /// </summary>
        [Parameter] public string? Style { get; set; }

        #region Composition Slots

        [Parameter] public RenderFragment<TItem>? ItemTemplate { get; set; }
        [Parameter] public RenderFragment<TItem>? SelectedTemplate { get; set; }
        [Parameter] public RenderFragment? EmptyTemplate { get; set; }
        [Parameter] public RenderFragment? LoadingTemplate { get; set; }

        #endregion

        #endregion

        #region Cascading Parameters

        [CascadingParameter] private EditContext? EditContext { get; set; }

        #endregion

        #region State

        private List<TItem> _filteredItems = new();
        private bool _isOpen;
        private bool _isLoading;
        private string _searchText = string.Empty;
        private int _activeIndex = -1;
        private List<TItem> _selectedItems = new();
        private FieldIdentifier _fieldIdentifier;
        private ElementReference _selectContainer;
        private ElementReference _searchInput;
        private ElementReference _dropdownElement;
        private CancellationTokenSource? _searchCts;
        private IJSObjectReference? _jsModule;
        private bool _shouldFocusSearch;
        private bool _shouldCheckPlacement;
        private bool _shouldScrollToSelected;

        #endregion

        #region Lifecycle

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            if (EditContext != null && ValueExpression != null)
            {
                _fieldIdentifier = FieldIdentifier.Create(ValueExpression);
                EditContext.OnValidationStateChanged += HandleValidationStateChanged;
            }

            if (Items != null)
            {
                _filteredItems = Items.ToList();
                SyncSelectedItems();
            }
            else if (LoadData != null)
            {
                await RefreshDataAsync();
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            
            if (Items != null && !_isLoading && string.IsNullOrEmpty(_searchText))
            {
                _filteredItems = Items.ToList();
            }
            
            SyncSelectedItems();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _jsModule = await JS.InvokeAsync<IJSObjectReference>("import", "./Components/Core/Forms/Selectors/HTTSelect/HTTSelect.razor.js");
                await _jsModule.InvokeVoidAsync("initializeClickOutside", _selectContainer, DotNetObjectReference.Create(this));
            }

            if (_shouldFocusSearch && _isOpen && Searchable)
            {
                _shouldFocusSearch = false;
                try
                {
                    await _searchInput.FocusAsync();
                }
                catch { /* Ignore if focus fails */ }
            }

            if (_shouldCheckPlacement && _isOpen)
            {
                _shouldCheckPlacement = false;
                try
                {
                    await JS.InvokeVoidAsync("window.httSelect.checkPlacement", _selectContainer, _dropdownElement);
                }
                catch { /* Fail safe */ }
            }

            if (_shouldScrollToSelected && _isOpen)
            {
                _shouldScrollToSelected = false;
                try
                {
                    await JS.InvokeVoidAsync("window.httSelect.scrollToSelected", _dropdownElement);
                }
                catch { /* Fail safe */ }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            if (EditContext != null)
            {
                EditContext.OnValidationStateChanged -= HandleValidationStateChanged;
            }
            _searchCts?.Cancel();
            _searchCts?.Dispose();
            _jsModule?.DisposeAsync();
        }

        #endregion

        #region Logic

        private void HandleValidationStateChanged(object? sender, ValidationStateChangedEventArgs e) => StateHasChanged();

        private bool IsInvalid => EditContext?.GetValidationMessages(_fieldIdentifier).Any() ?? false;

        private void SyncSelectedItems()
        {
            if (Multiple)
            {
                if (Value is System.Collections.IEnumerable values)
                {
                    var valueList = new List<object>();
                    foreach (var v in values) valueList.Add(v);

                    if (Items != null)
                    {
                        _selectedItems = Items.Where(i => valueList.Any(v => EqualityComparer<object>.Default.Equals(ValueSelector(i), v))).ToList();
                    }
                }
            }
            else
            {
                if (Value != null && Items != null)
                {
                    var selected = Items.FirstOrDefault(i => EqualityComparer<object>.Default.Equals(ValueSelector(i), Value));
                    if (selected != null)
                    {
                        _selectedItems = new List<TItem> { selected };
                    }
                }
                else if (Value == null)
                {
                    _selectedItems.Clear();
                }
            }
        }

        private async Task RefreshDataAsync()
        {
            if (LoadData == null) return;

            _isLoading = true;
            StateHasChanged();

            try
            {
                var data = await LoadData();
                _filteredItems = data.ToList();
                SyncSelectedItems();
            }
            finally
            {
                _isLoading = false;
                StateHasChanged();
            }
        }

        private async Task ToggleDropdown()
        {
            if (Disabled) return;

            _isOpen = !_isOpen;
            if (_isOpen)
            {
                _activeIndex = -1;
                _shouldCheckPlacement = true;
                _shouldScrollToSelected = true;

                if (Searchable)
                {
                    _shouldFocusSearch = true;
                }
            }
            else
            {
                _searchText = string.Empty;
                if (Items != null) _filteredItems = Items.ToList();
            }
        }

        [JSInvokable]
        public void CloseDropdown()
        {
            if (_isOpen)
            {
                _isOpen = false;
                _searchText = string.Empty;
                if (Items != null) _filteredItems = Items.ToList();
                StateHasChanged();
            }
        }

        private async Task HandleSearchInput(ChangeEventArgs e)
        {
            _searchText = e.Value?.ToString() ?? string.Empty;
            _activeIndex = -1;

            if (SearchFunc != null)
            {
                _searchCts?.Cancel();
                _searchCts = new CancellationTokenSource();
                var token = _searchCts.Token;

                try
                {
                    await Task.Delay(300, token); // Debounce
                    _isLoading = true;
                    StateHasChanged();

                    var results = await SearchFunc(_searchText);
                    if (!token.IsCancellationRequested)
                    {
                        _filteredItems = results.ToList();
                    }
                }
                catch (TaskCanceledException) { }
                finally
                {
                    if (!token.IsCancellationRequested)
                    {
                        _isLoading = false;
                        StateHasChanged();
                    }
                }
            }
            else if (Items != null)
            {
                if (string.IsNullOrWhiteSpace(_searchText))
                {
                    _filteredItems = Items.ToList();
                }
                else
                {
                    _filteredItems = Items.Where(i => TextSelector(i).Contains(_searchText, StringComparison.OrdinalIgnoreCase)).ToList();
                }
            }
        }

        private async Task SelectItem(TItem item)
        {
            if (DisabledSelector?.Invoke(item) == true) return;

            if (Multiple)
            {
                var itemValue = ValueSelector(item);
                var existing = _selectedItems.FirstOrDefault(i => EqualityComparer<object>.Default.Equals(ValueSelector(i), itemValue));

                if (existing != null)
                {
                    _selectedItems.Remove(existing);
                }
                else
                {
                    _selectedItems.Add(item);
                }

                Value = MapToTargetCollection(_selectedItems.Select(ValueSelector));
            }
            else
            {
                _selectedItems = new List<TItem> { item };
                var val = ValueSelector(item);
                Value = (TValue)Convert.ChangeType(val, typeof(TValue));
                _isOpen = false;
            }

            await ValueChanged.InvokeAsync(Value);
            EditContext?.NotifyFieldChanged(_fieldIdentifier);
            StateHasChanged();
        }

        private TValue? MapToTargetCollection(IEnumerable<object> values)
        {
            if (values == null) return default;

            var targetType = typeof(TValue);

            // Handle List<string> specifically for common use case
            if (targetType == typeof(List<string>))
            {
                return (TValue)(object)values.Select(v => v?.ToString() ?? string.Empty).ToList();
            }

            // Generic List support
            if (targetType.IsGenericType && (targetType.GetGenericTypeDefinition() == typeof(List<>) || targetType.GetGenericTypeDefinition() == typeof(IEnumerable<>) || targetType.GetGenericTypeDefinition() == typeof(ICollection<>)))
            {
                var elementType = targetType.GetGenericArguments()[0];
                var listType = typeof(List<>).MakeGenericType(elementType);
                var list = (System.Collections.IList)Activator.CreateInstance(listType)!;
                foreach (var v in values)
                {
                    list.Add(Convert.ChangeType(v, elementType));
                }
                return (TValue)list;
            }

            // Array support
            if (targetType.IsArray)
            {
                var elementType = targetType.GetElementType()!;
                var array = Array.CreateInstance(elementType, values.Count());
                int i = 0;
                foreach (var v in values)
                {
                    array.SetValue(Convert.ChangeType(v, elementType), i++);
                }
                return (TValue)(object)array;
            }

            return (TValue)(object)values.ToList();
        }

        private async Task RemoveItem(TItem item)
        {
            if (Disabled) return;

            var itemValue = ValueSelector(item);
            var existing = _selectedItems.FirstOrDefault(i => EqualityComparer<object>.Default.Equals(ValueSelector(i), itemValue));
            
            if (existing != null)
            {
                _selectedItems.Remove(existing);
            }

            if (Multiple)
            {
                Value = MapToTargetCollection(_selectedItems.Select(ValueSelector));
            }
            else
            {
                Value = default;
            }

            await ValueChanged.InvokeAsync(Value);
            EditContext?.NotifyFieldChanged(_fieldIdentifier);
        }

        private async Task ClearSelection()
        {
            if (Disabled) return;

            _selectedItems.Clear();
            Value = default;
            await ValueChanged.InvokeAsync(Value);
            EditContext?.NotifyFieldChanged(_fieldIdentifier);
        }

        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (Disabled) return;

            if (!_isOpen)
            {
                if (e.Key == "Enter" || e.Key == "ArrowDown" || e.Key == "ArrowUp")
                {
                    await ToggleDropdown();
                }
                return;
            }

            switch (e.Key)
            {
                case "Escape":
                    CloseDropdown();
                    break;
                case "ArrowDown":
                    _activeIndex = Math.Min(_activeIndex + 1, _filteredItems.Count - 1);
                    break;
                case "ArrowUp":
                    _activeIndex = Math.Max(_activeIndex - 1, 0);
                    break;
                case "Enter":
                    if (_activeIndex >= 0 && _activeIndex < _filteredItems.Count)
                    {
                        await SelectItem(_filteredItems[_activeIndex]);
                    }
                    break;
            }
        }

        private string BuildCssClass()
        {
            var sb = new StringBuilder();
            sb.Append("htt-select");
            sb.Append($" htt-select--{Size.ToString().ToLower()}");
            if (Disabled) sb.Append(" is-disabled");
            if (IsInvalid) sb.Append(" is-invalid");
            if (_isOpen) sb.Append(" is-open");
            if (Multiple) sb.Append(" is-multiple");
            if (!string.IsNullOrWhiteSpace(Class)) sb.Append($" {Class}");
            return sb.ToString().Trim();
        }

        #endregion
    }
}
