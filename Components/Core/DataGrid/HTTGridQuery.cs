namespace HTT.BlazorWasm.App.Components
{
    public class HTTGridQuery
    {
        public int PageIndex { get; set; } = 0;
        public int PageSize { get; set; } = 10;
        public List<SortDescriptor> Sorts { get; set; } = new();
        public List<FilterDescriptor> Filters { get; set; } = new();
        public string? SearchTerm { get; set; }
    }

    public class SortDescriptor
    {
        public string? Field { get; set; }
        public SortDirection Direction { get; set; }
    }

    public enum SortDirection
    {
        None,
        Ascending,
        Descending
    }

    public class FilterDescriptor
    {
        public string? Field { get; set; }
        public string? Operator { get; set; }
        public object? Value { get; set; }
    }
}
