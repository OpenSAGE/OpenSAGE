using System.Numerics;
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
        public struct MeshConstants
        {
            public Matrix4x4 World;
            public bool SkinningEnabled;
            public uint NumBones;
        }
    }
}
