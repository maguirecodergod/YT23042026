using System.Collections.Generic;

namespace HTT.BlazorWasm.App.Components
{
    public enum CTableSelectionMode
    {
        None,
        Single,
        Multiple
    }

    public class TableSortState
    {
        public string ColumnId { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public SortDirection Direction { get; set; }
    }

    public class TableState<TItem>
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        
        public List<TableSortState> Sorts { get; set; } = new();
        public Dictionary<string, object> Filters { get; set; } = new();
        
        public Dictionary<string, bool> ColumnVisibility { get; set; } = new();
        public Dictionary<string, string> ColumnWidths { get; set; } = new();
        
        public HashSet<TItem> SelectedItems { get; set; } = new();
    }
}
