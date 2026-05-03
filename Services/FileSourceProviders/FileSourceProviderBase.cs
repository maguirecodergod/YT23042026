namespace HTT.BlazorWasm.App.Services.FileSourceProviders
{
    /// <summary>
    /// Base class for file source providers.
    /// Implements common logic: file model factory, MIME resolution, type mapping.
    /// Concrete providers override GetFilesAsync only.
    /// </summary>
    public abstract class FileSourceProviderBase : IFileSourceProvider
    {
        public abstract CFileSourceType SourceType { get; }
        public abstract string LabelKey { get; }
        public abstract string IconClass { get; }
        public virtual bool IsAvailable => true;

        public abstract Task<IReadOnlyList<UploadFileModel>> GetFilesAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Factory: creates an UploadFileModel from raw file metadata.
        /// Resolves CFileType via CFileTypeMap for icon/preview decisions.
        /// </summary>
        protected static UploadFileModel CreateModel(
            string fileName,
            long sizeBytes,
            string contentType,
            CFileSourceType source)
        {
            var ext = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
            return new UploadFileModel
            {
                FileName    = fileName,
                SizeBytes   = sizeBytes,
                ContentType = contentType,
                FileType    = CFileTypeMap.Resolve(ext),
                Source      = source,
                Status      = CUploadStatus.Pending
            };
        }
    }
}
