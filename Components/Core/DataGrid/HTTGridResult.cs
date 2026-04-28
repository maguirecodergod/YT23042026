namespace HTT.BlazorWasm.App.Components.Core.DataGrid
{
    public class HTTGridResult<TItem>
    {
        public IEnumerable<TItem>? Items { get; set; }
        public int TotalCount { get; set; }
    }
}
