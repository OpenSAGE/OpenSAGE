using System.Numerics;

namespace OpenSage.Graphics.Shaders
{
    public static class FixedFunction
    {
        public enum TextureMappingType 
        {
            Uv = 0,
            Environment = 1,
            LinearOffset = 2,
            Rotate = 3,
            SineLinearOffset = 4,
            StepLinearOffset = 5,
            Screen = 6,
            Scale = 7,
            Grid = 8,
            Random = 9,
        }

        public struct TextureMapping
        {
            public TextureMappingType MappingType;

            public float Speed;
            public float Fps;
            public int Log2Width;

            public Vector2 UVPerSec;
            public Vector2 UVScale;
            public Vector2 UVCenter;
            public Vector2 UVAmplitude;
            public Vector2 UVFrequency;
            public Vector2 UVPhase;

            public Vector2 UVStep;
            public float StepsPerSecond;

#pragma warning disable IDE1006, CS0169
            private readonly float _Padding;
#pragma warning restore IDE1006, CS0169
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

        public enum DiffuseLightingType 
        {
            Disable = 0,
            Modulate = 1,
            Add = 2
        }

        public enum SecondaryTextureBlend
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
            public Bool32 SpecularEnabled;
            public Bool32 TexturingEnabled;
            public SecondaryTextureBlend SecondaryTextureColorBlend;
            public SecondaryTextureBlend SecondaryTextureAlphaBlend;
            public Bool32 AlphaTest;

#pragma warning disable CS0169
            private readonly Vector2 _padding;
#pragma warning restore CS0169
        }

        public struct MaterialConstantsType
        {
#pragma warning disable CS0169
            private readonly Vector3 _padding;
#pragma warning restore CS0169

            public int NumTextureStages;

            public VertexMaterial Material;
            public ShadingConfiguration Shading;
        }
    }
}
