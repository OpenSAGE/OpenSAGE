using LLGfx;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Rendering;

namespace OpenSage.Graphics
{
    public sealed class SpriteComponent : RenderableComponent
    {
        private SpriteEffect _effect;

        private EffectPipelineStateHandle _pipelineStateHandle;

        public Texture Texture { get; set; }
        public uint SelectedMipMapLevel { get; set; }

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
            renderList.AddRenderItem(new RenderItem
            {
                Renderable = this,
                Effect = _effect,
                PipelineStateHandle = _pipelineStateHandle,
                RenderCallback = (commandEncoder, effect, pipelineStateHandle) =>
                {
                    _effect.SetTexture(Texture);
                    _effect.SetMipMapLevel(SelectedMipMapLevel);

                    _effect.Apply(commandEncoder);

                    commandEncoder.Draw(PrimitiveType.TriangleList, 0, 3);
                }
            });
        }
    }
}
