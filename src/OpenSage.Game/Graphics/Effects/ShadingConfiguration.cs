using System.Runtime.InteropServices;

namespace OpenSage.Graphics.Effects
{
    [StructLayout(LayoutKind.Explicit, Size = 24)]
    public struct ShadingConfiguration
    {
        [FieldOffset(0)]
        public DiffuseLightingType DiffuseLightingType;

        [FieldOffset(4)]
        public bool SpecularEnabled;

        [FieldOffset(8)]
        public bool TexturingEnabled;

        [FieldOffset(12)]
        public SecondaryTextureBlend SecondaryTextureColorBlend;

        [FieldOffset(16)]
        public SecondaryTextureBlend SecondaryTextureAlphaBlend;

        [FieldOffset(20)]
        public bool AlphaTest;
    }

    public enum DiffuseLightingType : uint
    {
        Disable = 0,
        Modulate = 1,
        Add = 2
    }

    public enum SecondaryTextureBlend : uint
    {
        Disable = 0,
        Detail = 1,
        Scale = 2,
        InvScale = 3,
        DetailBlend = 4
    }
}
