using System.Collections.Generic;
using System.Numerics;
using OpenSage.Data.Map;
using OpenSage.Graphics.Effects;

namespace OpenSage.Settings
{
    public sealed class SceneSettings
    {
        public Dictionary<TimeOfDay, LightSettings> LightingConfigurations { get; set; }

        public TimeOfDay TimeOfDay { get; set; }

        internal LightSettings CurrentLightingConfiguration => LightingConfigurations[TimeOfDay];

        public SceneSettings()
        {
            var lights = new Lights
            {
                Light0 = new Light
                {
                    Ambient = new Vector3(0.3f, 0.3f, 0.3f),
                    Direction = Vector3.Normalize(new Vector3(-0.3f, 0.2f, -0.8f)),
                    Color = new Vector3(0.7f, 0.7f, 0.8f)
                }
            };

            LightingConfigurations = new Dictionary<TimeOfDay, LightSettings>
            {
                {
                    TimeOfDay.Morning,
                    new LightSettings
                    {
                        TerrainLights = lights,
                        ObjectLights = lights
                    }
                }
            };

            TimeOfDay = TimeOfDay.Morning;
        }
    }
}
