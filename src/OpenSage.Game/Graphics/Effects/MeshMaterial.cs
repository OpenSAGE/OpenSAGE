using System.Runtime.InteropServices;
using Veldrid;

namespace OpenSage.Graphics.Effects
{
    public abstract class MeshMaterial : EffectMaterial
    {
        protected abstract uint SlotSampler { get; }
        protected abstract uint SlotSkinningBuffer { get; }
        protected abstract uint SlotMeshConstants { get; }

        protected MeshMaterial(Effect effect)
            : base(effect)
        {
            SetProperty(SlotSampler, effect.GraphicsDevice.Aniso4xSampler);
        }

        public void SetSkinningBuffer(DeviceBuffer skinningBuffer)
        {
            SetProperty(SlotSkinningBuffer, skinningBuffer);
        }

        public void SetMeshConstants(DeviceBuffer meshConstants)
        {
            SetProperty(SlotMeshConstants, meshConstants);
        }

        [StructLayout(LayoutKind.Sequential, Size = 16)]
        public struct MeshConstants
        {
            public bool SkinningEnabled;
            public uint NumBones;
        }
    }
}
