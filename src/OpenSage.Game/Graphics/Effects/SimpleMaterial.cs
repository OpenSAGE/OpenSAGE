using Veldrid;

namespace OpenSage.Graphics.Effects
{
    public sealed class SimpleMaterial
    {
        public const uint SlotSampler = 7;
        public const uint SlotSkinningBuffer = 4;
        public const uint SlotMeshConstants = 2;

        public const uint SlotMaterialConstants = 5;

        public static ResourceLayoutElementDescription[] ResourceLayoutDescriptions = new[]
        {
            new ResourceLayoutElementDescription("GlobalConstantsVS", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("GlobalConstantsPS", ResourceKind.UniformBuffer, ShaderStages.Vertex),

            new ResourceLayoutElementDescription("MeshConstants", ResourceKind.UniformBuffer, ShaderStages.Vertex),

            new ResourceLayoutElementDescription("RenderItemConstantsVS", ResourceKind.UniformBuffer, ShaderStages.Vertex),

            new ResourceLayoutElementDescription("SkinningBuffer", ResourceKind.StructuredBufferReadOnly, ShaderStages.Vertex),

            new ResourceLayoutElementDescription("MaterialConstants", ResourceKind.UniformBuffer, ShaderStages.Fragment),

            new ResourceLayoutElementDescription("Texture_0", ResourceKind.TextureReadOnly, ShaderStages.Fragment),

            new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)
        };
    }
}
