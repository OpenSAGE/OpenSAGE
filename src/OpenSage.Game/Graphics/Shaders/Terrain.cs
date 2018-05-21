using System.Numerics;
using ShaderGen;
using static OpenSage.Graphics.Shaders.CloudHelpers;
using static OpenSage.Graphics.Shaders.CommonShaderHelpers;
using static OpenSage.Graphics.Shaders.LightingHelpers;
using static ShaderGen.ShaderBuiltins;

[assembly: ShaderSet("Terrain", "OpenSage.Graphics.Shaders.Terrain.VS", "OpenSage.Graphics.Shaders.Terrain.PS")]

namespace OpenSage.Graphics.Shaders
{
    public class Terrain
    {
        public struct VertexInput
        {
            [PositionSemantic] public Vector3 Position;
            [NormalSemantic] public Vector3 Normal;
            [TextureCoordinateSemantic] public Vector2 UV;
        }

        public struct PixelInput
        {
            [SystemPositionSemantic] public Vector4 Position;

            [TextureCoordinateSemantic] public Vector3 WorldPosition;
            [TextureCoordinateSemantic] public Vector3 WorldNormal;
            [TextureCoordinateSemantic] public Vector2 UV;
            [TextureCoordinateSemantic] public Vector2 CloudUV;
        }

        public GlobalConstantsShared GlobalConstantsShared;

        public GlobalConstantsVS GlobalConstantsVS;

        public Global_LightingConstantsVS Global_LightingConstantsVS;

        public Global_LightingConstantsPS Global_LightingConstantsPS;

        public Texture2DResource Global_CloudTexture;

        public MeshShaderHelpers.RenderItemConstantsVS RenderItemConstantsVS;

        public struct TerrainMaterialConstantsType
        {
            public Vector2 MapBorderWidth;
            public Vector2 MapSize;
            public uint /*bool*/ IsMacroTextureStretched;
        }

        public TerrainMaterialConstantsType TerrainMaterialConstants;

        public Texture2DResource TileData; // TODO: This should really be Texture2DResource<uint4>

        public struct CliffInfo
        {
            public Vector2 BottomLeftUV;
            public Vector2 BottomRightUV;
            public Vector2 TopRightUV;
            public Vector2 TopLeftUV;
        }

        public StructuredBuffer<CliffInfo> CliffDetails;

        public struct TextureInfo
        {
            public uint TextureIndex;
            public uint CellSize;
        }

        public StructuredBuffer<TextureInfo> TextureDetails;

        public Texture2DArrayResource Textures;

        public Texture2DResource MacroTexture;

        public SamplerResource Sampler;

        [VertexShader]
        public PixelInput VS(VertexInput input)
        {
            PixelInput output;

            var worldPosition = (Vector4.Transform(new Vector4(input.Position, 1), RenderItemConstantsVS.World)).XYZ();

            output.Position = Vector4.Transform(new Vector4(worldPosition, 1), GlobalConstantsVS.ViewProjection);
            output.WorldPosition = worldPosition;

            output.WorldNormal = TransformNormal(input.Normal, RenderItemConstantsVS.World);

            output.UV = input.UV;

            output.CloudUV = GetCloudUV(
                output.WorldPosition,
                Global_LightingConstantsVS.CloudShadowMatrix,
                GlobalConstantsShared.TimeInSeconds);

            return output;
        }

        private Vector3 SampleTexture(
            uint textureIndex,
            Vector2 uv,
            Vector2 ddxUV,
            Vector2 ddyUV)
        {
            var textureInfo = TextureDetails[textureIndex];

            var scaledUV = uv / textureInfo.CellSize;

            // Can't use standard Sample because UV is scaled by texture CellSize,
            // and that doesn't work for divergent texture lookups.
            var diffuseTextureColor = SampleGrad(
                Textures,
                Sampler,
                scaledUV,
                textureInfo.TextureIndex,
                ddxUV / textureInfo.CellSize,
                ddyUV / textureInfo.CellSize);

            return diffuseTextureColor.XYZ();
        }

        private float CalculateDiagonalBlendFactor(Vector2 fracUV, bool twoSided)
        {
            return twoSided
                ? 1 - Saturate((fracUV.X + fracUV.Y) - 1)
                : Saturate(1 - (fracUV.X + fracUV.Y));
        }

        private float CalculateBlendFactor(
            uint blendDirection,
            uint blendFlags,
            Vector2 fracUV)
        {
            var flipped = (blendFlags & 1u) == 1u;
            var twoSided = (blendFlags & 2u) == 2u;

            if (flipped)
            {
                switch (blendDirection)
                {
                    case 1u: // BLEND_DIRECTION_TOWARDS_RIGHT:
                        fracUV.X = 1 - fracUV.X;
                        break;

                    case 2u: // BLEND_DIRECTION_TOWARDS_TOP:
                    case 4u: // BLEND_DIRECTION_TOWARDS_TOP_RIGHT:
                    case 8u: // BLEND_DIRECTION_TOWARDS_TOP_LEFT:
                        fracUV.Y = 1 - fracUV.Y;
                        break;
                }
            }

            float blendFactor = 0;

            switch (blendDirection)
            {
                case 1u: // BLEND_DIRECTION_TOWARDS_RIGHT:
                    blendFactor = fracUV.X;
                    break;

                case 2u: // BLEND_DIRECTION_TOWARDS_TOP:
                    blendFactor = fracUV.Y;
                    break;

                case 4u: // BLEND_DIRECTION_TOWARDS_TOP_RIGHT:
                    fracUV = Vector2.One - fracUV;
                    blendFactor = CalculateDiagonalBlendFactor(fracUV, twoSided);
                    break;

                case 8u: // BLEND_DIRECTION_TOWARDS_TOP_LEFT:
                    fracUV.Y = 1 - fracUV.Y;
                    blendFactor = CalculateDiagonalBlendFactor(fracUV, twoSided);
                    break;
            }

            return blendFactor;
        }

        private Vector3 SampleBlendedTextures(Vector2 uv)
        {
            var tileDatum = Load(TileData, Sampler, uv, 0);

            var fracUV = Frac(uv);

            var cliffTextureIndex = (uint) tileDatum.Z;
            if (cliffTextureIndex != 0u)
            {
                var cliffInfo = CliffDetails[(int)cliffTextureIndex - 1];

                var uvXBottom = Lerp(cliffInfo.BottomLeftUV, cliffInfo.BottomRightUV, fracUV.X);
                var uvXTop = Lerp(cliffInfo.TopLeftUV, cliffInfo.TopRightUV, fracUV.X);

                uv = Lerp(uvXBottom, uvXTop, fracUV.Y);
            }

            var ddxUV = Ddx(uv);
            var ddyUV = Ddy(uv);

            var packedTextureIndices = (uint) tileDatum.X;
            var textureIndex0 = packedTextureIndices & 0xFFu;
            var textureIndex1 = (packedTextureIndices >> 8) & 0xFFu;
            var textureIndex2 = (packedTextureIndices >> 16) & 0xFFu;

            var packedBlendInfo = (uint) tileDatum.Y;
            var blendDirection1 = packedBlendInfo & 0xFFu;
            var blendFlags1 = (packedBlendInfo >> 8) & 0xFFu;
            var blendDirection2 = (packedBlendInfo >> 16) & 0xFFu;
            var blendFlags2 = (packedBlendInfo >> 24) & 0xFFu;

            var textureColor0 = SampleTexture(textureIndex0, uv, ddxUV, ddyUV);
            var textureColor1 = SampleTexture(textureIndex1, uv, ddxUV, ddyUV);
            var textureColor2 = SampleTexture(textureIndex2, uv, ddxUV, ddyUV);

            var blendFactor1 = CalculateBlendFactor(blendDirection1, blendFlags1, fracUV);
            var blendFactor2 = CalculateBlendFactor(blendDirection2, blendFlags2, fracUV);

            return
                Lerp(
                    Lerp(
                        textureColor0,
                        textureColor1,
                        blendFactor1),
                    textureColor2,
                    blendFactor2);
        }

        private Vector2 GetMacroTextureUV(Vector3 worldPosition)
        {
            if (TerrainMaterialConstants.IsMacroTextureStretched == 1u)
            {
                return (worldPosition.XY() + TerrainMaterialConstants.MapBorderWidth) / new Vector2(TerrainMaterialConstants.MapSize.X, -TerrainMaterialConstants.MapSize.Y);
            }
            else
            {
                var macroTextureScale = 1 / 660.0f;
                return worldPosition.XY() * new Vector2(macroTextureScale, -macroTextureScale);
            }
        }

        [FragmentShader]
        public Vector4 PS(PixelInput input)
        {
            DoLighting(
                Global_LightingConstantsPS,
                input.WorldPosition,
                input.WorldNormal,
                Vector3.One,
                Vector3.One,
                Vector3.Zero,
                0,
                GlobalConstantsShared.CameraPosition,
                false,
                out var diffuseColor,
                out var specularColor);

            var textureColor = SampleBlendedTextures(input.UV);

            var cloudColor = GetCloudColor(Global_CloudTexture, Sampler, input.CloudUV);

            var macroTextureUV = GetMacroTextureUV(input.WorldPosition);
            var macroTextureColor = Sample(MacroTexture, Sampler, macroTextureUV).XYZ();

            return new Vector4(
                diffuseColor * textureColor * cloudColor * macroTextureColor,
                1);
        }
    }
}
