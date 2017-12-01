using System.Runtime.InteropServices;

namespace OpenSage.Graphics.Effects
{
    public abstract class MeshMaterial : EffectMaterial
    {
        protected MeshMaterial(Effect effect)
            : base(effect)
        {
            SetProperty("Sampler", effect.GraphicsDevice.SamplerAnisotropicWrap);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SkinningConstants
        {
            public bool SkinningEnabled;
            public uint NumBones;
        }
    }
}
