using System.Collections.Generic;
using OpenSage.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    public sealed class NormalMappedShaderResources : ShaderMaterialShaderResources
    {
        public NormalMappedShaderResources(
            ShaderSetStore store)
            : base(store, "NormalMapped", CreateMaterialResourceBindings)
        {

        }

        private static IEnumerable<ResourceBinding> CreateMaterialResourceBindings()
        {
            yield return new ResourceBinding(
                0,
                new ResourceLayoutElementDescription("MaterialConstants", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                new ResourceType(
                    "MaterialConstants",
                    64,
                    new ResourceTypeMember("BumpScale", ResourceType.Float, 0),
                    new ResourceTypeMember("SpecularExponent", ResourceType.Float, 4),
                    new ResourceTypeMember("AlphaTestEnable", ResourceType.Int, 8),
                    new ResourceTypeMember("_Padding", ResourceType.Float, 12),
                    new ResourceTypeMember("AmbientColor", ResourceType.Vec4, 16),
                    new ResourceTypeMember("DiffuseColor", ResourceType.Vec4, 32),
                    new ResourceTypeMember("SpecularColor", ResourceType.Vec4, 48)));

            yield return new ResourceBinding(
                1,
                new ResourceLayoutElementDescription("DiffuseTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                ResourceType.Texture2D);

            yield return new ResourceBinding(
                2,
                new ResourceLayoutElementDescription("NormalMap", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                ResourceType.Texture2D);

            yield return new ResourceBinding(
                3,
                new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment),
                ResourceType.Sampler);
        }
    }
}
