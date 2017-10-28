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
        public float Speed;
    }

    public enum TextureMappingType : uint
    {
        Uv = 0,
        Environment = 1,
        LinearOffset = 2,
        Rotate = 3
    }
}
