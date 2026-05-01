namespace HTT.BlazorWasm.App.Services
{
    public class HTTToastService
    {
        private readonly List<ToastModel> _activeToasts = new();
        private readonly Queue<ToastModel> _toastQueue = new();
        
        public event Action? OnChange;
        public ToastOptions GlobalOptions { get; } = new();

        public IReadOnlyList<ToastModel> Toasts => _activeToasts.AsReadOnly();

        public void ShowSuccess(string message, string? title = null, Action<ToastModel>? configure = null)
            => Show(CToastType.Success, message, title, configure);

        public void ShowError(string message, string? title = null, Action<ToastModel>? configure = null)
            => Show(CToastType.Error, message, title, configure);

        public void ShowWarning(string message, string? title = null, Action<ToastModel>? configure = null)
            => Show(CToastType.Warning, message, title, configure);

        public void ShowInfo(string message, string? title = null, Action<ToastModel>? configure = null)
            => Show(CToastType.Info, message, title, configure);

        public void ShowCustom(ToastModel model)
        {
            AddToast(model);
        }

        private void Show(CToastType type, string message, string? title, Action<ToastModel>? configure)
        {
            var model = new ToastModel
            {
                Id = Guid.NewGuid(),
                Type = type,
                Message = message,
                Title = title ?? type.ToString(),
                Duration = GlobalOptions.DefaultDuration,
                Position = GlobalOptions.Position,
                CreatedAt = DateTime.Now
            };

            configure?.Invoke(model);
            AddToast(model);
        }

        private void AddToast(ToastModel model)
        {
            if (_activeToasts.Count < GlobalOptions.MaxToasts)
            {
                if (GlobalOptions.NewestOnTop)
                    _activeToasts.Insert(0, model);
                else
                    _activeToasts.Add(model);
            }
            else
            {
                _toastQueue.Enqueue(model);
            }

            NotifyStateChanged();
        }

        public void Remove(Guid id)
        {
            var toast = _activeToasts.FirstOrDefault(t => t.Id == id);
            if (toast != null)
            {
                _activeToasts.Remove(toast);
                
                // Process queue
                if (_toastQueue.Any())
                {
                    var next = _toastQueue.Dequeue();
                    if (GlobalOptions.NewestOnTop)
                        _activeToasts.Insert(0, next);
                    else
                        _activeToasts.Add(next);
                }
                
                NotifyStateChanged();
            }
        }

        public void Clear()
        {
            _activeToasts.Clear();
            _toastQueue.Clear();
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
