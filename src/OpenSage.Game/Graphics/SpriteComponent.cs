using LLGfx;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Rendering;
using OpenSage.Mathematics;

namespace OpenSage.Graphics
{
    public sealed class SpriteComponent : RenderableComponent
    {
        private SpriteMaterial _material;

        private EffectPipelineStateHandle _pipelineStateHandle;

        public Texture Texture { get; set; }

        private uint _selectedMipMapLevel;
        public uint SelectedMipMapLevel
        {
            get { return _selectedMipMapLevel; }
            set
            {
                _selectedMipMapLevel = value;
                _material.SetMipMapLevel(value);
            }
        }

        internal override bool IsAlwaysVisible => true;

        internal override BoundingBox LocalBoundingBox => new BoundingBox();

        protected override void Start()
        {
            base.Start();

            _material = new SpriteMaterial(ContentManager.GetEffect<SpriteEffect>());
            _material.SetTexture(Texture);

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
                _material,
                _pipelineStateHandle,
                (commandEncoder, effect, pipelineStateHandle, instanceData) =>
                {
                    effect.Apply(commandEncoder);

                    commandEncoder.Draw(PrimitiveType.TriangleList, 0, 3);
                }));
        }
    }
}
