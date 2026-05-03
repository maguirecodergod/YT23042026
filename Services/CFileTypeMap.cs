namespace HTT.BlazorWasm.App.Services
{
    /// <summary>
    /// Central registry mapping file extensions to CFileType categories.
    /// Used by HTTFileUploader to resolve file type for icons, validation, and preview.
    /// 
    /// Design: static sealed class (no instance needed) with O(1) dictionary lookup.
    /// Extension is normalized to lowercase without leading dot.
    /// </summary>
    public static class CFileTypeMap
    {
        private static readonly IReadOnlyDictionary<string, CFileType> _map =
            new Dictionary<string, CFileType>(StringComparer.OrdinalIgnoreCase)
            {
                // ── Images ──────────────────────────────────────────────
                ["png"]  = CFileType.Image,
                ["jpg"]  = CFileType.Image,
                ["jpeg"] = CFileType.Image,
                ["webp"] = CFileType.Image,
                ["gif"]  = CFileType.Image,
                ["svg"]  = CFileType.Image,
                ["bmp"]  = CFileType.Image,
                ["ico"]  = CFileType.Image,
                ["tiff"] = CFileType.Image,
                ["avif"] = CFileType.Image,

                // ── Videos ──────────────────────────────────────────────
                ["mp4"]  = CFileType.Video,
                ["mov"]  = CFileType.Video,
                ["avi"]  = CFileType.Video,
                ["mkv"]  = CFileType.Video,
                ["webm"] = CFileType.Video,
                ["flv"]  = CFileType.Video,
                ["wmv"]  = CFileType.Video,
                ["m4v"]  = CFileType.Video,

                // ── Documents ───────────────────────────────────────────
                ["pdf"]  = CFileType.Document,
                ["docx"] = CFileType.Document,
                ["doc"]  = CFileType.Document,
                ["xlsx"] = CFileType.Document,
                ["xls"]  = CFileType.Document,
                ["pptx"] = CFileType.Document,
                ["ppt"]  = CFileType.Document,
                ["txt"]  = CFileType.Document,
                ["csv"]  = CFileType.Document,
                ["md"]   = CFileType.Document,
                ["rtf"]  = CFileType.Document,
                ["odt"]  = CFileType.Document,
                ["ods"]  = CFileType.Document,

                // ── Archives ─────────────────────────────────────────────
                ["zip"]  = CFileType.Archive,
                ["rar"]  = CFileType.Archive,
                ["7z"]   = CFileType.Archive,
                ["tar"]  = CFileType.Archive,
                ["gz"]   = CFileType.Archive,
                ["bz2"]  = CFileType.Archive,
                ["xz"]   = CFileType.Archive,
            };

        /// <summary>
        /// Resolves a file extension to its semantic CFileType category.
        /// </summary>
        /// <param name="extension">Extension with or without leading dot (case-insensitive).</param>
        /// <returns>Matched CFileType, or CFileType.Other for unknown extensions.</returns>
        public static CFileType Resolve(string extension)
        {
            var normalized = extension.TrimStart('.').ToLowerInvariant();
            return _map.TryGetValue(normalized, out var type) ? type : CFileType.Other;
        }

        /// <summary>
        /// Returns the Bootstrap icon class appropriate for the given CFileType.
        /// Used by the file list item to render the file-type icon.
        /// </summary>
        public static string GetIconClass(CFileType type, string extension) => type switch
        {
            CFileType.Image    => "bi bi-file-earmark-image",
            CFileType.Video    => "bi bi-file-earmark-play",
            CFileType.Archive  => "bi bi-file-earmark-zip",
            CFileType.Document => GetDocumentIcon(extension),
            _                  => "bi bi-file-earmark"
        };

        private static string GetDocumentIcon(string ext) => ext.ToLowerInvariant() switch
        {
            "pdf"             => "bi bi-file-earmark-pdf",
            "xlsx" or "xls" or "csv" => "bi bi-file-earmark-excel",
            "docx" or "doc" or "rtf" => "bi bi-file-earmark-word",
            "pptx" or "ppt"  => "bi bi-file-earmark-ppt",
            "txt" or "md"    => "bi bi-file-earmark-text",
            _                => "bi bi-file-earmark-richtext"
        };

        /// <summary>
        /// Returns the CSS color variable class for the icon based on file type.
        /// </summary>
        public static string GetIconColorClass(CFileType type, string extension) => type switch
        {
            CFileType.Image    => "htt-file-icon--image",
            CFileType.Video    => "htt-file-icon--video",
            CFileType.Archive  => "htt-file-icon--archive",
            CFileType.Document => GetDocumentColorClass(extension),
            _                  => "htt-file-icon--other"
        };

        private static string GetDocumentColorClass(string ext) => ext.ToLowerInvariant() switch
        {
            "pdf"             => "htt-file-icon--pdf",
            "xlsx" or "xls" or "csv" => "htt-file-icon--excel",
            "docx" or "doc" or "rtf" => "htt-file-icon--word",
            "pptx" or "ppt"  => "htt-file-icon--ppt",
            _                => "htt-file-icon--document"
        };
    }
}
