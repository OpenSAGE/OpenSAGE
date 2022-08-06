using OpenSage.Graphics.Shaders;

namespace OpenSage.Settings
{
    public sealed class LightSettings
    {
        public readonly GlobalShaderResources.LightingConstantsPS LightsPS;

        public LightSettings(
            in GlobalShaderResources.LightingConfiguration terrainLightsPS,
            in GlobalShaderResources.LightingConfiguration objectLightsPS)
        {
            LightsPS.Terrain = terrainLightsPS;
            LightsPS.Object = objectLightsPS;
        }
    }
}
