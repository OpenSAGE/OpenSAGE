using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Rendering;
using OpenSage.Mathematics;

namespace OpenSage.Graphics
{
    public sealed class MeshComponent : RenderableComponent
    {
        private MeshEffect _effect;

        public ModelMesh Mesh { get; set; }

        internal override ModelMesh MeshBase => Mesh;

        internal override BoundingBox LocalBoundingBox => Mesh.BoundingBox;

        protected override void Start()
        {
            base.Start();

            _effect = ContentManager.GetEffect<MeshEffect>();
        }

        internal override void BuildRenderList(RenderList renderList)
        {
            renderList.AddInstancedRenderItem(Mesh, this, _effect);
        }
    }
}
