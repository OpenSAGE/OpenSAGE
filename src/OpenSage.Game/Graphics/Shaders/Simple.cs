using System.Numerics;
using ShaderGen;
using static ShaderGen.ShaderBuiltins;
using static OpenSage.Graphics.Shaders.CloudHelpers;
using static OpenSage.Graphics.Shaders.CommonShaderHelpers;
using static OpenSage.Graphics.Shaders.LightingHelpers;
using static OpenSage.Graphics.Shaders.MeshShaderHelpers;

[assembly: ShaderSet("Simple", "OpenSage.Graphics.Shaders.Simple.VS", "OpenSage.Graphics.Shaders.Simple.PS")]

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

        [VertexShader]
        public VertexOutput VS(VertexInput input)
        {
            VertexOutput output;

            VSSkinnedInstanced(
                input,
                out output.Position,
                out output.WorldPosition,
                out output.WorldNormal,
                out output.CloudUV,
                SkinningBuffer,
                MeshConstants.SkinningEnabled,
                MeshConstants.NumBones,
                RenderItemConstantsVS.World,
                GlobalConstantsVS.ViewProjection,
                Global_LightingConstantsVS.CloudShadowMatrix,
                GlobalConstantsShared.TimeInSeconds);

            output.UV0 = input.UV0;

            return output;
        }

        [FragmentShader]
        public Vector4 PS(PixelInput input)
        {
            var uv =
                (input.UV0 * MaterialConstants.TexCoordTransform_0.XY()) +
                (GlobalConstantsShared.TimeInSeconds * MaterialConstants.TexCoordTransform_0.ZW());

            var color = new Vector4(MaterialConstants.ColorEmissive.XYZ(), 1);

            color *= Sample(Texture_0, Sampler, uv);

            var cloudColor = GetCloudColor(Global_CloudTexture, Sampler, input.CloudUV);
            color = new Vector4(color.XYZ() * cloudColor, color.W);

            // TODO: Fog.

            return color;
        }
    }
}
