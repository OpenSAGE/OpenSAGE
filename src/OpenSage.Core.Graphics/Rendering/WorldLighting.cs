using System.Collections.Generic;
using System.Numerics;
using OpenSage.Data.Map;
using OpenSage.Graphics.Shaders;

namespace OpenSage.Settings
{
    public sealed class WorldLighting
    {
        public IReadOnlyDictionary<TimeOfDay, LightSettings> LightingConfigurations { get; set; }

        public TimeOfDay TimeOfDay { get; set; }

        public LightSettings CurrentLightingConfiguration => LightingConfigurations[TimeOfDay];

        public bool EnableCloudShadows { get; set; } = true;

        public static WorldLighting CreateDefault()
        {
            var lights = new GlobalShaderResources.LightingConfiguration
            {
                Light0 = new GlobalShaderResources.Light
                {
                    Ambient = new Vector3(0.3f, 0.3f, 0.3f),
                    Direction = Vector3.Normalize(new Vector3(-0.3f, 0.2f, -0.8f)),
                    Color = new Vector3(0.7f, 0.7f, 0.8f)
                },
            };

            var lightingConfigurations = new Dictionary<TimeOfDay, LightSettings>
            {
                {
                    TimeOfDay.Morning,
                    new LightSettings(lights, lights)
                }
            };

            return new WorldLighting(lightingConfigurations, TimeOfDay.Morning);
        }

        public WorldLighting(
            IReadOnlyDictionary<TimeOfDay, LightSettings> lightingConfigurations,
            TimeOfDay timeOfDay)
        {
            LightingConfigurations = lightingConfigurations;
            TimeOfDay = timeOfDay;
        }
    }
}
