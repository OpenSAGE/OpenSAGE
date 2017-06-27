using System.IO;
using OpenZH.Data.Big;

namespace OpenZH.DataViewer.ViewModels
{
	public class ArchiveEntryViewModel : ItemViewModel
    {
		public BigArchiveEntry Item { get; set; }

        public override string DisplayName => Item.FullName;
        public override string GroupName => "Files";

        public ArchiveEntryViewModel(BigArchiveEntry item)
		{
			Item = item;
		}

        public static ArchiveEntryViewModel Create(BigArchiveEntry archiveEntry)
        {
            switch (Path.GetExtension(archiveEntry.FullName).ToLower())
            {
                case ".dds":
                    return new DdsArchiveEntryViewModel(archiveEntry);

                case ".wav":
                    return new WavArchiveEntryViewModel(archiveEntry);

                case ".w3d":
                    return new W3dArchiveEntryViewModel(archiveEntry);

                default:
                    return new ArchiveEntryViewModel(archiveEntry);
            }
        }
	}
}