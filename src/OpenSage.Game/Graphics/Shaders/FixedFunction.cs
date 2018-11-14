using System.Numerics;
using ShaderGen;
using static ShaderGen.ShaderBuiltins;
using static OpenSage.Graphics.Shaders.CloudHelpers;
using static OpenSage.Graphics.Shaders.CommonShaderHelpers;
using static OpenSage.Graphics.Shaders.LightingHelpers;
using static OpenSage.Graphics.Shaders.MeshShaderHelpers;

[assembly: ShaderSet("FixedFunction", "OpenSage.Graphics.Shaders.FixedFunction.VS", "OpenSage.Graphics.Shaders.FixedFunction.PS")]

namespace OpenSage.Graphics.Shaders
{
    public class FixedFunction
    {
        public struct VertexOutput
        {
            [SystemPositionSemantic] public Vector4 Position;

            [TextureCoordinateSemantic] public Vector3 WorldPosition;
            [TextureCoordinateSemantic] public Vector3 WorldNormal;
            [TextureCoordinateSemantic] public Vector2 UV0;
            [TextureCoordinateSemantic] public Vector2 UV1;
            [TextureCoordinateSemantic] public Vector2 CloudUV;
        }

        public struct PixelInput
        {
            [SystemPositionSemantic] public Vector4 ScreenPosition;

            [TextureCoordinateSemantic] public Vector3 WorldPosition;
            [TextureCoordinateSemantic] public Vector3 WorldNormal;
            [TextureCoordinateSemantic] public Vector2 UV0;
            [TextureCoordinateSemantic] public Vector2 UV1;
            [TextureCoordinateSemantic] public Vector2 CloudUV;
        }

        public GlobalConstantsShared GlobalConstantsShared;

        public GlobalConstantsVS GlobalConstantsVS;

        public GlobalConstantsPS GlobalConstantsPS;

        public Global_LightingConstantsVS Global_LightingConstantsVS;

        public Global_LightingConstantsPS Global_LightingConstantsPS;

        public Texture2DResource Global_CloudTexture;

        public MeshConstants MeshConstants;

        public RenderItemConstantsVS RenderItemConstantsVS;

        public StructuredBuffer<Matrix4x4> SkinningBuffer;

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
            public /* bool */ uint BlendEnabled;
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

        public MaterialConstantsType MaterialConstants;

        public Texture2DResource Texture0;
        public Texture2DResource Texture1;

        public SamplerResource Sampler;

        [VertexShader]
        public VertexOutput VS(VertexInput input)
        {
            VertexOutput output;

            if (MeshConstants.SkinningEnabled == 1u)
            {
                GetSkinnedVertexData(ref input, SkinningBuffer[input.BoneIndex]);
            }

            VSSkinnedInstanced(
                input,
                out output.Position,
                out output.WorldPosition,
                out output.WorldNormal,
                out output.CloudUV,
                RenderItemConstantsVS.World,
                GlobalConstantsVS.ViewProjection,
                Global_LightingConstantsVS.CloudShadowMatrix,
                GlobalConstantsShared.TimeInSeconds);

            output.UV0 = input.UV0;
            output.UV1 = input.UV1;

            return output;
        }

        private Vector4 SampleTexture(
            Vector3 worldNormal, Vector2 uv, Vector2 screenPosition,
            TextureMapping textureMapping,
            Texture2DResource diffuseTexture,
            Vector3 viewVector)
        {
            const float twoPi = 2 * 3.1415926535f;

            var t = GlobalConstantsShared.TimeInSeconds;

            switch (textureMapping.MappingType)
            {
                case TextureMappingType.Uv:
                {
                    uv = new Vector2(uv.X, 1 - uv.Y);
                    break;
                }

                case TextureMappingType.Environment:
                {
                    uv = (Vector3.Reflect(viewVector, worldNormal).XY() / 2.0f) + new Vector2(0.5f, 0.5f);
                    break;
                }

                case TextureMappingType.LinearOffset:
                {
                    var offset = textureMapping.UVPerSec * t;
                    uv = new Vector2(uv.X, 1 - uv.Y) + offset;
                    uv *= textureMapping.UVScale;
                    break;
                }

                case TextureMappingType.Rotate:
                {
                    var angle = textureMapping.Speed * t * twoPi;
                    var s = Sin(angle);
                    var c = Cos(angle);

                    uv -= textureMapping.UVCenter;

                    var rotatedPoint = new Vector2(
                        uv.X * c - uv.Y * s,
                        uv.X * s + uv.Y * c);

                    uv = rotatedPoint + textureMapping.UVCenter;

                    uv *= textureMapping.UVScale;

                    break;
                }

                case TextureMappingType.SineLinearOffset:
                {
                    uv.X += textureMapping.UVAmplitude.X * Sin(textureMapping.UVFrequency.X * t * twoPi - textureMapping.UVPhase.X * twoPi);
                    uv.Y += textureMapping.UVAmplitude.Y * Cos(textureMapping.UVFrequency.Y * t * twoPi - textureMapping.UVPhase.Y * twoPi);
                    break;
                }

                case TextureMappingType.Screen:
                {
                    uv = (screenPosition / GlobalConstantsPS.ViewportSize) * textureMapping.UVScale;
                    break;
                }

                case TextureMappingType.Scale:
                {
                    uv *= textureMapping.UVScale;
                    break;
                }

                case TextureMappingType.Grid:
                {
                    uv = new Vector2(uv.X, 1 - uv.Y);
                    // TODO: This should really use a uint overload of Pow.
                    var numFramesPerSide = Pow(2f, textureMapping.Log2Width);
                    var numFrames = numFramesPerSide * numFramesPerSide;
                    var currentFrame = Mod(t * textureMapping.Fps, numFrames);
                    var currentFrameU = Mod(currentFrame, numFramesPerSide);
                    var currentFrameV = currentFrame / numFramesPerSide;
                    uv.X += currentFrameU / numFramesPerSide;
                    uv.Y += currentFrameV / numFramesPerSide;
                    break;
                }
            }

            return Sample(diffuseTexture, Sampler, uv);
        }

        [FragmentShader]
        public Vector4 PS(PixelInput input)
        {
            DoLighting(
                Global_LightingConstantsPS,
                input.WorldPosition,
                input.WorldNormal,
                MaterialConstants.Material.Ambient,
                MaterialConstants.Material.Diffuse,
                MaterialConstants.Material.Specular,
                MaterialConstants.Material.Shininess,
                GlobalConstantsShared.CameraPosition,
                true,
                out var diffuseColor,
                out var specularColor);

            Vector4 diffuseTextureColor;
            if (MaterialConstants.Shading.TexturingEnabled == 1u)
            {
                var v = CalculateViewVector(GlobalConstantsShared.CameraPosition, input.WorldPosition);

                diffuseTextureColor = SampleTexture(
                    input.WorldNormal, input.UV0, input.ScreenPosition.XY(),
                    MaterialConstants.Material.TextureMappingStage0,
                    Texture0,
                    v);

                if (MaterialConstants.NumTextureStages > 1u)
                {
                    var secondaryTextureColor = SampleTexture(
                        input.WorldNormal, input.UV1, input.ScreenPosition.XY(),
                        MaterialConstants.Material.TextureMappingStage1,
                        Texture1,
                        v);

                    switch (MaterialConstants.Shading.SecondaryTextureColorBlend)
                    {
                        case SecondaryTextureBlend.Detail:
                            diffuseTextureColor = new Vector4(
                                secondaryTextureColor.XYZ(),
                                diffuseTextureColor.W);
                            break;

                        case SecondaryTextureBlend.Scale:
                            diffuseTextureColor = new Vector4(
                                diffuseTextureColor.XYZ() * secondaryTextureColor.XYZ(),
                                diffuseTextureColor.W);
                            break;

                        case SecondaryTextureBlend.InvScale:
                            diffuseTextureColor = new Vector4(
                                (Vector3.One - diffuseTextureColor.XYZ()) * secondaryTextureColor.XYZ(),
                                diffuseTextureColor.W);
                            break;

                        case SecondaryTextureBlend.DetailBlend:
                            // (otherAlpha)*local + (~otherAlpha)*other
                            diffuseTextureColor = new Vector4(
                                (secondaryTextureColor.X * diffuseTextureColor.XYZ()) + ((1 - secondaryTextureColor.X) * secondaryTextureColor.XYZ()),
                                diffuseTextureColor.W);
                            break;
                    }

                    switch (MaterialConstants.Shading.SecondaryTextureAlphaBlend)
                    {
                        case SecondaryTextureBlend.Detail:
                            diffuseTextureColor.W = secondaryTextureColor.W;
                            break;

                        case SecondaryTextureBlend.Scale:
                            diffuseTextureColor.W *= secondaryTextureColor.W;
                            break;

                        case SecondaryTextureBlend.InvScale:
                            diffuseTextureColor.W += (1 - diffuseTextureColor.W) * secondaryTextureColor.W;
                            break;
                    }
                }

                if (MaterialConstants.Shading.AlphaTest == 1u)
                {
                    if (FailsAlphaTest(diffuseTextureColor.W))
                    {
                        Discard();
                    }
                }
            }
            else
            {
                diffuseTextureColor = Vector4.One;
            }

            var totalObjectLighting = Saturate(diffuseColor + MaterialConstants.Material.Emissive);

            var objectColor = diffuseTextureColor.XYZ();

            switch (MaterialConstants.Shading.DiffuseLightingType)
            {
                case DiffuseLightingType.Modulate:
                    objectColor *= totalObjectLighting;
                    break;

                case DiffuseLightingType.Add:
                    objectColor += totalObjectLighting;
                    break;
            }

            if (MaterialConstants.Shading.SpecularEnabled == 1u)
            {
                objectColor += specularColor;
            }

            var cloudColor = GetCloudColor(
                Global_CloudTexture,
                Sampler,
                input.CloudUV);

            // Some textures have an entirely transparent alpha channel, when they're only ever used with blending disabled.
            // This means we would end up with A = 0 in the framebuffer, which is fine when rendering directly to the back buffer.
            // But otherwise it causes unwanted transparent pixels, such as in the viewer. So when blending is disabled,
            // we set A = 1 to avoid this problem.
            if (MaterialConstants.Shading.BlendEnabled == 0)
            {
                diffuseTextureColor.W = 1;
            }

            return new Vector4(
                objectColor * cloudColor,
                MaterialConstants.Material.Opacity * diffuseTextureColor.W);
        }
    }
}
