using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class SimpleShaderResources : ShaderMaterialShaderResources
    {
        public SimpleShaderResources(
            GraphicsDevice graphicsDevice,
            GlobalShaderResources globalShaderResources,
            MeshShaderResources meshShaderResources)
            : base(
                graphicsDevice,
                globalShaderResources,
                meshShaderResources,
                "Simple",
                CreateMaterialResourceLayout)
        {

        }

        private static ResourceLayout CreateMaterialResourceLayout(GraphicsDevice graphicsDevice)
        {
            return graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("MaterialConstants", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("Texture_0", ResourceKind.TextureReadOnly, ShaderStages.Fragment)));
        }
    }
}
