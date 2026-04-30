namespace HTT.BlazorWasm.App.Components
{
    public class HTTBreadcrumbItem
    {
        public string Text { get; set; } = string.Empty;
        public string? Href { get; set; }
        public string? Icon { get; set; }
        public bool IsActive { get; set; }
    }
}
