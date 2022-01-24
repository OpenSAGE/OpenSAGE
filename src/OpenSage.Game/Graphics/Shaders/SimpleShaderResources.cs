using System.Collections.Generic;
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
                CreateMaterialResourceBindings)
        {

        }

        private static IEnumerable<ResourceBinding> CreateMaterialResourceBindings()
        {
            yield return new ResourceBinding(
                0,
                new ResourceLayoutElementDescription("MaterialConstants", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                new ResourceType(
                    "MaterialConstants",
                    32,
                    new ResourceTypeMember("ColorEmissive", ResourceType.Vec4, 0),
                    new ResourceTypeMember("TexCoordTransform_0", ResourceType.Vec4, 16)));

            yield return new ResourceBinding(
                1,
                new ResourceLayoutElementDescription("Texture_0", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                ResourceType.Texture2D);

            yield return new ResourceBinding(
                2,
                new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment),
                ResourceType.Sampler);
        }
    }
}
