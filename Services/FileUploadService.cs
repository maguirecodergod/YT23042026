using System.Diagnostics;
using System.Security.Cryptography;

namespace HTT.BlazorWasm.App.Services
{
    /// <summary>
    /// Enterprise upload engine implementing IFileUploadService.
    /// 
    /// Features:
    ///   – Chunk-based streaming upload with configurable chunk size
    ///   – Real-time speed and ETA calculation via rolling window
    ///   – Per-file CancellationToken for independent cancellation
    ///   – MD5 hash computation for duplicate detection
    ///   – Progress reported via IProgress[UploadFileModel] to keep UI decoupled
    ///   – Validates extension, file size, and queue count
    ///
    /// Note: In WASM, System.Security.Cryptography.MD5 is available (Blazor 6+).
    /// For older runtimes, swap with a managed implementation.
    /// </summary>
    internal sealed class FileUploadService : IFileUploadService
    {
        private const int SpeedWindowMs = 1000; // rolling window for speed calculation

        // ═══════════════════════════════════════════════════════════
        //  IFileUploadService — Validate
        // ═══════════════════════════════════════════════════════════

        /// <inheritdoc/>
        public Dictionary<string, string> Validate(
            IReadOnlyList<UploadFileModel> files,
            FileUploadOptions options)
        {
            var errors = new Dictionary<string, string>();

            // Rule 1: Queue count limit
            if (files.Count > options.MaxFileCount)
            {
                // Flag the excess files with a count error
                foreach (var excess in files.Skip(options.MaxFileCount))
                    errors[excess.Id] = "FileUploader.Error.MaxFileCount";
            }

            long maxBytes = (long)options.MaxFileSizeMB * 1_048_576;

            foreach (var file in files.Take(options.MaxFileCount))
            {
                // Rule 2: Extension whitelist
                if (options.AllowedExtensions.Count > 0
                    && !options.AllowedExtensions.Contains(file.Extension, StringComparer.OrdinalIgnoreCase))
                {
                    errors[file.Id] = "FileUploader.Error.InvalidExtension";
                    continue;
                }

                // Rule 3: File size limit
                if (file.SizeBytes > maxBytes)
                {
                    errors[file.Id] = "FileUploader.Error.FileTooLarge";
                }
            }

            return errors;
        }

        // ═══════════════════════════════════════════════════════════
        //  IFileUploadService — ComputeMd5Async
        // ═══════════════════════════════════════════════════════════

        /// <inheritdoc/>
        public async Task<string> ComputeMd5Async(Stream stream, CancellationToken ct = default)
        {
            // IncrementalHash is WASM-safe (unlike MD5.Create() which uses native crypto on browser).
            using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
            var buffer = new byte[81_920]; // 80 KB read buffer
            int read;

            while ((read = await stream.ReadAsync(buffer, ct).ConfigureAwait(false)) > 0)
            {
                hasher.AppendData(buffer, 0, read);
            }

            var hashBytes = hasher.GetHashAndReset();
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        // ═══════════════════════════════════════════════════════════
        //  IFileUploadService — UploadAsync (Chunk Streaming)
        // ═══════════════════════════════════════════════════════════

        /// <inheritdoc/>
        public async Task UploadAsync(
            UploadFileModel model,
            Stream stream,
            string uploadUrl,
            int chunkSizeBytes = 524_288,
            IProgress<UploadFileModel>? progress = null,
            CancellationToken ct = default)
        {
            model.Status = CUploadStatus.Uploading;
            model.ProgressPercent = 0;
            model.BytesUploaded = 0;
            model.SpeedBytesPerSecond = 0;
            model.EstimatedSecondsRemaining = null;
            model.UpdatedAt = DateTimeOffset.UtcNow;

            progress?.Report(model);

            var totalBytes = model.SizeBytes;
            var buffer = new byte[chunkSizeBytes];
            int chunkIndex = 0;
            var stopwatch = Stopwatch.StartNew();
            long lastReportedBytes = 0;
            var lastWindowTime = stopwatch.Elapsed;

            try
            {
                while (true)
                {
                    ct.ThrowIfCancellationRequested();

                    int read = await stream.ReadAsync(buffer, ct).ConfigureAwait(false);
                    if (read == 0) break;

                    // Simulate actual HTTP chunk upload to uploadUrl
                    // Replace with real HttpClient.PostAsync in production
                    await SimulateChunkUploadAsync(buffer, read, chunkIndex, ct);

                    model.BytesUploaded += read;
                    chunkIndex++;

                    // Speed calculation: rolling window
                    var now = stopwatch.Elapsed;
                    var windowElapsed = (now - lastWindowTime).TotalSeconds;

                    if (windowElapsed >= 0.2) // update every 200ms
                    {
                        var bytesInWindow = model.BytesUploaded - lastReportedBytes;
                        model.SpeedBytesPerSecond = windowElapsed > 0
                            ? (long)(bytesInWindow / windowElapsed)
                            : 0;

                        lastReportedBytes = model.BytesUploaded;
                        lastWindowTime = now;
                    }

                    // Progress and ETA
                    model.ProgressPercent = totalBytes > 0
                        ? Math.Clamp((double)model.BytesUploaded / totalBytes * 100, 0, 100)
                        : 0;

                    model.EstimatedSecondsRemaining = model.SpeedBytesPerSecond > 0
                        ? (double)(totalBytes - model.BytesUploaded) / model.SpeedBytesPerSecond
                        : null;

                    model.UpdatedAt = DateTimeOffset.UtcNow;
                    progress?.Report(model);
                }

                // Finalize
                model.ProgressPercent = 100;
                model.SpeedBytesPerSecond = 0;
                model.EstimatedSecondsRemaining = null;
                model.Status = CUploadStatus.Completed;
                model.BytesUploaded = totalBytes;
                model.UpdatedAt = DateTimeOffset.UtcNow;
                progress?.Report(model);
            }
            catch (OperationCanceledException)
            {
                model.Status = CUploadStatus.Cancelled;
                model.UpdatedAt = DateTimeOffset.UtcNow;
                progress?.Report(model);
            }
            catch (Exception ex)
            {
                model.Status = CUploadStatus.Failed;
                model.ErrorDetail = ex.Message;
                model.ErrorKey = "FileUploader.Error.UploadFailed";
                model.UpdatedAt = DateTimeOffset.UtcNow;
                progress?.Report(model);
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  Private — Simulated Chunk Upload (replace with real HTTP)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Simulates a chunked HTTP POST with artificial latency.
        /// REPLACE THIS with a real HttpClient multipart/form-data or
        /// binary streaming request to your backend endpoint.
        /// 
        /// Example production implementation:
        ///   var content = new ByteArrayContent(buffer, 0, bytesRead);
        ///   content.Headers.Add("X-Chunk-Index", chunkIndex.ToString());
        ///   await _httpClient.PostAsync(uploadUrl, content, ct);
        /// </summary>
        private static async Task SimulateChunkUploadAsync(byte[] buffer, int count, int index, CancellationToken ct)
        {
            // Simulate ~2MB/s network transfer: 512KB chunk ≈ 250ms
            var delayMs = (int)((double)count / (2_097_152.0) * 1000);
            await Task.Delay(Math.Max(delayMs, 10), ct).ConfigureAwait(false);
        }
    }
}
