namespace HTT.BlazorWasm.App.Components
{
    public partial class HTTToastContainer : ComponentBase, IDisposable
    {
        protected override void OnInitialized()
        {
            ToastService.OnChange += HandleChange;
        }

        private async void HandleChange()
        {
            await InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            ToastService.OnChange -= HandleChange;
        }
    }
}
