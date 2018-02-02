using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Content;
using Veldrid;

namespace OpenSage.Graphics.Effects
{
    public sealed class SpriteMaterial : EffectMaterial
    {
        public SpriteMaterial(ContentManager contentManager, Effect effect)
            : base(effect)
        {
            SetSampler(contentManager.PointClampSampler);

            PipelineState = new EffectPipelineState(
                RasterizerStateDescriptionUtility.CullNoneSolid,
                DepthStencilStateDescription.Disabled,
                BlendStateDescription.SingleAlphaBlend);
        }

        public void SetMaterialConstantsVS(DeviceBuffer value)
        {
            SetProperty(0, value);
        }

        public void SetSampler(Sampler samplerState)
        {
            SetProperty(1, samplerState);
        }

        public void SetTexture(Texture texture)
        {
            SetProperty(2, texture);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MaterialConstantsVS
        {
            public Matrix4x4 Projection;
        }

        public static ResourceLayoutElementDescription[] ResourceLayoutDescriptions = new[]
        {
            new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment)
        };
    }
}
