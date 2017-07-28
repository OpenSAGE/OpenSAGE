using OpenZH.Data.W3d;
using System.IO;
using OpenZH.Data;

namespace OpenZH.DataViewer.ViewModels
{
    public sealed class W3dArchiveEntryViewModel : ArchiveEntryViewModel
    {
        public W3dArchiveEntryViewModel(FileSystemEntry archiveEntry)
            : base(archiveEntry)
        {
        }

        protected override void CreateChildren()
        {
            W3dFile w3dFile;

            using (var stream = Item.Open())
            using (var binaryReader = new BinaryReader(stream))
                w3dFile = W3dFile.Parse(binaryReader);

            foreach (var mesh in w3dFile.Meshes)
                Children.Add(new W3dMeshItemViewModel(mesh));

            // TODO
        }
    }
}
