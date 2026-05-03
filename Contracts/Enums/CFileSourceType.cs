namespace HTT.BlazorWasm.App.Contracts
{
    /// <summary>
    /// Identifies the source channel through which a file entered the uploader.
    /// Enables analytics, audit logging, and source-specific UI hints.
    /// </summary>
    public enum CFileSourceType
    {
        /// <summary>Opened via the OS file-picker dialog.</summary>
        LocalFilePicker,

        /// <summary>Dropped onto the drag-and-drop zone.</summary>
        DragDrop,

        /// <summary>Pasted from the clipboard (Ctrl+V).</summary>
        Clipboard,

        /// <summary>Downloaded from a remote URL entered by the user.</summary>
        RemoteUrl,

        /// <summary>Imported from Google Drive (extensible mock).</summary>
        GoogleDrive,

        /// <summary>Imported from Microsoft OneDrive (extensible mock).</summary>
        OneDrive
    }
}
