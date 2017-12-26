using System.Runtime.InteropServices;
using OpenSage.LowLevel.Graphics3D;

namespace OpenSage.Graphics.Effects
{
    public sealed class SpriteMaterial : EffectMaterial
    {
        public SpriteMaterial(Effect effect)
            : base(effect)
        {
            SetProperty("Sampler", effect.GraphicsDevice.SamplerPointClamp);

            // TODO: Clean this up.
            var rasterizerState = RasterizerStateDescription.CullBackSolid;
            rasterizerState.IsFrontCounterClockwise = false;

            PipelineState = new EffectPipelineState(
                rasterizerState,
                DepthStencilStateDescription.None,
                BlendStateDescription.AlphaBlend);
        }

        public void SetMaterialConstants(Buffer<MaterialConstants> buffer)
        {
            SetProperty("MaterialConstants", buffer);
        }

        public void SetTexture(Texture texture)
        {
            SetProperty("Texture", texture);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MaterialConstants
        {
            public float Opacity;
        }
    }
}
