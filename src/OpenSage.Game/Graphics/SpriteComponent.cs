using LL.Graphics3D;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Rendering;
using OpenSage.Mathematics;

namespace OpenSage.Graphics
{
    public sealed class SpriteComponent : RenderableComponent
    {
        private SpriteBatch _spriteBatch;
        
        private EffectPipelineStateHandle _pipelineStateHandle;

        public Texture Texture { get; set; }

        public uint SelectedMipMapLevel { get; set; }

        internal override bool IsAlwaysVisible => true;

        internal override BoundingBox LocalBoundingBox => new BoundingBox();

        protected override void Start()
        {
            base.Start();

            _spriteBatch = new SpriteBatch(ContentManager);

            var rasterizerState = RasterizerStateDescription.CullBackSolid;
            rasterizerState.IsFrontCounterClockwise = false;

            _pipelineStateHandle = new EffectPipelineState(
                rasterizerState,
                DepthStencilStateDescription.None,
                BlendStateDescription.Opaque)
                .GetHandle();
        }

        protected override void Destroy()
        {
            _spriteBatch.Dispose();

            base.Destroy();
        }

        internal override void BuildRenderList(RenderList renderList)
        {
            renderList.AddRenderItem(new RenderItem(
                this,
                _spriteBatch.Material,
                _pipelineStateHandle,
                (commandEncoder, effect, pipelineStateHandle, instanceData) =>
                {
                    _spriteBatch.Begin(commandEncoder, Scene.Camera.Viewport.Bounds());

                    _spriteBatch.Draw(
                        Texture,
                        new Rectangle(0, 0, Texture.Width, Texture.Height),
                        Scene.Camera.Viewport.Bounds(),
                        SelectedMipMapLevel);

                    _spriteBatch.End();
                }));
        }
    }
}
