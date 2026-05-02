namespace HTT.BlazorWasm.App.Contracts
{
    public interface IHTTToastService
    {
        /// <summary>
        /// Danh sách các thông báo đang hiển thị
        /// </summary>
        IReadOnlyList<ToastModel> Toasts { get; }

        /// <summary>
        /// Sự kiện thay đổi trạng thái của thông báo
        /// </summary>
        event Action? OnChange;

        /// <summary>
        /// Cấu hình toàn cục cho thông báo
        /// </summary>
        ToastOptions GlobalOptions { get; }
        /// <summary>
        /// Hiện thông báo thành công
        /// </summary>
        /// <param name="message">Nội dung thông báo</param>
        /// <param name="title">Tiêu đề thông báo</param>
        /// <param name="configure">Cấu hình thông báo</param>
        void ShowSuccess(string message, string? title = null, Action<ToastModel>? configure = null);

        /// <summary>
        /// Hiện thông báo lỗi
        /// </summary>
        /// <param name="message">Nội dung thông báo</param>
        /// <param name="title">Tiêu đề thông báo</param>
        /// <param name="configure">Cấu hình thông báo</param>
        void ShowError(string message, string? title = null, Action<ToastModel>? configure = null);

        /// <summary>
        /// Hiện thông báo cảnh báo
        /// </summary>
        /// <param name="message">Nội dung thông báo</param>
        /// <param name="title">Tiêu đề thông báo</param>
        /// <param name="configure">Cấu hình thông báo</param>
        void ShowWarning(string message, string? title = null, Action<ToastModel>? configure = null);

        /// <summary>
        /// Hiện thông báo thông tin
        /// </summary>
        /// <param name="message">Nội dung thông báo</param>
        /// <param name="title">Tiêu đề thông báo</param>
        /// <param name="configure">Cấu hình thông báo</param>
        void ShowInfo(string message, string? title = null, Action<ToastModel>? configure = null);

        /// <summary>
        /// Hiện thông báo tùy chỉnh
        /// </summary>
        /// <param name="model">Thông báo tùy chỉnh</param>
        void ShowCustom(ToastModel model);

        /// <summary>
        /// Xóa thông báo
        /// </summary>
        /// <param name="id">ID của thông báo cần xóa</param>
        void Remove(Guid id);
        /// <summary>
        /// Xóa tất cả thông báo
        /// </summary>
        void Clear();
    }
}