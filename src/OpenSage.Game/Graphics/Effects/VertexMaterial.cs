using System.Numerics;
using System.Runtime.InteropServices;

namespace OpenSage.Graphics.Effects
{
    [StructLayout(LayoutKind.Explicit)]
    public struct VertexMaterial
    {
        [FieldOffset(0)]
        public Vector3 Ambient;

        [FieldOffset(16)]
        public Vector3 Diffuse;

        [FieldOffset(32)]
        public Vector3 Specular;

        [FieldOffset(44)]
        public float Shininess;

        [FieldOffset(48)]
        public Vector3 Emissive;

        [FieldOffset(60)]
        public float Opacity;

        [FieldOffset(64)]
        public TextureMapping TextureMappingStage0;

        [FieldOffset(128)]
        public TextureMapping TextureMappingStage1;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct TextureMapping
    {
        [FieldOffset(0)]
        public TextureMappingType MappingType;

        [FieldOffset(4)]
        public float Speed;

        [FieldOffset(8)]
        public float Fps;

        [FieldOffset(12)]
        public uint Log2Width;

        [FieldOffset(16)]
        public Vector2 UVPerSec;

        [FieldOffset(24)]
        public Vector2 UVScale;

        [FieldOffset(32)]
        public Vector2 UVCenter;

        [FieldOffset(40)]
        public Vector2 UVAmplitude;

        [FieldOffset(48)]
        public Vector2 UVFrequency;

        [FieldOffset(56)]
        public Vector2 UVPhase;
    }

    public enum TextureMappingType : uint
    {
        Uv = 0,
        Environment = 1,
        LinearOffset = 2,
        Rotate = 3,
        SineLinearOffset = 4,
        Screen = 5,
        Scale = 6,
        Grid = 7
    }
}
