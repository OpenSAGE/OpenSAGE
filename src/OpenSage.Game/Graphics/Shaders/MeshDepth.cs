using System.Numerics;
using ShaderGen;
using static OpenSage.Graphics.Shaders.CommonShaderHelpers;
using static OpenSage.Graphics.Shaders.MeshShaderHelpers;

[assembly: ShaderSet("MeshDepth", "OpenSage.Graphics.Shaders.MeshDepth.VS", "OpenSage.Graphics.Shaders.MeshDepth.PS")]

namespace OpenSage.Graphics.Shaders
{
    public class MeshDepth
    {
        public struct VertexOutput
        {
            [SystemPositionSemantic] public Vector4 Position;
        }

        public GlobalConstantsVS GlobalConstantsVS;

        public MeshConstants MeshConstants;

        public RenderItemConstantsVS RenderItemConstantsVS;

        public StructuredBuffer<Matrix4x4> SkinningBuffer;

        [VertexShader]
        public VertexOutput VS(VertexInput input)
        {
            VertexOutput output;

            if (MeshConstants.SkinningEnabled == 1)
            {
                GetSkinnedVertexData(ref input, SkinningBuffer[input.BoneIndex]);
            }

            VSSkinnedInstancedPositionOnly(
                input,
                out output.Position,
                out _,
                RenderItemConstantsVS.World,
                GlobalConstantsVS.ViewProjection);

            return output;
        }

        [FragmentShader]
        public void PS(VertexOutput input) { }
    }
}
