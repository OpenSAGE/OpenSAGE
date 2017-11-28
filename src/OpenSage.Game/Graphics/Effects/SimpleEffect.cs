using LLGfx;
using LLGfx.Effects;

namespace OpenSage.Graphics.Effects
{
    public sealed class SimpleEffect : Effect
    {
        public SimpleEffect(GraphicsDevice graphicsDevice) 
            : base(
                  graphicsDevice, 
                  "SimpleVS", 
                  "SimplePS",
                  MeshVertex.VertexDescriptor,
                  CreatePipelineLayoutDescription())
        {
            //_textureConstantBuffer = DynamicBuffer<TextureConstants>.Create(graphicsDevice, BufferBindFlags.ConstantBuffer);
        }

        private static PipelineLayoutDescription CreatePipelineLayoutDescription()
        {
            return new PipelineLayoutDescription
            {
                Entries = new[]
                {
                    // TextureCB
                    PipelineLayoutEntry.CreateResource(
                        ShaderStageVisibility.Pixel,
                        ResourceType.ConstantBuffer,
                        0),

                    // Texture
                    PipelineLayoutEntry.CreateResourceView(
                        ShaderStageVisibility.Pixel,
                        ResourceType.Texture,
                        0, 1)
                },

                StaticSamplerStates = new[]
                {
                    new StaticSamplerDescription(
                        ShaderStageVisibility.Pixel,
                        0,
                        SamplerStateDescription.Default)
                }
            };
        }

        protected override void OnApply(CommandEncoder commandEncoder)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnBegin()
        {
            throw new System.NotImplementedException();
        }
    }
}
