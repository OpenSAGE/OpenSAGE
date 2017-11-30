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
        }

        protected override void OnApply()
        {
            SetValue("Sampler", GraphicsDevice.SamplerAnisotropicWrap);
        }

        protected override void OnBegin()
        {
            
        }
    }
}
