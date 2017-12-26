using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Effects
{
    public abstract class MeshMaterial : EffectMaterial
    {
        protected MeshMaterial(Effect effect)
            : base(effect)
        {
            SetProperty("Sampler", effect.GraphicsDevice.SamplerAnisotropicWrap);
        }

        public void SetSkinningBuffer(Buffer<Matrix4x3> skinningBuffer)
        {
            SetProperty("SkinningBuffer", skinningBuffer);
        }

        public void SetMeshConstants(Buffer<MeshConstants> meshConstants)
        {
            SetProperty("MeshConstants", meshConstants);
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
