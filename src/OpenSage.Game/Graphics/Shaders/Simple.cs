using System.Numerics;
using ShaderGen;
using static ShaderGen.ShaderBuiltins;
using static OpenSage.Graphics.Shaders.CloudHelpers;
using static OpenSage.Graphics.Shaders.CloudHelpers2;
using static OpenSage.Graphics.Shaders.CommonShaderHelpers;
using static OpenSage.Graphics.Shaders.LightingHelpers;
using static OpenSage.Graphics.Shaders.MeshShaderHelpers;

[assembly: ShaderSet("Simple", null, "OpenSage.Graphics.Shaders.Simple.PS")]

namespace OpenSage.Graphics.Shaders
{
    public class Simple
    {
        public struct VertexOutput
        {
            [SystemPositionSemantic] public Vector4 Position;

            [TextureCoordinateSemantic] public Vector3 WorldPosition;
            [TextureCoordinateSemantic] public Vector3 WorldNormal;
            [TextureCoordinateSemantic] public Vector2 UV0;
            [TextureCoordinateSemantic] public Vector2 CloudUV;
        }

        public struct PixelInput
        {
            [TextureCoordinateSemantic] public Vector3 WorldPosition;
            [TextureCoordinateSemantic] public Vector3 WorldNormal;
            [TextureCoordinateSemantic] public Vector2 UV0;
            [TextureCoordinateSemantic] public Vector2 CloudUV;
        }

        public GlobalConstantsShared GlobalConstantsShared;

        public GlobalConstantsVS GlobalConstantsVS;

        public Global_LightingConstantsVS Global_LightingConstantsVS;

        public Texture2DResource Global_CloudTexture;

        public MeshConstants MeshConstants;

        public RenderItemConstantsVS RenderItemConstantsVS;

        public StructuredBuffer<Matrix4x4> SkinningBuffer;

        public struct MaterialConstantsType
        {
            public Vector4 ColorEmissive;
            public Vector4 TexCoordTransform_0;
        }

        public MaterialConstantsType MaterialConstants;

        public Texture2DResource Texture_0;
        public SamplerResource Sampler;

        //[VertexShader]
        //public VertexOutput VS(VertexInput input)
        //{
        //    VertexOutput output;

        //    VSSkinnedInstanced(
        //        input,
        //        out output.Position,
        //        out output.WorldPosition,
        //        out output.WorldNormal,
        //        out output.CloudUV,
        //        SkinningBuffer,
        //        MeshConstants.SkinningEnabled,
        //        MeshConstants.NumBones,
        //        RenderItemConstantsVS.World,
        //        GlobalConstantsVS.ViewProjection,
        //        Global_LightingConstantsVS.CloudShadowMatrix);

        //    output.UV0 = input.UV0;

        //    return output;
        //}

        [FragmentShader]
        public Vector4 PS(PixelInput input)
        {
            var uv =
                (input.UV0 * MaterialConstants.TexCoordTransform_0.XY()) +
                (GlobalConstantsShared.TimeInSeconds * MaterialConstants.TexCoordTransform_0.ZW());

            var color = new Vector4(MaterialConstants.ColorEmissive.XYZ(), 1);

            color *= Sample(Texture_0, Sampler, uv);

            var cloudColor = GetCloudColor2(Global_CloudTexture, Sampler, input.CloudUV);
            //color = new Vector4(color.XYZ() * cloudColor, color.W);

            // TODO: Fog.

            return color;
        }
    }

    public static class CloudHelpers2
    {
        public static Vector2 GetCloudUV2(
            Vector3 worldPosition,
            Matrix4x4 cloudShadowMatrix,
            uint timeInSeconds)
        {
            // TODO: Wasteful to do a whole matrix-multiply here when we only need xy.
            var lightSpacePos = Vector4.Transform(worldPosition, cloudShadowMatrix).XY();

            var cloudTextureScale = new Vector2(1 / 660.0f, 1 / 660.0f); // TODO: Read this from Weather.ini
            var offset = Frac(timeInSeconds * new Vector2(-0.012f, -0.018f)); // TODO: Read this from Weather.ini

            return lightSpacePos * cloudTextureScale + offset;
        }

        public static Vector3 GetCloudColor2(
            Texture2DResource cloudTexture,
            SamplerResource samplerState,
            Vector2 cloudUV)
        {
            return Vector3.Zero;
            //return Sample(cloudTexture, samplerState, cloudUV).XYZ();
        }
    }
}
