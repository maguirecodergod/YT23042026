using System.Collections.Generic;

namespace HTT.BlazorWasm.App.Components.Core.DataGrid
{
    public class HTTGridState<TItem>
    {
        public int PageIndex { get; set; } = 0;
        public int PageSize { get; set; } = 10;
        public List<SortDescriptor> Sorts { get; set; } = new();
        public List<FilterDescriptor> Filters { get; set; } = new();
        public List<TItem> SelectedItems { get; set; } = new();
        public HashSet<TItem> ExpandedRows { get; set; } = new();
    }
}
