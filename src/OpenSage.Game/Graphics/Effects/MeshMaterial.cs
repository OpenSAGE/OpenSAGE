using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Content;
using Veldrid;

namespace OpenSage.Graphics.Effects
{
    public abstract class MeshMaterial : EffectMaterial
    {
        protected MeshMaterial(ContentManager contentManager, Effect effect)
            : base(contentManager, effect)
        {
            SetProperty("Sampler", effect.GraphicsDevice.Aniso4xSampler);
        }

        public void SetSkinningBuffer(DeviceBuffer skinningBuffer)
        {
            SetProperty("SkinningBuffer", skinningBuffer);
        }

        public void SetMeshConstants(DeviceBuffer meshConstants)
        {
            SetProperty("MeshConstants", meshConstants);
        }

        public override LightingType LightingType => LightingType.Object;

        [StructLayout(LayoutKind.Sequential, Size = 32)]
        public struct MeshConstants
        {
            public Vector3 HouseColor;
            public bool HasHouseColor;
            public bool SkinningEnabled;
            public uint NumBones;
        }
    }
}
