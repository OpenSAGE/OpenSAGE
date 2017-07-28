using System.IO;
using OpenZH.Data;

namespace OpenZH.DataViewer.ViewModels
{
	public class ArchiveEntryViewModel : ItemViewModel
    {
		public FileSystemEntry Item { get; set; }

        public override string DisplayName => Item.FilePath;
        public override string GroupName => "Files";

        public ArchiveEntryViewModel(FileSystemEntry item)
		{
			Item = item;
		}

        public static ArchiveEntryViewModel Create(FileSystemEntry file)
        {
            switch (Path.GetExtension(file.FilePath).ToLower())
            {
                case ".dds":
                    return new TextureArchiveEntryViewModel(file, TextureFormat.Dds);

                case ".tga":
                    return new TextureArchiveEntryViewModel(file, TextureFormat.Tga);

                case ".wav":
                    return new WavArchiveEntryViewModel(file);

                case ".w3d":
                    return new W3dArchiveEntryViewModel(file);

                default:
                    return new ArchiveEntryViewModel(file);
            }
        }
	}
}