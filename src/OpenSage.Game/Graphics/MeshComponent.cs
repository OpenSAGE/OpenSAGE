using OpenSage.Graphics.Rendering;
using OpenSage.Mathematics;

namespace OpenSage.Graphics
{
    public sealed class MeshComponent : RenderableComponent
    {
        public ModelMesh Mesh { get; set; }

        internal override ModelMesh MeshBase => Mesh;

        internal override BoundingBox LocalBoundingBox => Mesh.BoundingBox;

        internal override void BuildRenderList(RenderList renderList)
        {
            Mesh.BuildRenderList(renderList, this);
        }
    }
}
