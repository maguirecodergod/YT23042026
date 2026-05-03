namespace HTT.BlazorWasm.App.Services.FileSourceProviders
{
    /// <summary>
    /// File source provider for URL-based file import.
    /// The user types a remote URL; this provider fetches metadata (HEAD request)
    /// and creates an UploadFileModel. Actual streaming is handled by IFileUploadService.
    /// </summary>
    public sealed class RemoteUrlProvider : FileSourceProviderBase
    {
        private readonly HttpClient _http;

        public RemoteUrlProvider(HttpClient http) => _http = http;

        public override CFileSourceType SourceType => CFileSourceType.RemoteUrl;
        public override string LabelKey  => "FileUploader.Source.Url";
        public override string IconClass => "bi bi-link-45deg";

        /// <summary>
        /// Resolves a remote URL into a file model via HTTP HEAD request.
        /// Falls back to path-based name resolution if HEAD is not supported.
        /// </summary>
        /// <param name="url">Fully-qualified URL to the remote file.</param>
        /// <param name="cancellationToken">Cancellation support.</param>
        public async Task<UploadFileModel?> GetFileFromUrlAsync(
            string url,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var uri = new Uri(url);
                var fileName = Path.GetFileName(uri.LocalPath);
                if (string.IsNullOrWhiteSpace(fileName))
                    fileName = "remote-file";

                // Try HEAD to get content-length and content-type
                var request = new HttpRequestMessage(HttpMethod.Head, uri);
                var response = await _http.SendAsync(request, cancellationToken);

                var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
                var size = response.Content.Headers.ContentLength ?? 0L;

                return CreateModel(fileName, size, contentType, CFileSourceType.RemoteUrl);
            }
            catch
            {
                return null;
            }
        }

        public override Task<IReadOnlyList<UploadFileModel>> GetFilesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<UploadFileModel>>([]);
    }

    /// <summary>
    /// Mock Google Drive provider — interface placeholder for OAuth integration.
    /// Replace GetFilesAsync with real Google Picker API when available.
    /// </summary>
    public sealed class GoogleDriveProvider : FileSourceProviderBase
    {
        public override CFileSourceType SourceType => CFileSourceType.GoogleDrive;
        public override string LabelKey  => "FileUploader.Source.GoogleDrive";
        public override string IconClass => "bi bi-google";
        public override bool IsAvailable => false; // Enable when OAuth is wired

        public override Task<IReadOnlyList<UploadFileModel>> GetFilesAsync(CancellationToken cancellationToken = default)
        {
            // TODO: Integrate Google Picker API via JSInterop
            return Task.FromResult<IReadOnlyList<UploadFileModel>>([]);
        }
    }

    /// <summary>
    /// Mock OneDrive provider — interface placeholder for MSAL integration.
    /// Replace GetFilesAsync with real Microsoft Graph file picker when available.
    /// </summary>
    public sealed class OneDriveProvider : FileSourceProviderBase
    {
        public override CFileSourceType SourceType => CFileSourceType.OneDrive;
        public override string LabelKey  => "FileUploader.Source.OneDrive";
        public override string IconClass => "bi bi-microsoft";
        public override bool IsAvailable => false; // Enable when MSAL is wired

        public override Task<IReadOnlyList<UploadFileModel>> GetFilesAsync(CancellationToken cancellationToken = default)
        {
            // TODO: Integrate Microsoft Graph file picker via JSInterop
            return Task.FromResult<IReadOnlyList<UploadFileModel>>([]);
        }
    }
}
