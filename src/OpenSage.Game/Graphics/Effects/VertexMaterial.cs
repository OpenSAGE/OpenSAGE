using System.Numerics;
using System.Runtime.InteropServices;

namespace OpenSage.Graphics.Effects
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexMaterial
    {
        public Vector3 Ambient;
        public Vector3 Diffuse;
        public Vector3 Specular;
        public float Shininess;
        public Vector3 Emissive;
        public float Opacity;

        public TextureMapping TextureMappingStage0;
        public TextureMapping TextureMappingStage1;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TextureMapping
    {
        public TextureMappingType MappingType;

        public Vector2 UVPerSec;
        public Vector2 UVScale;
        public Vector2 UVCenter;
        public Vector2 UVAmplitude;
        public Vector2 UVFrequency;
        public Vector2 UVPhase;
        public float Speed;
        public float Fps;
        public uint Log2Width;
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
