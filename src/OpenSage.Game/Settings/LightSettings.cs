using OpenSage.Graphics.Shaders;

namespace OpenSage.Settings
{
    public sealed class LightSettings
    {
        internal readonly GlobalShaderResources.LightingConstantsPS TerrainLightsPS;
        internal readonly GlobalShaderResources.LightingConstantsPS ObjectLightsPS;

        internal LightSettings(
            in GlobalShaderResources.LightingConstantsPS terrainLightsPS,
            in GlobalShaderResources.LightingConstantsPS objectLightsPS)
        {
            TerrainLightsPS = terrainLightsPS;
            ObjectLightsPS = objectLightsPS;
        }
    }
}
