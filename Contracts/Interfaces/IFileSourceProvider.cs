namespace HTT.BlazorWasm.App.Contracts
{
    /// <summary>
    /// Abstraction for a file source provider.
    /// Each upload channel (file picker, drag-drop, clipboard, URL, cloud) implements this interface.
    /// Follows the Open/Closed Principle — new sources can be added without modifying HTTFileUploader.
    /// </summary>
    public interface IFileSourceProvider
    {
        /// <summary>
        /// Unique key identifying this provider.
        /// Maps to CFileSourceType and is used for routing in HTTFileUploader.
        /// </summary>
        CFileSourceType SourceType { get; }

        /// <summary>
        /// Localization key for the provider's display label shown in the UI.
        /// Example: "FileUploader.Source.LocalFile"
        /// </summary>
        string LabelKey { get; }

        /// <summary>
        /// Bootstrap icon class for the provider's tab/button.
        /// Example: "bi bi-folder2-open"
        /// </summary>
        string IconClass { get; }

        /// <summary>
        /// Whether this provider is currently available.
        /// Allows runtime feature-flagging (e.g., disable cloud sources in offline mode).
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Asynchronously obtains files from this source.
        /// The implementation controls how files are selected (dialog, paste, fetch, OAuth, etc.).
        /// Returns an empty collection if the user cancelled or no files were found.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for async cleanup.</param>
        Task<IReadOnlyList<UploadFileModel>> GetFilesAsync(CancellationToken cancellationToken = default);
    }
}
