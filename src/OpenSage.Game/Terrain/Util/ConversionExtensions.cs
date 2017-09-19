using System.Numerics;
using OpenSage.Data.Map;
using OpenSage.Graphics.Effects;

namespace OpenSage.Terrain.Util
{
    internal static class ConversionExtensions
    {
        public static Vector3 ToVector3(this MapVector3 value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }

        public static Vector2 ToVector2(this MapTexCoord value)
        {
            return new Vector2(value.U, -value.V);
        }

        public static Lights ToLights(this GlobalLightingConfiguration mapLightingConfiguration)
        {
            return new Lights
            {
                Light0 = ToLight(mapLightingConfiguration.TerrainSun),
                Light1 = ToLight(mapLightingConfiguration.TerrainAccent1),
                Light2 = ToLight(mapLightingConfiguration.TerrainAccent2),
            };
        }

        private static Light ToLight(GlobalLight mapLight)
        {
            return new Light
            {
                Ambient = mapLight.Ambient.ToVector3(),
                Color = mapLight.Color.ToVector3(),
                Direction = Vector3.Normalize(mapLight.EulerAngles.ToVector3())
            };
        }
    }
}
