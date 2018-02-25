using System.Numerics;
using ShaderGen;
using static ShaderGen.ShaderBuiltins;

namespace OpenSage.Graphics.Shaders
{
    public static class CloudHelpers
    {
        public static Vector2 GetCloudUV(
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

        public static Vector3 GetCloudColor(
            Texture2DResource cloudTexture,
            SamplerResource samplerState,
            Vector2 cloudUV)
        {
            return Vector3.Zero;
            //return Sample(cloudTexture, samplerState, cloudUV).XYZ();
        }
    }
}
