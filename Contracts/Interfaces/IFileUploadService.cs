namespace HTT.BlazorWasm.App.Contracts
{
    /// <summary>
    /// Core upload engine interface for HTTFileUploader.
    /// Responsible for chunk-based streaming uploads with real-time progress reporting.
    /// 
    /// Design principles:
    ///   – Progress is reported via IProgress[UploadFileModel] to decouple engine from UI.
    ///   – Chunk size is configurable per file for adaptive performance.
    ///   – Hash is computed prior to upload to support duplicate detection and integrity.
    ///   – Cancellation token is per-file so individual items can be cancelled independently.
    /// </summary>
    public interface IFileUploadService
    {
        /// <summary>
        /// Validate a list of staged files against the configured rules.
        /// Returns a dictionary mapping FileId → localization error key for each invalid file.
        /// Valid files are not included in the result.
        /// </summary>
        /// <param name="files">Staged file models to validate.</param>
        /// <param name="options">Validation constraints from the component parameters.</param>
        Dictionary<string, string> Validate(
            IReadOnlyList<UploadFileModel> files,
            FileUploadOptions options);

        /// <summary>
        /// Compute MD5 hash of the given browser file stream.
        /// Used before upload for duplicate detection. Cheap on small files, async on large.
        /// </summary>
        /// <param name="stream">File stream to hash.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<string> ComputeMd5Async(Stream stream, CancellationToken ct = default);

        /// <summary>
        /// Upload a single file with streaming chunk support.
        /// Mutates the <paramref name="model"/> in-place for real-time progress.
        /// Reports intermediate state via <paramref name="progress"/> to trigger UI re-renders.
        /// </summary>
        /// <param name="model">Mutable file model tracking upload state.</param>
        /// <param name="stream">Browser file stream to read from.</param>
        /// <param name="uploadUrl">Target API endpoint.</param>
        /// <param name="chunkSizeBytes">Chunk size in bytes. Defaults to 512 KB.</param>
        /// <param name="progress">Progress reporter for intermediate state updates.</param>
        /// <param name="ct">Per-file cancellation token.</param>
        Task UploadAsync(
            UploadFileModel model,
            Stream stream,
            string uploadUrl,
            int chunkSizeBytes = 524_288,
            IProgress<UploadFileModel>? progress = null,
            CancellationToken ct = default);
    }

    /// <summary>
    /// Strongly-typed validation constraints passed from HTTFileUploader parameters.
    /// Avoids loosely typed param bags.
    /// </summary>
    public sealed record FileUploadOptions(
        IReadOnlyList<string> AllowedExtensions,
        int MaxFileSizeMB,
        int MaxFileCount
    );
}
