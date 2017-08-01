using OpenZH.Data;
using OpenZH.Data.W3d;

namespace OpenZH.DataViewer.ViewModels
{
    public sealed class W3dMeshItemViewModel : ItemViewModel
    {
        public FileSystemEntry ParentItem { get; }
        public W3dMesh Mesh { get; }

        public override string DisplayName => Mesh.Header.MeshName;
        public override string GroupName => "Meshes";

        public W3dMeshItemViewModel(FileSystemEntry parentItem, W3dMesh mesh)
        {
            ParentItem = parentItem;
            Mesh = mesh;
        }
    }
}
