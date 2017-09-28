using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Rendering;

namespace OpenSage.Graphics
{
    public sealed class MeshComponent : RenderableComponent
    {
        private MeshEffect _effect;

        public ModelMesh Mesh { get; set; }

        protected override void Start()
        {
            base.Start();

            _effect = ContentManager.GetEffect<MeshEffect>();
        }

        internal override void BuildRenderList(RenderList renderList)
        {
            Mesh.BuildRenderList(renderList, this, _effect);
        }
    }
}
