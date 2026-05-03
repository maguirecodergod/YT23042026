namespace HTT.BlazorWasm.App.Contracts
{
    /// <summary>
    /// Represents the upload lifecycle state of a single file in HTTFileUploader.
    /// Drives progress bar variant, action button visibility, and status badge rendering.
    /// </summary>
    public enum CUploadStatus
    {
        /// <summary>File has been staged but upload has not started.</summary>
        Pending,

        /// <summary>Upload is actively in progress (chunked or streaming).</summary>
        Uploading,

        /// <summary>Upload completed successfully.</summary>
        Completed,

        /// <summary>Upload failed. Retry action is exposed.</summary>
        Failed,

        /// <summary>Upload was cancelled by the user.</summary>
        Cancelled
    }
}
