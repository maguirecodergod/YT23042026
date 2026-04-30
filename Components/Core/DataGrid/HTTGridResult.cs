namespace HTT.BlazorWasm.App.Components
{
    public class HTTGridResult<TItem>
    {
        public IEnumerable<TItem>? Items { get; set; }
        public int TotalCount { get; set; }
    }
}
