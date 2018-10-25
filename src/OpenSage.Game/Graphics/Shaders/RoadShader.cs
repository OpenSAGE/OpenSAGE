using System.Numerics;
using ShaderGen;
using static ShaderGen.ShaderBuiltins;
using static OpenSage.Graphics.Shaders.CloudHelpers;
using static OpenSage.Graphics.Shaders.CommonShaderHelpers;
using static OpenSage.Graphics.Shaders.LightingHelpers;

[assembly: ShaderSet("RoadShader", "OpenSage.Graphics.Shaders.RoadShader.VS", "OpenSage.Graphics.Shaders.RoadShader.PS")]

namespace OpenSage.Graphics.Shaders
{
    public class RoadShader
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

        public Texture2DResource Texture;
        public SamplerResource Sampler;

        [VertexShader]
        public PixelInput VS(VertexInput input)
        {
            PixelInput output;

            var worldPosition = input.Position;

            output.Position = Vector4.Transform(new Vector4(worldPosition, 1), GlobalConstantsVS.ViewProjection);
            output.WorldPosition = worldPosition;

            output.WorldNormal = input.Normal;

            output.UV = input.UV;

            output.CloudUV = GetCloudUV(
                output.WorldPosition,
                Global_LightingConstantsVS.CloudShadowMatrix,
                GlobalConstantsShared.TimeInSeconds);

            return output;
        }

        [FragmentShader]
        public Vector4 PS(PixelInput input)
        {
            Vector3 diffuseColor;
            Vector3 specularColor;
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
                out diffuseColor,
                out specularColor);

            var textureColor = Sample(Texture, Sampler, input.UV);

            var cloudColor = GetCloudColor(Global_CloudTexture, Sampler, input.CloudUV);

            return new Vector4(
                diffuseColor * textureColor.XYZ() * cloudColor,
                textureColor.W);
        }
    }
}
