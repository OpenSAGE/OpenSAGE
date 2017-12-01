using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Rendering;
using OpenSage.Mathematics;

namespace OpenSage.Graphics
{
    public sealed class SkinnedMeshComponent : RenderableComponent
    {
        public ModelMesh Mesh { get; set; }

        internal override ModelMesh MeshBase => Mesh;

        internal override BoundingBox LocalBoundingBox => Mesh.BoundingBox;

        internal override void BuildRenderList(RenderList renderList)
        {
            renderList.AddInstancedRenderItem(Mesh, this, ContentManager.GetEffect<MeshEffect>());
        }
    }
}
