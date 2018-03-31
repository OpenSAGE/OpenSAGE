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

        public struct TextureMapping
        {
            public uint MappingType;

            public float Speed;
            public float FPS;
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
            public Vector3 Diffuse;
            public Vector3 Specular;
            public float Shininess;
            public Vector3 Emissive;
            public float Opacity;

            public TextureMapping TextureMappingStage0;
            public TextureMapping TextureMappingStage1;
        }

        public struct ShadingConfiguration
        {
            public uint DiffuseLightingType;
            public /*bool*/ uint SpecularEnabled;
            public /*bool*/ uint TexturingEnabled;
            public uint SecondaryTextureColorBlend;
            public uint SecondaryTextureAlphaBlend;
            public /*bool*/ uint AlphaTest;
        }

        public struct MaterialConstantsType
        {
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

            if (MeshConstants.SkinningEnabled == 1)
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
                case 0: // TEXTURE_MAPPING_UV
                    uv = new Vector2(uv.X, 1 - uv.Y);
                    break;

                case 1: // TEXTURE_MAPPING_ENVIRONMENT
                    uv = (Vector3.Reflect(viewVector, worldNormal).XY() / 2.0f) + new Vector2(0.5f, 0.5f);
                    break;

                case 2: // TEXTURE_MAPPING_LINEAR_OFFSET
                    var offset = textureMapping.UVPerSec * t;
                    uv = new Vector2(uv.X, 1 - uv.Y) + offset;
                    uv *= textureMapping.UVScale;
                    break;

                case 3: // TEXTURE_MAPPING_ROTATE
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

                case 4: // TEXTURE_MAPPING_SINE_LINEAR_OFFSET
                    uv.X += textureMapping.UVAmplitude.X * Sin(textureMapping.UVFrequency.X * t * twoPi - textureMapping.UVPhase.X * twoPi);
                    uv.Y += textureMapping.UVAmplitude.Y * Cos(textureMapping.UVFrequency.Y * t * twoPi - textureMapping.UVPhase.Y * twoPi);
                    break;

                case 5: // TEXTURE_MAPPING_SCREEN
                    uv = (screenPosition / GlobalConstantsPS.ViewportSize) * textureMapping.UVScale;
                    break;

                case 6: // TEXTURE_MAPPING_SCALE
                    uv *= textureMapping.UVScale;
                    break;

                case 7: // TEXTURE_MAPPING_GRID
                    uv = new Vector2(uv.X, 1 - uv.Y);
                    // TODO: This should really use a uint overload of Pow.
                    var numFramesPerSide = Pow(2, textureMapping.Log2Width);
                    var numFrames = numFramesPerSide * numFramesPerSide;
                    var currentFrame = Mod(t * textureMapping.FPS, numFrames);
                    var currentFrameU = Mod(currentFrame, numFramesPerSide);
                    var currentFrameV = currentFrame / numFramesPerSide;
                    uv.X += currentFrameU / numFramesPerSide;
                    uv.Y += currentFrameV / numFramesPerSide;
                    break;
            }

            return Sample(diffuseTexture, Sampler, uv);
        }

        [FragmentShader]
        public Vector4 PS(PixelInput input)
        {
            //LightingParameters lightingParams;
            //lightingParams.WorldPosition = input.WorldPosition;
            //lightingParams.WorldNormal = input.WorldNormal;
            //lightingParams.MaterialAmbient = MaterialConstants.Material.Ambient;
            //lightingParams.MaterialDiffuse = MaterialConstants.Material.Diffuse;
            //lightingParams.MaterialSpecular = MaterialConstants.Material.Specular;
            //lightingParams.MaterialShininess = MaterialConstants.Material.Shininess;

            Vector3 diffuseColor;
            Vector3 specularColor;
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
                out diffuseColor,
                out specularColor);

            Vector4 diffuseTextureColor;
            if (MaterialConstants.Shading.TexturingEnabled == 1)
            {
                var v = CalculateViewVector(GlobalConstantsShared.CameraPosition, input.WorldPosition);

                diffuseTextureColor = SampleTexture(
                    input.WorldNormal, input.UV0, input.ScreenPosition.XY(),
                    MaterialConstants.Material.TextureMappingStage0,
                    Texture0,
                    v);

                if (MaterialConstants.NumTextureStages > 1)
                {
                    var secondaryTextureColor = SampleTexture(
                        input.WorldNormal, input.UV1, input.ScreenPosition.XY(),
                        MaterialConstants.Material.TextureMappingStage1,
                        Texture1,
                        v);

                    switch (MaterialConstants.Shading.SecondaryTextureColorBlend)
                    {
                        case 1: // SECONDARY_TEXTURE_BLEND_DETAIL
                            diffuseTextureColor = new Vector4(
                                secondaryTextureColor.XYZ(),
                                diffuseTextureColor.W);
                            break;

                        case 2: // SECONDARY_TEXTURE_BLEND_SCALE
                            diffuseTextureColor = new Vector4(
                                diffuseTextureColor.XYZ() * secondaryTextureColor.XYZ(),
                                diffuseTextureColor.W);
                            break;

                        case 3: // SECONDARY_TEXTURE_BLEND_INV_SCALE
                            diffuseTextureColor = new Vector4(
                                (Vector3.One - diffuseTextureColor.XYZ()) * secondaryTextureColor.XYZ(),
                                diffuseTextureColor.W);
                            break;

                        case 4: // SECONDARY_TEXTURE_BLEND_DETAIL_BLEND
                            // (otherAlpha)*local + (~otherAlpha)*other
                            diffuseTextureColor = new Vector4(
                                (secondaryTextureColor.X * diffuseTextureColor.XYZ()) + ((1 - secondaryTextureColor.X) * secondaryTextureColor.XYZ()),
                                diffuseTextureColor.W);
                            break;
                    }

                    switch (MaterialConstants.Shading.SecondaryTextureAlphaBlend)
                    {
                        case 1: // SECONDARY_TEXTURE_BLEND_DETAIL
                            diffuseTextureColor.W = secondaryTextureColor.W;
                            break;

                        case 2: // SECONDARY_TEXTURE_BLEND_SCALE:
                            diffuseTextureColor.W *= secondaryTextureColor.W;
                            break;

                        case 3: // SECONDARY_TEXTURE_BLEND_INV_SCALE
                            diffuseTextureColor.W += (1 - diffuseTextureColor.W) * secondaryTextureColor.W;
                            break;
                    }
                }

                if (MaterialConstants.Shading.AlphaTest == 1)
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
                case 1: // DIFFUSE_LIGHTING_MODULATE
                    objectColor *= totalObjectLighting;
                    break;

                case 2: // DIFFUSE_LIGHTING_ADD
                    objectColor += totalObjectLighting;
                    break;
            }

            if (MaterialConstants.Shading.SpecularEnabled == 1)
            {
                objectColor += specularColor;
            }

            var cloudColor = GetCloudColor(
                Global_CloudTexture,
                Sampler,
                input.CloudUV);

            return new Vector4(
                objectColor * cloudColor,
                MaterialConstants.Material.Opacity * diffuseTextureColor.W);
        }
    }
}
