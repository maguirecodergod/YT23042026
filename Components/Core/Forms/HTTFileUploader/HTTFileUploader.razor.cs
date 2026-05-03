using Microsoft.AspNetCore.Components.Forms;
using HTT.BlazorWasm.App.Services;
using HTT.BlazorWasm.App.Services.FileSourceProviders;
using System.Text;

namespace HTT.BlazorWasm.App.Components
{
    /// <summary>
    /// HTTFileUploader — Enterprise-grade, multi-source file upload component.
    ///
    /// Architecture:
    ///   HTTFileUploader (UI + orchestration)
    ///     └─ IFileUploadService       — upload engine (chunk, progress, hash)
    ///     └─ IFileSourceProvider[]    — pluggable source channels
    ///     └─ UploadFileModel[]        — per-file state machine
    ///     └─ CancellationTokenSource  — per-file cancel support
    ///
    /// Design decisions:
    ///   – Queue is a List[UploadFileModel]; rendering uses @key for stable DOM identity
    ///   – Progress reported via IProgress[UploadFileModel] → InvokeAsync → StateHasChanged
    ///   – Clipboard/Drag-Drop handled via JS interop on the drop zone div
    ///   – Image preview uses browser ObjectURL (createObjectURL / revokeObjectURL) to avoid
    ///     loading large files into WASM heap
    ///   – MD5 computed before upload only when EnableHashing = true (perf guard)
    /// </summary>
    public partial class HTTFileUploader : HTTComponentBase
    {
        // ═══════════════════════════════════════════════════════════
        //  Injected Services
        // ═══════════════════════════════════════════════════════════

        [Inject] private IFileUploadService UploadService { get; set; } = default!;

        // ═══════════════════════════════════════════════════════════
        //  Parameters — Configuration
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Whitelisted file extensions (lowercase, without dot).
        /// Empty list = all extensions accepted.
        /// Example: new List[string] { "pdf", "docx", "png" }
        /// </summary>
        [Parameter] public List<string> AllowedExtensions { get; set; } = [];

        /// <summary>Maximum file size per file in megabytes. Default: 50 MB.</summary>
        [Parameter] public int MaxFileSizeMB { get; set; } = 50;

        /// <summary>Maximum number of files in the queue simultaneously. Default: 10.</summary>
        [Parameter] public int MaxFileCount { get; set; } = 10;

        /// <summary>
        /// API endpoint that receives the file chunks.
        /// Required for real uploads. In demo mode (UploadUrl = ""), simulated upload runs.
        /// </summary>
        [Parameter] public string UploadUrl { get; set; } = string.Empty;

        /// <summary>
        /// Chunk size in bytes for streaming upload. Default: 512 KB.
        /// Adjust down for low-bandwidth environments.
        /// </summary>
        [Parameter] public int ChunkSizeBytes { get; set; } = 524_288;

        /// <summary>
        /// If true, computes MD5 hash of each file before upload.
        /// Adds CPU time for large files; recommended for document/archive types.
        /// </summary>
        [Parameter] public bool EnableHashing { get; set; } = true;

        /// <summary>
        /// If true, uploads begin automatically after files are added to the queue.
        /// If false, the user must click the upload button.
        /// </summary>
        [Parameter] public bool AutoUpload { get; set; }

        /// <summary>Custom CSS class applied to the root container.</summary>
        [Parameter] public string? Class { get; set; }

        // ═══════════════════════════════════════════════════════════
        //  Parameters — Events
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Fired when all queued files have reached a terminal state
        /// (Completed, Failed, or Cancelled). Passes the full model list.
        /// </summary>
        [Parameter] public EventCallback<List<UploadFileModel>> OnUploadCompleted { get; set; }

        /// <summary>Fired when a file is removed from the queue.</summary>
        [Parameter] public EventCallback<UploadFileModel> OnFileRemoved { get; set; }

        /// <summary>Fired when validation rejects a file.</summary>
        [Parameter] public EventCallback<(UploadFileModel File, string ErrorKey)> OnValidationError { get; set; }

        // ═══════════════════════════════════════════════════════════
        //  Internal State
        // ═══════════════════════════════════════════════════════════

        /// <summary>The active upload queue, ordered by insertion time.</summary>
        private readonly List<UploadFileModel> _queue = [];

        /// <summary>Per-file cancellation tokens keyed by FileId.</summary>
        private readonly Dictionary<string, CancellationTokenSource> _ctsSources = [];

        /// <summary>IBrowserFile references keyed by FileId — needed for actual stream reads.</summary>
        private readonly Dictionary<string, IBrowserFile> _browserFiles = [];

        /// <summary>Blob object URLs for image previews, keyed by FileId. Revoked on remove.</summary>
        private readonly Dictionary<string, string> _previewUrls = [];

        /// <summary>Drives the drag-over visual state.</summary>
        private bool _isDragOver;

        /// <summary>Whether the URL input panel is expanded.</summary>
        private bool _showUrlInput;

        /// <summary>URL input field binding.</summary>
        private string _urlInput = string.Empty;

        /// <summary>Whether URL is being fetched.</summary>
        private bool _isFetchingUrl;

        /// <summary>Unique ID for the hidden input element to support multiple instances on one page.</summary>
        private readonly string _inputId = $"htt-file-input-{Guid.NewGuid():N}";

        /// <summary>Reference to the hidden input wrapper span.</summary>
        private ElementReference _fileInputRef;

        /// <summary>ElementReference for the drop zone div (clipboard/drag event target).</summary>
        private ElementReference _dropZoneRef;

        private DotNetObjectReference<HTTFileUploader>? _dotNetRef;

        // ═══════════════════════════════════════════════════════════
        //  Computed Helpers
        // ═══════════════════════════════════════════════════════════

        private FileUploadOptions ValidationOptions => new(
            AllowedExtensions,
            MaxFileSizeMB,
            MaxFileCount
        );

        private bool HasFiles => _queue.Count > 0;
        private bool IsQueueFull => _queue.Count >= MaxFileCount;

        private bool AllTerminated => _queue.Count > 0 &&
            _queue.All(f => f.Status is CUploadStatus.Completed
                                     or CUploadStatus.Failed
                                     or CUploadStatus.Cancelled);

        private bool HasPendingFiles => _queue.Any(f =>
            f.Status is CUploadStatus.Pending or CUploadStatus.Failed);

        private int CompletedCount => _queue.Count(f => f.Status == CUploadStatus.Completed);
        private int FailedCount => _queue.Count(f => f.Status == CUploadStatus.Failed);

        protected string AcceptAttribute
        {
            get
            {
                if (AllowedExtensions.Count == 0) return string.Empty;

                var normalized = AllowedExtensions.Select(e => e.TrimStart('.').ToLowerInvariant()).ToList();
                var result = normalized.Select(e => $".{e}").ToList();

                // Add common MIME types for better filtering in some browsers
                if (normalized.Any(e => CFileTypeMap.Resolve(e) == CFileType.Image)) result.Insert(0, "image/*");
                if (normalized.Any(e => CFileTypeMap.Resolve(e) == CFileType.Video)) result.Insert(0, "video/*");
                if (normalized.Any(e => e == "pdf")) result.Insert(0, "application/pdf");

                return string.Join(",", result);
            }
        }

        protected string RootCssClass => new StringBuilder("htt-uploader")
            .Append(string.IsNullOrWhiteSpace(Class) ? string.Empty : $" {Class}")
            .ToString();

        protected string DropZoneCssClass => new StringBuilder("htt-uploader__dropzone")
            .Append(_isDragOver ? " htt-uploader__dropzone--drag-over" : string.Empty)
            .Append(IsQueueFull ? " htt-uploader__dropzone--full" : string.Empty)
            .ToString();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _dotNetRef = DotNetObjectReference.Create(this);
                // Register JS clipboard paste and drag-drop listener on the drop zone
                await JS.InvokeVoidAsync("HTTFileUploader.init", _dropZoneRef, _inputId, _dotNetRef);
            }
        }

        public override void Dispose()
        {
            // Cancel all in-flight uploads
            foreach (var cts in _ctsSources.Values)
            {
                cts.Cancel();
                cts.Dispose();
            }

            // Revoke blob URLs to free browser memory
            foreach (var url in _previewUrls.Values)
                _ = JS.InvokeVoidAsync("URL.revokeObjectURL", url);

            // Clean up JS listeners
            _ = JS.InvokeVoidAsync("HTTFileUploader.destroy", _dropZoneRef);
            _dotNetRef?.Dispose();

            base.Dispose();
        }

        // ═══════════════════════════════════════════════════════════
        //  Event Handlers — Local File Picker
        // ═══════════════════════════════════════════════════════════

        private async Task OnFilesSelected(InputFileChangeEventArgs e)
        {
            try
            {
                // We allow up to 100 files in the selection to avoid immediate crash,
                // but our internal validation will still enforce MaxFileCount.
                var selected = e.GetMultipleFiles(100).ToList();
                await EnqueueBrowserFilesAsync(selected, CFileSourceType.LocalFilePicker);
            }
            catch (InvalidOperationException)
            {
                // This happens if user selects > 100 files
                Toast.ShowCustom(new ToastModel
                {
                    Message  = L["FileUploader.Error.TooManyFilesSelected"],
                    Type     = CToastType.Error,
                    Duration = 4000
                });
            }
        }

        private Task OnDropZoneClick()
            => JS.InvokeVoidAsync("HTTFileUploader.triggerFilePicker", _inputId).AsTask();

        // ═══════════════════════════════════════════════════════════
        //  Event Handlers — Drag & Drop
        // ═══════════════════════════════════════════════════════════

        private void OnDragEnter() => _isDragOver = true;
        private void OnDragLeave() => _isDragOver = false;

        private async Task OnFilesDropped(InputFileChangeEventArgs e)
        {
            _isDragOver = false;
            var dropped = e.GetMultipleFiles(MaxFileCount + 10).ToList();
            await EnqueueBrowserFilesAsync(dropped, CFileSourceType.DragDrop);
        }

        // ═══════════════════════════════════════════════════════════
        //  Event Handlers — Clipboard (JS Interop Callback)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Called from JS when the user pastes a file via Ctrl+V.
        /// JS sends file metadata; actual bytes arrive via a hidden file input trick.
        /// </summary>
        [JSInvokable("OnClipboardPaste")]
        public async Task OnClipboardPaste(string fileName, long size, string contentType)
        {
            var ext = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
            var model = new UploadFileModel
            {
                FileName = fileName,
                SizeBytes = size,
                ContentType = contentType,
                FileType = CFileTypeMap.Resolve(ext),
                Source = CFileSourceType.Clipboard,
                Status = CUploadStatus.Pending
            };

            await EnqueueModelAsync(model, null);
        }

        // ═══════════════════════════════════════════════════════════
        //  Event Handlers — URL Import
        // ═══════════════════════════════════════════════════════════

        private async Task OnUrlImport()
        {
            if (string.IsNullOrWhiteSpace(_urlInput) || _isFetchingUrl) return;

            _isFetchingUrl = true;
            StateHasChanged();

            try
            {
                var uri = new Uri(_urlInput.Trim());
                var fileName = Path.GetFileName(uri.LocalPath);
                if (string.IsNullOrWhiteSpace(fileName)) fileName = "remote-file";

                var ext = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
                var model = new UploadFileModel
                {
                    FileName = fileName,
                    SizeBytes = 0,
                    ContentType = "application/octet-stream",
                    FileType = CFileTypeMap.Resolve(ext),
                    Source = CFileSourceType.RemoteUrl,
                    Status = CUploadStatus.Pending
                };

                await EnqueueModelAsync(model, null);
                _urlInput = string.Empty;
                _showUrlInput = false;
            }
            catch
            {
                Toast.ShowCustom(new ToastModel
                {
                    Message = L["FileUploader.Error.InvalidUrl"],
                    Type = CToastType.Error,
                    Duration = 4000
                });
            }
            finally
            {
                _isFetchingUrl = false;
                StateHasChanged();
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  Upload Orchestration
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Enqueues IBrowserFile entries from picker or drag-drop.
        /// Creates models, validates, generates previews, then auto-uploads if configured.
        /// </summary>
        private async Task EnqueueBrowserFilesAsync(
            IReadOnlyList<IBrowserFile> files,
            CFileSourceType source)
        {
            foreach (var file in files)
            {
                var ext = Path.GetExtension(file.Name).TrimStart('.').ToLowerInvariant();
                var model = new UploadFileModel
                {
                    FileName = file.Name,
                    SizeBytes = file.Size,
                    ContentType = file.ContentType,
                    FileType = CFileTypeMap.Resolve(ext),
                    Source = source,
                    Status = CUploadStatus.Pending
                };

                await EnqueueModelAsync(model, file);
            }
        }

        /// <summary>
        /// Core enqueue pipeline:
        ///   1. Validate (extension, size, count)
        ///   2. Generate image preview URL
        ///   3. Compute MD5 hash (optional)
        ///   4. Add to queue
        ///   5. Auto-upload if enabled
        /// </summary>
        private async Task EnqueueModelAsync(UploadFileModel model, IBrowserFile? browserFile)
        {
            // Validate against current queue
            var testQueue = _queue.Concat([model]).ToList();
            var errors = UploadService.Validate(testQueue, ValidationOptions);

            if (errors.TryGetValue(model.Id, out var errorKey))
            {
                model.Status = CUploadStatus.Failed;
                model.ErrorKey = errorKey;
                await OnValidationError.InvokeAsync((model, errorKey));

                Toast.ShowCustom(new ToastModel
                {
                    Message = L.GetString(errorKey, model.FileName),
                    Type = CToastType.Warning,
                    Duration = 4000
                });
                return;
            }

            // Register browser file reference for later stream reading
            if (browserFile is not null)
                _browserFiles[model.Id] = browserFile;

            // Generate image preview via JS blob URL
            if (model.FileType == CFileType.Image && browserFile is not null)
            {
                try
                {
                    var previewUrl = await JS.InvokeAsync<string>(
                        "HTTFileUploader.createPreviewUrl", _inputId, model.FileName);
                    if (!string.IsNullOrEmpty(previewUrl))
                        _previewUrls[model.Id] = previewUrl;
                }
                catch { /* Preview is non-critical — continue without it */ }
            }

            // MD5 hash (for duplicate detection)
            if (EnableHashing && browserFile is not null)
            {
                try
                {
                    using var stream = browserFile.OpenReadStream(MaxFileSizeMB * 1_048_576L);
                    model.Md5Hash = await UploadService.ComputeMd5Async(stream);
                }
                catch { /* Hash is non-critical */ }
            }

            _queue.Add(model);
            StateHasChanged();

            if (AutoUpload)
                await StartUploadAsync(model);
        }

        /// <summary>Starts all pending files in the queue.</summary>
        private async Task UploadAllAsync()
        {
            var pending = _queue
                .Where(f => f.Status is CUploadStatus.Pending or CUploadStatus.Failed)
                .ToList();

            var tasks = pending.Select(StartUploadAsync);
            await Task.WhenAll(tasks);

            if (AllTerminated)
                await OnUploadCompleted.InvokeAsync([.. _queue]);
        }

        /// <summary>Starts upload for a single file model.</summary>
        private async Task StartUploadAsync(UploadFileModel model)
        {
            if (!_browserFiles.TryGetValue(model.Id, out var browserFile))
            {
                model.Status = CUploadStatus.Failed;
                model.ErrorKey = "FileUploader.Error.NoStream";
                StateHasChanged();
                return;
            }

            // Clean up any existing CTS for this file (retry scenario)
            if (_ctsSources.TryGetValue(model.Id, out var oldCts))
            {
                oldCts.Cancel();
                oldCts.Dispose();
            }

            var cts = new CancellationTokenSource();
            _ctsSources[model.Id] = cts;

            var progress = new Progress<UploadFileModel>(_ =>
                InvokeAsync(StateHasChanged));

            try
            {
                using var stream = browserFile.OpenReadStream(MaxFileSizeMB * 1_048_576L, cts.Token);
                await UploadService.UploadAsync(
                    model,
                    stream,
                    UploadUrl,
                    ChunkSizeBytes,
                    progress,
                    cts.Token);
            }
            finally
            {
                _ctsSources.Remove(model.Id);
                await InvokeAsync(StateHasChanged);
            }
        }

        private async Task CancelUploadAsync(UploadFileModel model)
        {
            if (_ctsSources.TryGetValue(model.Id, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
                _ctsSources.Remove(model.Id);
            }

            model.Status = CUploadStatus.Cancelled;
            StateHasChanged();
        }

        private async Task RetryUploadAsync(UploadFileModel model)
        {
            model.Status = CUploadStatus.Pending;
            model.ProgressPercent = 0;
            model.BytesUploaded = 0;
            model.ErrorKey = null;
            model.ErrorDetail = null;
            StateHasChanged();

            await StartUploadAsync(model);
        }

        private async Task RemoveFileAsync(UploadFileModel model)
        {
            // Cancel if uploading
            if (_ctsSources.TryGetValue(model.Id, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
                _ctsSources.Remove(model.Id);
            }

            // Revoke preview blob URL
            if (_previewUrls.TryGetValue(model.Id, out var url))
            {
                await JS.InvokeVoidAsync("URL.revokeObjectURL", url);
                _previewUrls.Remove(model.Id);
            }

            _browserFiles.Remove(model.Id);
            _queue.Remove(model);

            await OnFileRemoved.InvokeAsync(model);
            StateHasChanged();
        }

        private async Task ClearAllAsync()
        {
            // Cancel all active uploads
            foreach (var cts in _ctsSources.Values)
            {
                cts.Cancel();
                cts.Dispose();
            }

            _ctsSources.Clear();

            // Revoke all preview URLs
            foreach (var url in _previewUrls.Values)
                await JS.InvokeVoidAsync("URL.revokeObjectURL", url);

            _previewUrls.Clear();
            _browserFiles.Clear();
            _queue.Clear();
            StateHasChanged();
        }

        // ═══════════════════════════════════════════════════════════
        //  Helper Methods
        // ═══════════════════════════════════════════════════════════

        private string GetStatusBadgeClass(CUploadStatus status) => status switch
        {
            CUploadStatus.Pending => "htt-uploader__badge htt-uploader__badge--pending",
            CUploadStatus.Uploading => "htt-uploader__badge htt-uploader__badge--uploading",
            CUploadStatus.Completed => "htt-uploader__badge htt-uploader__badge--completed",
            CUploadStatus.Failed => "htt-uploader__badge htt-uploader__badge--failed",
            CUploadStatus.Cancelled => "htt-uploader__badge htt-uploader__badge--cancelled",
            _ => "htt-uploader__badge"
        };

        private string GetStatusLabelKey(CUploadStatus status) => status switch
        {
            CUploadStatus.Pending => "FileUploader.Status.Pending",
            CUploadStatus.Uploading => "FileUploader.Status.Uploading",
            CUploadStatus.Completed => "FileUploader.Status.Completed",
            CUploadStatus.Failed => "FileUploader.Status.Failed",
            CUploadStatus.Cancelled => "FileUploader.Status.Cancelled",
            _ => string.Empty
        };

        private CButtonVariant GetProgressVariant(CUploadStatus status) => status switch
        {
            CUploadStatus.Uploading => CButtonVariant.Primary,
            CUploadStatus.Completed => CButtonVariant.Success,
            CUploadStatus.Failed => CButtonVariant.Danger,
            CUploadStatus.Cancelled => CButtonVariant.Warning,
            _ => CButtonVariant.Secondary
        };

        private string GetPreviewUrl(string fileId)
            => _previewUrls.TryGetValue(fileId, out var url) ? url : string.Empty;

        private bool HasPreview(UploadFileModel model)
            => model.FileType == CFileType.Image && _previewUrls.ContainsKey(model.Id);
    }
}
