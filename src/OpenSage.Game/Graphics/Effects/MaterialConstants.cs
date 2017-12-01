using System.Runtime.InteropServices;

namespace OpenSage.Graphics.Effects
{
    [StructLayout(LayoutKind.Explicit)]
    public struct MaterialConstants
    {
        [FieldOffset(0)]
        public uint NumTextureStages;

        [FieldOffset(16)]
        public VertexMaterial Material;

        [FieldOffset(208)]
        public ShadingConfiguration Shading;
    }
}
