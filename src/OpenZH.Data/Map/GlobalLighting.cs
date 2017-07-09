using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public sealed class GlobalLighting : Asset
    {
        public TimeOfDay Time { get; private set; }
        
        public Dictionary<TimeOfDay, GlobalLightingConfiguration> LightingConfigurations { get; private set; }

        public static GlobalLighting Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var time = reader.ReadUInt32AsEnum<TimeOfDay>();

                var lightingConfigurations = new Dictionary<TimeOfDay, GlobalLightingConfiguration>();

                var timeOfDayValues = Enum.GetValues(typeof(TimeOfDay)).Cast<uint>().ToArray();

                for (var i = 0; i < timeOfDayValues.Length; i++)
                {
                    lightingConfigurations[(TimeOfDay) timeOfDayValues[i]] = GlobalLightingConfiguration.Parse(reader);
                }

                var unknown = reader.ReadSingle();
                if (!float.IsNaN(unknown))
                {
                    throw new InvalidDataException();
                }

                return new GlobalLighting
                {
                    Time = time,
                    LightingConfigurations = lightingConfigurations
                };
            });
        }
    }

    public enum TimeOfDay : uint
    {
        Morning = 1,
        Afternoon,
        Evening,
        Night
    }
}
