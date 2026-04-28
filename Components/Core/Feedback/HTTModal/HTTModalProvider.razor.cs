namespace HTT.BlazorWasm.App.Components
{
    public partial class HTTModalProvider : IDisposable
    {
        protected override void OnInitialized()
        {
            ModalService.OnChanged += HandleChanged;
        }

        private void HandleChanged()
        {
            InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            ModalService.OnChanged -= HandleChanged;
        }
    }
}
