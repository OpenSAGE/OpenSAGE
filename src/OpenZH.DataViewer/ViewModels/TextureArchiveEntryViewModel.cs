using OpenZH.Data;

namespace OpenZH.DataViewer.ViewModels
{
    public sealed class TextureArchiveEntryViewModel : ArchiveEntryViewModel
    {
        public TextureFormat TextureFormat { get; }

        public TextureArchiveEntryViewModel(FileSystemEntry archiveEntry, TextureFormat textureFormat)
            : base(archiveEntry)
        {
            TextureFormat = textureFormat;
        }
    }

    public enum TextureFormat
    {
        Dds,
        Tga
    }
}
