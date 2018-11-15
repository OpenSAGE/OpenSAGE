using System.Numerics;

namespace OpenSage.Graphics.Shaders
{
    public static class FixedFunction
    {
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

        public struct TextureMapping
        {
            public TextureMappingType MappingType;

            public float Speed;
            public float Fps;
            public uint Log2Width;

            public Vector2 UVPerSec;
            public Vector2 UVScale;
            public Vector2 UVCenter;
            public Vector2 UVAmplitude;
            public Vector2 UVFrequency;
            public Vector2 UVPhase;
        }

        public struct VertexMaterial
        {
            public Vector3 Ambient;

#pragma warning disable CS0169
            private readonly float _padding1;
#pragma warning restore CS0169

            public Vector3 Diffuse;

#pragma warning disable CS0169
            private readonly float _padding2;
#pragma warning restore CS0169

            public Vector3 Specular;
            public float Shininess;
            public Vector3 Emissive;
            public float Opacity;

            public TextureMapping TextureMappingStage0;
            public TextureMapping TextureMappingStage1;
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

        public struct ShadingConfiguration
        {
            public DiffuseLightingType DiffuseLightingType;
            public /*bool*/ uint SpecularEnabled;
            public /*bool*/ uint TexturingEnabled;
            public SecondaryTextureBlend SecondaryTextureColorBlend;
            public SecondaryTextureBlend SecondaryTextureAlphaBlend;
            public /*bool*/ uint AlphaTest;

#pragma warning disable CS0169
            private readonly float _padding;
#pragma warning restore CS0169
        }

        public struct MaterialConstantsType
        {
#pragma warning disable CS0169
            private readonly Vector3 _padding;
#pragma warning restore CS0169

            public uint NumTextureStages;

            public VertexMaterial Material;
            public ShadingConfiguration Shading;
        }
    }
}
