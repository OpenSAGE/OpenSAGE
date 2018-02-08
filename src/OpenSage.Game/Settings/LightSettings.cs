using System.Numerics;
using OpenSage.Graphics.Effects;

namespace OpenSage.Settings
{
    public sealed class LightSettings
    {
        public readonly LightingConstantsPS TerrainLightsPS;
        public readonly LightingConstantsPS ObjectLightsPS;

        public LightSettings(in LightingConstantsPS terrainLightsPS, in LightingConstantsPS objectLightsPS)
        {
            TerrainLightsPS = terrainLightsPS;
            ObjectLightsPS = objectLightsPS;
        }
    }
}
