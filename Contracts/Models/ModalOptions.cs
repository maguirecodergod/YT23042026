namespace HTT.BlazorWasm.App.Contracts
{
    public class ModalOptions
    {
        public string? Title { get; set; }
        public Dictionary<string, object>? Parameters { get; set; }
        public bool Closable { get; set; } = true;
        public bool MaskClosable { get; set; } = true;
        public bool Centered { get; set; } = true;
        public bool Fullscreen { get; set; }
        public string? Width { get; set; } = "520px";
        public bool DestroyOnClose { get; set; } = true;
        public string? Class { get; set; }
        public string? Style { get; set; }
        public bool Loading { get; set; }
        public RenderFragment? Footer { get; set; }
    }
}
