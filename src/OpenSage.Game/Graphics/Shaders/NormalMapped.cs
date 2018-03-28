using System.Numerics;
using ShaderGen;
using static ShaderGen.ShaderBuiltins;
using static OpenSage.Graphics.Shaders.CloudHelpers;
using static OpenSage.Graphics.Shaders.CommonShaderHelpers;
using static OpenSage.Graphics.Shaders.LightingHelpers;
using static OpenSage.Graphics.Shaders.MeshShaderHelpers;

[assembly: ShaderSet("NormalMapped", "OpenSage.Graphics.Shaders.NormalMapped.VS", "OpenSage.Graphics.Shaders.NormalMapped.PS")]

namespace OpenSage.Graphics.Shaders
{
    public class NormalMapped
    {
        public struct VertexOutput
        {
            [SystemPositionSemantic] public Vector4 Position;

            [TextureCoordinateSemantic] public Vector3 WorldPosition;
            [TextureCoordinateSemantic] public Vector3 WorldNormal;
            [TextureCoordinateSemantic] public Vector2 UV0;
            [TextureCoordinateSemantic] public Vector2 CloudUV;

            [TextureCoordinateSemantic] public Vector3 WorldTangent;
            [TextureCoordinateSemantic] public Vector3 WorldBinormal;
        }

        public struct PixelInput
        {
            [TextureCoordinateSemantic] public Vector3 WorldPosition;
            [TextureCoordinateSemantic] public Vector3 WorldNormal;
            [TextureCoordinateSemantic] public Vector2 UV0;
            [TextureCoordinateSemantic] public Vector2 CloudUV;
            [TextureCoordinateSemantic] public Vector3 WorldTangent;
            [TextureCoordinateSemantic] public Vector3 WorldBinormal;
        }

        public GlobalConstantsShared GlobalConstantsShared;

        public GlobalConstantsVS GlobalConstantsVS;

        public Global_LightingConstantsVS Global_LightingConstantsVS;

        public Global_LightingConstantsPS Global_LightingConstantsPS;

        public Texture2DResource Global_CloudTexture;

        public MeshConstants MeshConstants;

        public RenderItemConstantsVS RenderItemConstantsVS;

        public StructuredBuffer<Matrix4x4> SkinningBuffer;

        public struct MaterialConstantsType
        {
            public float BumpScale;
            public float SpecularExponent;
            public uint /*bool*/ AlphaTestEnable;
            public Vector4 AmbientColor;
            public Vector4 DiffuseColor;
            public Vector4 SpecularColor;
        }

        public MaterialConstantsType MaterialConstants;

        public Texture2DResource DiffuseTexture;
        public Texture2DResource NormalMap;

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

            output.WorldTangent = TransformNormal(input.Tangent, RenderItemConstantsVS.World);
            output.WorldBinormal = TransformNormal(input.Binormal, RenderItemConstantsVS.World);

            return output;
        }

        [FragmentShader]
        public Vector4 PS(PixelInput input)
        {
            var uv = input.UV0;
            uv.Y = 1 - uv.Y;

            var tangentToWorldSpace = new Matrix4x4(
                input.WorldTangent.X, input.WorldTangent.Y, input.WorldTangent.Z, 0,
                input.WorldBinormal.X, input.WorldBinormal.Y, input.WorldBinormal.Z, 0,
                input.WorldNormal.X, input.WorldNormal.Y, input.WorldNormal.Z, 0,
                0, 0, 0, 1);

            var tangentSpaceNormal = (Sample(NormalMap, Sampler, uv).XYZ() * 2) - Vector3.One;
            tangentSpaceNormal = new Vector3(tangentSpaceNormal.XY() * MaterialConstants.BumpScale, tangentSpaceNormal.Z);
            tangentSpaceNormal = Vector3.Normalize(tangentSpaceNormal);

            var worldSpaceNormal = TransformNormal(tangentSpaceNormal, tangentToWorldSpace);

            Vector3 diffuseColor;
            Vector3 specularColor;
            DoLighting(
                Global_LightingConstantsPS,
                input.WorldPosition,
                worldSpaceNormal,
                MaterialConstants.AmbientColor.XYZ(),
                MaterialConstants.DiffuseColor.XYZ(),
                MaterialConstants.SpecularColor.XYZ(),
                MaterialConstants.SpecularExponent,
                GlobalConstantsShared.CameraPosition,
                false, //TODO: true,
                out diffuseColor,
                out specularColor);

            var diffuseTextureColor = Sample(DiffuseTexture, Sampler, uv);

            if (MaterialConstants.AlphaTestEnable == 1)
            {
                if (FailsAlphaTest(diffuseTextureColor.W))
                {
                    Discard();
                }
            }

            var objectColor = diffuseTextureColor.XYZ() * diffuseColor;

            objectColor += specularColor;

            var cloudColor = GetCloudColor(Global_CloudTexture, Sampler, input.CloudUV);
            objectColor *= cloudColor;

            return new Vector4(
                objectColor,
                MaterialConstants.DiffuseColor.W * diffuseTextureColor.W);
        }
    }
}
