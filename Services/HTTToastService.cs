namespace HTT.BlazorWasm.App.Services
{
    /// <summary>
    /// Dịch vụ hiển thị thông báo
    /// </summary>
    internal sealed class HTTToastService : IHTTToastService, IDisposable
    {
        /// <summary>
        /// Danh sách các thông báo đang hiển thị
        /// </summary>
        private readonly List<ToastModel> _activeToasts = new();

        /// <summary>
        /// Hàng đợi các thông báo chờ hiển thị
        /// </summary>
        private readonly Queue<ToastModel> _toastQueue = new();

        /// <summary>
        /// Sự kiện thay đổi trạng thái của thông báo
        /// </summary>
        public event Action? OnChange;

        /// <summary>
        /// Cấu hình toàn cục cho thông báo
        /// </summary>
        public ToastOptions GlobalOptions { get; } = new();

        /// <summary>
        /// Danh sách các thông báo đang hiển thị
        /// </summary>
        public IReadOnlyList<ToastModel> Toasts => _activeToasts.AsReadOnly();

        /// <summary>
        /// Hiện thông báo thành công
        /// </summary>
        public void ShowSuccess(string message, string? title = null, Action<ToastModel>? configure = null)
            => Show(CToastType.Success, message, title, configure);

        /// <summary>
        /// Hiện thông báo lỗi
        /// </summary>
        public void ShowError(string message, string? title = null, Action<ToastModel>? configure = null)
            => Show(CToastType.Error, message, title, configure);

        /// <summary>
        /// Hiện thông báo cảnh báo
        /// </summary>
        public void ShowWarning(string message, string? title = null, Action<ToastModel>? configure = null)
            => Show(CToastType.Warning, message, title, configure);

        /// <summary>
        /// Hiện thông báo thông tin
        /// </summary>
        public void ShowInfo(string message, string? title = null, Action<ToastModel>? configure = null)
            => Show(CToastType.Info, message, title, configure);

        /// <summary>
        /// Hiện thông báo tùy chỉnh
        /// </summary>
        public void ShowCustom(ToastModel model)
        {
            AddToast(model);
        }

        /// <summary>
        /// Hiện thông báo
        /// </summary>
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
                CreatedAt = DateTimeOffset.UtcNow
            };

            configure?.Invoke(model);
            AddToast(model);
        }

        /// <summary>
        /// Thêm thông báo vào danh sách
        /// </summary>
        /// <param name="model">Thông báo cần thêm</param>
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

        /// <summary>
        /// Xóa thông báo
        /// </summary>
        /// <param name="id">ID của thông báo cần xóa</param>
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

        /// <summary>
        /// Xóa tất cả thông báo
        /// </summary>
        public void Clear()
        {
            _activeToasts.Clear();
            _toastQueue.Clear();
            NotifyStateChanged();
        }

        /// <summary>
        /// Thông báo thay đổi trạng thái của thông báo
        /// </summary>
        private void NotifyStateChanged() => OnChange?.Invoke();
        /// <summary>
        /// Giải phóng tài nguyên
        /// </summary>
        public void Dispose()
        {
            _activeToasts.Clear();
            _toastQueue.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
