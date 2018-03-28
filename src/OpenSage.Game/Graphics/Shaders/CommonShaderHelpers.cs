using System.Numerics;
using ShaderGen;

namespace OpenSage.Graphics.Shaders
{
    public static class CommonShaderHelpers
    {
        public struct VertexShaderOutputCommon
        {
            [SystemPositionSemantic] public Vector4 Position;
        }

        public struct GlobalConstantsShared
        {
            public Vector3 CameraPosition;
            public float TimeInSeconds;
        }

        public struct GlobalConstantsVS
        {
            public Matrix4x4 ViewProjection;
        }

        public struct GlobalConstantsPS
        {
            public Vector2 ViewportSize;
        }

        public static bool FailsAlphaTest(float alpha)
        {
            // 0x60 / 0xFF = 0.37647
            return alpha < 0.37647f;
        }

        public static Vector3 TransformNormal(Vector3 v, Matrix4x4 m)
        {
            return Vector4.Transform(new Vector4(v, 0), m).XYZ();
        }
    }
}
