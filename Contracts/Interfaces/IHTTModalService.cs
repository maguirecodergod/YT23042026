namespace HTT.BlazorWasm.App.Contracts
{
    public interface IHTTModalService
    {
        Task<ModalResult> ShowAsync<TComponent>(ModalOptions? options = null) where TComponent : IComponent;
        Task<ModalResult> ShowAsync<TComponent>(string title, Dictionary<string, object>? parameters = null) where TComponent : IComponent;
        
        void Close(ModalResult result);
        void Close(Guid id, ModalResult result);
        
        event Action? OnChanged;
        IEnumerable<ModalReference> ActiveModals { get; }
    }

    public class ModalReference
    {
        public Guid Id { get; } = Guid.NewGuid();
        public Type ComponentType { get; init; } = default!;
        public ModalOptions Options { get; init; } = default!;
        public TaskCompletionSource<ModalResult> TaskCompletionSource { get; } = new();

        public void Dismiss(ModalResult result)
        {
            TaskCompletionSource.TrySetResult(result);
        }
    }
}
