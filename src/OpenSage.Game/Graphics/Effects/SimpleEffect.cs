using LLGfx;

namespace OpenSage.Graphics.Effects
{
    public sealed class SimpleEffect : Effect
    {
        public SimpleEffect(GraphicsDevice graphicsDevice) 
            : base(
                  graphicsDevice, 
                  "SimpleVS", 
                  "SimplePS",
                  MeshVertex.VertexDescriptor)
        {
            //_textureConstantBuffer = DynamicBuffer<TextureConstants>.Create(graphicsDevice, BufferBindFlags.ConstantBuffer);
        }

        protected override void OnApply()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnBegin()
        {
            throw new System.NotImplementedException();
        }
    }
}
