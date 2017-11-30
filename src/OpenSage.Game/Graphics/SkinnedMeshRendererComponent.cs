using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Rendering;
using OpenSage.Mathematics;

namespace OpenSage.Graphics
{
    public sealed class SkinnedMeshComponent : RenderableComponent
    {
        private MeshMaterial _material;

        public ModelMesh Mesh { get; set; }

        internal override ModelMesh MeshBase => Mesh;

        internal override BoundingBox LocalBoundingBox => Mesh.BoundingBox;

        protected override void Start()
        {
            base.Start();

            _material = new MeshMaterial(ContentManager.GetEffect<MeshEffect>());
        }

        internal override void BuildRenderList(RenderList renderList)
        {
            renderList.AddInstancedRenderItem(Mesh, this, _material);
        }
    }
}
