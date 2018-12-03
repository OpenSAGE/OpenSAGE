using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Content;
using Veldrid;

namespace OpenSage.Graphics.Effects
{
    public sealed class SpriteMaterial : EffectMaterial
    {
        public SpriteMaterial(ContentManager contentManager, Effect effect, in BlendStateDescription blendStateDescription, in OutputDescription outputDescription)
            : base(contentManager, effect)
        {
            SetSampler(contentManager.PointClampSampler);

            PipelineState = new EffectPipelineState(
                RasterizerStateDescriptionUtility.CullNoneSolid,
                DepthStencilStateDescription.Disabled,
                blendStateDescription,
                outputDescription);
        }

        public void SetMaterialConstantsVS(DeviceBuffer value)
        {
            SetProperty("Projection", value);
        }

        public void SetSpriteConstantsPS(DeviceBuffer value)
        {
            SetProperty("SpriteConstants", value);
        }

        public void SetSampler(Sampler samplerState)
        {
            SetProperty("Sampler", samplerState);
        }

        public void SetTexture(Texture texture)
        {
            SetProperty("Texture", texture);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MaterialConstantsVS
        {
            public Matrix4x4 Projection;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SpriteConstantsPS
        {
            private readonly Vector3 _Padding;
            public uint IgnoreAlpha;
        }
    }
}
