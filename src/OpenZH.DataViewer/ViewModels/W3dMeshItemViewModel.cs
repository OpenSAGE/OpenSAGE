using OpenZH.Data.W3d;

namespace OpenZH.DataViewer.ViewModels
{
    public sealed class W3dMeshItemViewModel : ItemViewModel
    {
        public W3dMesh Mesh { get; }

        public override string DisplayName => Mesh.Header.MeshName;
        public override string GroupName => "Meshes";

        public W3dMeshItemViewModel(W3dMesh mesh)
        {
            Mesh = mesh;
        }
    }
}
