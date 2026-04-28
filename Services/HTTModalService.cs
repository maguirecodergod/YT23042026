namespace HTT.BlazorWasm.App.Services
{
    public class HTTModalService : IHTTModalService
    {
        private readonly List<ModalReference> _activeModals = new();

        public IEnumerable<ModalReference> ActiveModals => _activeModals;

        public event Action? OnChanged;

        public Task<ModalResult> ShowAsync<TComponent>(ModalOptions? options = null) where TComponent : IComponent
        {
            var reference = new ModalReference
            {
                ComponentType = typeof(TComponent),
                Options = options ?? new ModalOptions()
            };

            _activeModals.Add(reference);
            NotifyChanged();

            return reference.TaskCompletionSource.Task;
        }

        public Task<ModalResult> ShowAsync<TComponent>(string title, Dictionary<string, object>? parameters = null) where TComponent : IComponent
        {
            return ShowAsync<TComponent>(new ModalOptions { Title = title, Parameters = parameters });
        }

        public void Close(ModalResult result)
        {
            var lastModal = _activeModals.LastOrDefault();
            if (lastModal != null)
            {
                Close(lastModal.Id, result);
            }
        }

        public void Close(Guid id, ModalResult result)
        {
            var modal = _activeModals.FirstOrDefault(m => m.Id == id);
            if (modal != null)
            {
                modal.Dismiss(result);
                _activeModals.Remove(modal);
                NotifyChanged();
            }
        }

        private void NotifyChanged() => OnChanged?.Invoke();
    }
}
