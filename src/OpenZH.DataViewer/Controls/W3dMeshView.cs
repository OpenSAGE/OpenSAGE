using OpenZH.Data.W3d;
using Xamarin.Forms;

namespace OpenZH.DataViewer.Controls
{
    public class W3dMeshView : View
    {
        public static readonly BindableProperty MeshProperty = BindableProperty.Create(
            nameof(Mesh), typeof(W3dMesh), typeof(W3dMeshView));

        public W3dMesh Mesh
        {
            get { return (W3dMesh) GetValue(MeshProperty); }
            set { SetValue(MeshProperty, value); }
        }
    }
}
