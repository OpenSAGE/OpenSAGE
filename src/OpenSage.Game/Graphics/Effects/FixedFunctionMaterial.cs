using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Content;
using Veldrid;

namespace OpenSage.Graphics.Effects
{
    public sealed class FixedFunctionMaterial : MeshMaterial
    {
        public FixedFunctionMaterial(ContentManager contentManager, Effect effect)
            : base(contentManager, effect)
        {
            
        }

        public void SetTexture0(Texture texture)
        {
            SetProperty("Texture0", texture);
        }

        public void SetTexture1(Texture texture)
        {
            SetProperty("Texture1", texture);
        }

        public void SetMaterialConstants(DeviceBuffer materialConstants)
        {
            SetProperty("MaterialConstants", materialConstants);
        }

        [StructLayout(LayoutKind.Explicit, Size = 240)]
        public struct MaterialConstants
        {
            [FieldOffset(0)]
            public uint NumTextureStages;

            [FieldOffset(16)]
            public VertexMaterial Material;

            [FieldOffset(208)]
            public ShadingConfiguration Shading;
        }

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
}
