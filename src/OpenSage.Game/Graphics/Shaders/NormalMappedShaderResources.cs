using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class NormalMappedShaderResources : ShaderMaterialShaderResources
    {
        public NormalMappedShaderResources(
            GraphicsDevice graphicsDevice,
            GlobalShaderResources globalShaderResources,
            MeshShaderResources meshShaderResources)
            : base(
                graphicsDevice,
                globalShaderResources,
                meshShaderResources,
                "NormalMapped",
                CreateMaterialResourceLayout)
        {

        }

        private static ResourceLayout CreateMaterialResourceLayout(GraphicsDevice graphicsDevice)
        {
            return graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("MaterialConstants", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("DiffuseTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("NormalMap", ResourceKind.TextureReadOnly, ShaderStages.Fragment)));
        }
    }
}
