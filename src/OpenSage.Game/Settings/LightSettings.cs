using OpenSage.Graphics.Shaders;

namespace OpenSage.Settings
{
    public sealed class LightSettings
    {
        internal readonly GlobalLightingTypes.LightingConstantsPS TerrainLightsPS;
        internal readonly GlobalLightingTypes.LightingConstantsPS ObjectLightsPS;

        internal LightSettings(
            in GlobalLightingTypes.LightingConstantsPS terrainLightsPS,
            in GlobalLightingTypes.LightingConstantsPS objectLightsPS)
        {
            TerrainLightsPS = terrainLightsPS;
            ObjectLightsPS = objectLightsPS;
        }
    }
}
