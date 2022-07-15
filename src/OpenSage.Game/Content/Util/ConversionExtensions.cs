using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Map;
using OpenSage.Data.Wnd;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;
using OpenSage.Settings;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Content.Util
{
    internal static class ConversionExtensions
    {
        public static Dictionary<TimeOfDay, LightSettings> ToLightSettingsDictionary(this Dictionary<TimeOfDay, GlobalLightingConfiguration> value)
        {
            return value.ToDictionary(
                x => x.Key,
                x => x.Value.ToLightSettings());
        }

        private static LightSettings ToLightSettings(this GlobalLightingConfiguration mapLightingConfiguration)
        {
            return new LightSettings(
                new GlobalShaderResources.LightingConfiguration
                {
                    Light0 = ToLight(mapLightingConfiguration.TerrainSun),
                    Light1 = ToLight(mapLightingConfiguration.TerrainAccent1),
                    Light2 = ToLight(mapLightingConfiguration.TerrainAccent2),
                },
                new GlobalShaderResources.LightingConfiguration
                {
                    // RA3 and later only had one light defined per time of day.
                    Light0 = ToLight(mapLightingConfiguration.ObjectSun ?? mapLightingConfiguration.TerrainSun),
                    Light1 = ToLight(mapLightingConfiguration.ObjectAccent1 ?? mapLightingConfiguration.TerrainAccent1),
                    Light2 = ToLight(mapLightingConfiguration.ObjectAccent2 ?? mapLightingConfiguration.TerrainAccent2),
                }
                // TODO: Infantry lights
                );
        }

        private static GlobalShaderResources.Light ToLight(GlobalLight mapLight)
        {
            return new GlobalShaderResources.Light
            {
                Ambient = mapLight.Ambient,
                Color = mapLight.Color,
                Direction = Vector3.Normalize(mapLight.Direction)
            };
        }

        public static Rectangle ToRectangle(this WndScreenRect value)
        {
            return new Rectangle(
                value.UpperLeft.X,
                value.UpperLeft.Y,
                value.BottomRight.X - value.UpperLeft.X,
                value.BottomRight.Y - value.UpperLeft.Y);
        }

        public static ColorRgbaF ToColorRgbaF(this ColorRgba value)
        {
            return new ColorRgbaF(
                value.R / 255.0f,
                value.G / 255.0f,
                value.B / 255.0f,
                value.A / 255.0f);
        }
    }
}
