using LLGfx;
using LLGfx.Effects;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Rendering;
using OpenSage.Mathematics;

namespace OpenSage.Graphics
{
    public sealed class SpriteComponent : RenderableComponent
    {
        private SpriteEffect _effect;

        private EffectPipelineStateHandle _pipelineStateHandle;

        public Texture Texture { get; set; }
        public uint SelectedMipMapLevel { get; set; }

        internal override bool IsAlwaysVisible => true;

        internal override BoundingBox LocalBoundingBox => new BoundingBox();

        protected override void Start()
        {
            base.Start();

            _effect = ContentManager.GetEffect<SpriteEffect>();

            var rasterizerState = RasterizerStateDescription.CullBackSolid;
            rasterizerState.IsFrontCounterClockwise = false;

            _pipelineStateHandle = new EffectPipelineState(
                rasterizerState,
                DepthStencilStateDescription.None,
                BlendStateDescription.Opaque)
                .GetHandle();
        }

        internal override void BuildRenderList(RenderList renderList)
        {
            renderList.AddRenderItem(new RenderItem(
                this,
                _effect,
                _pipelineStateHandle,
                (commandEncoder, effect, pipelineStateHandle, instanceData) =>
                {
                    _effect.SetTexture(Texture);
                    _effect.SetMipMapLevel(SelectedMipMapLevel);

                    _effect.Apply(commandEncoder);

                    commandEncoder.Draw(PrimitiveType.TriangleList, 0, 3);
                }));
        }
    }
}
