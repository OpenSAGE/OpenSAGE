using System.Numerics;
using System.Runtime.InteropServices;

namespace OpenSage.Graphics.Shaders
{
    internal static class GlobalTypes
    {
        [StructLayout(LayoutKind.Sequential, Size = 16)]
        public struct GlobalConstantsShared
        {
            public Vector3 CameraPosition;
            public float TimeInSeconds;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GlobalConstantsVS
        {
            public Matrix4x4 ViewProjection;
        }

        [StructLayout(LayoutKind.Sequential, Size = 16)]
        public struct GlobalConstantsPS
        {
            public Vector2 ViewportSize;
        }
    }
}
