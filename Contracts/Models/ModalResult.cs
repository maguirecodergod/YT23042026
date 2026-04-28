namespace HTT.BlazorWasm.App.Contracts
{
    public class ModalResult
    {
        public bool Confirmed { get; set; }
        public object? Data { get; set; }

        public static ModalResult Ok(object? data = null) => new() { Confirmed = true, Data = data };
        public static ModalResult Cancel() => new() { Confirmed = false };
    }
}
