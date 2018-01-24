using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.LowLevel.Graphics3D;

namespace OpenSage.Graphics.Effects
{
    public sealed class SpriteMaterial : EffectMaterial
    {
        public SpriteMaterial(Effect effect)
            : base(effect)
        {
            SetSampler(effect.GraphicsDevice.SamplerPointClamp);

            PipelineState = new EffectPipelineState(
                RasterizerStateDescription.CullNoneSolid,
                DepthStencilStateDescription.None,
                BlendStateDescription.AlphaBlend);
        }

        public void SetMaterialConstantsVS(Buffer<MaterialConstantsVS> buffer)
        {
            SetProperty("MaterialConstantsVS", buffer);
        }

        public void SetSampler(SamplerState samplerState)
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
    }
}
