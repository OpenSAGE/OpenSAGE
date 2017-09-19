using System.Numerics;
using System.Runtime.InteropServices;

namespace OpenSage.Graphics.Effects
{
    [StructLayout(LayoutKind.Explicit, Size = 96)]
    public struct VertexMaterial
    {
        [FieldOffset(0)]
        public Vector3 Ambient;

        [FieldOffset(12)]
        public Vector3 Diffuse;

        [FieldOffset(24)]
        public Vector3 Specular;

        [FieldOffset(36)]
        public float Shininess;

        [FieldOffset(40)]
        public Vector3 Emissive;

        [FieldOffset(52)]
        public float Opacity;

        [FieldOffset(56)]
        public TextureMapping TextureMappingStage0;

        [FieldOffset(76)]
        public TextureMapping TextureMappingStage1;
    }

    [StructLayout(LayoutKind.Explicit, Size = 20)]
    public struct TextureMapping
    {
        [FieldOffset(0)]
        public TextureMappingType MappingType;

        [FieldOffset(4)]
        public Vector2 UVPerSec;

        [FieldOffset(12)]
        public Vector2 UVScale;
    }

    public enum TextureMappingType : uint
    {
        Uv = 0,
        Environment = 1,
        LinearOffset = 2
    }
}
