using System.Collections.Generic;
using OpenSage.Data.Map;

namespace OpenSage.Settings
{
    public sealed class SceneSettings
    {
        public Dictionary<TimeOfDay, GlobalLightingConfiguration> LightingConfigurations { get; }

        public TimeOfDay TimeOfDay { get; set; }

        internal GlobalLightingConfiguration CurrentLightingConfiguration => LightingConfigurations[TimeOfDay];

        public SceneSettings(GlobalLighting globalLighting)
        {
            LightingConfigurations = globalLighting.LightingConfigurations;
            TimeOfDay = globalLighting.Time;
        }
    }
}
