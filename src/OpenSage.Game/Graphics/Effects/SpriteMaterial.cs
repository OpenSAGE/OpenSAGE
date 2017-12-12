using System.Runtime.InteropServices;
using LL.Graphics3D;

namespace OpenSage.Graphics.Effects
{
    public sealed class SpriteMaterial : EffectMaterial
    {
        public SpriteMaterial(Effect effect)
            : base(effect)
        {
            SetProperty("Sampler", effect.GraphicsDevice.SamplerLinearClamp);
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
            public ColorRgbaF TintColor;
        }
    }
}
