namespace HTT.BlazorWasm.App.Contracts
{
    /// <summary>
    /// Categorizes uploaded files by their semantic type.
    /// Used by CFileTypeMap for extension-to-type resolution and
    /// by HTTFileUploader for icon/preview rendering decisions.
    /// </summary>
    public enum CFileType
    {
        /// <summary>Raster and vector images: .png, .jpg, .jpeg, .webp, .gif, .svg, .bmp, .ico</summary>
        Image,

        /// <summary>Video media: .mp4, .mov, .avi, .mkv, .webm, .flv</summary>
        Video,

        /// <summary>Office / productivity documents: .pdf, .docx, .doc, .xlsx, .xls, .pptx, .ppt, .txt, .csv, .md</summary>
        Document,

        /// <summary>Compressed archives: .zip, .rar, .7z, .tar, .gz, .bz2</summary>
        Archive,

        /// <summary>Any extension not covered by the above categories.</summary>
        Other
    }
}
