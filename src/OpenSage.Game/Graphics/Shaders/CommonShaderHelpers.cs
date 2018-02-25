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

        public const float AlphaTestThreshold = 0x60 / (float) 0xFF;
    }
}
