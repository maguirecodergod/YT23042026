namespace HTT.BlazorWasm.App.Contracts
{
    /// <summary>
    /// Rich model representing a single file item within the HTTFileUploader queue.
    /// Carries identity, upload progress, speed metrics, and result metadata.
    /// 
    /// Lifecycle:
    ///   Pending → Uploading → Completed
    ///                      ↘ Failed → (Retry) → Uploading
    ///                      ↘ Cancelled
    /// </summary>
    public sealed class UploadFileModel
    {
        // ═══════════════════════════════════════════════════════════
        //  Identity
        // ═══════════════════════════════════════════════════════════

        /// <summary>Stable client-side identifier (GUID). Stable across retries.</summary>
        public string Id { get; init; } = Guid.NewGuid().ToString("N");

        /// <summary>Original file name including extension.</summary>
        public string FileName { get; init; } = string.Empty;

        /// <summary>MIME type as reported by the browser.</summary>
        public string ContentType { get; init; } = string.Empty;

        /// <summary>File size in bytes.</summary>
        public long SizeBytes { get; init; }

        /// <summary>Semantic category resolved by CFileTypeMap.</summary>
        public CFileType FileType { get; init; } = CFileType.Other;

        /// <summary>Channel through which this file entered the queue.</summary>
        public CFileSourceType Source { get; init; } = CFileSourceType.LocalFilePicker;

        // ═══════════════════════════════════════════════════════════
        //  Upload Progress (mutable — updated by IFileUploadService)
        // ═══════════════════════════════════════════════════════════

        /// <summary>Current lifecycle state.</summary>
        public CUploadStatus Status { get; set; } = CUploadStatus.Pending;

        /// <summary>Upload progress in percentage [0–100].</summary>
        public double ProgressPercent { get; set; }

        /// <summary>Current upload speed in bytes per second.</summary>
        public long SpeedBytesPerSecond { get; set; }

        /// <summary>Estimated seconds remaining at current speed. Null when speed is zero.</summary>
        public double? EstimatedSecondsRemaining { get; set; }

        /// <summary>Number of bytes uploaded so far.</summary>
        public long BytesUploaded { get; set; }

        // ═══════════════════════════════════════════════════════════
        //  Result & Error
        // ═══════════════════════════════════════════════════════════

        /// <summary>Server-assigned URL or resource identifier after successful upload.</summary>
        public string? RemoteUrl { get; set; }

        /// <summary>
        /// MD5 hash computed client-side before upload (hex string).
        /// Used for duplicate detection and server-side integrity check.
        /// </summary>
        public string? Md5Hash { get; set; }

        /// <summary>Localization key of the last validation or network error.</summary>
        public string? ErrorKey { get; set; }

        /// <summary>Raw error message from the server (not localized — for dev logs only).</summary>
        public string? ErrorDetail { get; set; }

        /// <summary>Timestamp of the last status transition.</summary>
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        // ═══════════════════════════════════════════════════════════
        //  Computed Helpers
        // ═══════════════════════════════════════════════════════════

        /// <summary>Human-readable file size string. Example: "4.2 MB", "512 KB".</summary>
        public string FormattedSize => FormatBytes(SizeBytes);

        /// <summary>Human-readable speed string. Example: "1.2 MB/s".</summary>
        public string FormattedSpeed => SpeedBytesPerSecond > 0
            ? $"{FormatBytes(SpeedBytesPerSecond)}/s"
            : string.Empty;

        /// <summary>Human-readable ETA. Example: "2m 30s", "45s".</summary>
        public string FormattedEta
        {
            get
            {
                if (EstimatedSecondsRemaining is not { } secs || secs <= 0) return string.Empty;
                var ts = TimeSpan.FromSeconds(secs);
                return ts.TotalMinutes >= 1
                    ? $"{(int)ts.TotalMinutes}m {ts.Seconds}s"
                    : $"{ts.Seconds}s";
            }
        }

        /// <summary>File extension (lowercase, without leading dot).</summary>
        public string Extension => Path.GetExtension(FileName).TrimStart('.').ToLowerInvariant();

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1_048_576) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1_073_741_824) return $"{bytes / 1_048_576.0:F1} MB";
            return $"{bytes / 1_073_741_824.0:F2} GB";
        }
    }
}
