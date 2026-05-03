using Microsoft.AspNetCore.Components.Forms;

namespace HTT.BlazorWasm.App.Services.FileSourceProviders
{
    /// <summary>
    /// File source provider for local OS file picker (input[type=file]).
    /// This provider is driven directly by the InputFile component in HTTFileUploader —
    /// it wraps IBrowserFile references into UploadFileModel objects.
    /// GetFilesAsync is not used for this provider (files arrive via OnChange event).
    /// </summary>
    public sealed class LocalFilePickerProvider : FileSourceProviderBase
    {
        public override CFileSourceType SourceType => CFileSourceType.LocalFilePicker;
        public override string LabelKey  => "FileUploader.Source.LocalFile";
        public override string IconClass => "bi bi-folder2-open";

        /// <summary>
        /// Convert IBrowserFile list to UploadFileModel list.
        /// Called from HTTFileUploader.OnFilesSelected.
        /// </summary>
        public static IReadOnlyList<UploadFileModel> Convert(IReadOnlyList<IBrowserFile> files)
        {
            return files
                .Select(f => CreateModel(f.Name, f.Size, f.ContentType, CFileSourceType.LocalFilePicker))
                .ToList();
        }

        /// <inheritdoc/>
        public override Task<IReadOnlyList<UploadFileModel>> GetFilesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<UploadFileModel>>([]);
    }
}
