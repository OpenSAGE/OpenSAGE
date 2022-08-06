using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using OpenSage.FileFormats;
using OpenSage.Mathematics;

namespace OpenSage.Data.Map
{
    public sealed class GlobalLighting : Asset
    {
        public const string AssetName = "GlobalLighting";

        private static readonly TimeOfDay[] TimeOfDayValues = Enum.GetValues(typeof(TimeOfDay)).Cast<TimeOfDay>().ToArray();

        public TimeOfDay Time { get; private set; }
        
        public Dictionary<TimeOfDay, GlobalLightingConfiguration> LightingConfigurations { get; private set; }

        public MapColorArgb ShadowColor { get; private set; }

        public byte[] Unknown { get; private set; }

        [AddedIn(SageGame.Cnc4)]
        public Vector3? Unknown2 { get; private set; }

        [AddedIn(SageGame.Cnc4)]
        public MapColorArgb? Unknown3 { get; private set; }

        public ColorRgbF? NoCloudFactor { get; private set; }

        internal static GlobalLighting Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var time = reader.ReadUInt32AsEnum<TimeOfDay>();

                var lightingConfigurations = new Dictionary<TimeOfDay, GlobalLightingConfiguration>();

                for (var i = 0; i < TimeOfDayValues.Length; i++)
                {
                    lightingConfigurations[TimeOfDayValues[i]] = GlobalLightingConfiguration.Parse(reader, version);
                }

                var shadowColor = MapColorArgb.Parse(reader);

                // TODO: BFME. Overbright? Bloom?
                byte[] unknown = null;
                if (version >= 7 && version < 11)
                {
                    unknown = reader.ReadBytes(version >= 9 ? 4 : 44);
                }

                Vector3? unknown2 = null;
                MapColorArgb? unknown3 = null;
                if (version >= 12)
                {
                    unknown2 = reader.ReadVector3();
                    unknown3 = MapColorArgb.Parse(reader);
                }

                ColorRgbF? noCloudFactor = null;
                if (version >= 8)
                {
                    noCloudFactor = reader.ReadColorRgbF();
                }

                return new GlobalLighting
                {
                    Time = time,
                    LightingConfigurations = lightingConfigurations,
                    ShadowColor = shadowColor,
                    Unknown = unknown,
                    Unknown2 = unknown2,
                    Unknown3 = unknown3,
                    NoCloudFactor = noCloudFactor
                };
            });
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write((uint) Time);

                for (var i = 0; i < TimeOfDayValues.Length; i++)
                {
                    LightingConfigurations[TimeOfDayValues[i]].WriteTo(writer, Version);
                }

                ShadowColor.WriteTo(writer);

                if (Version >= 7 && Version < 11)
                {
                    writer.Write(Unknown);
                }

                if (Version >= 12)
                {
                    writer.Write(Unknown2.Value);
                    Unknown3.Value.WriteTo(writer);
                }

                if (Version >= 8)
                {
                    writer.Write(NoCloudFactor.Value);
                }
            });
        }
    }
}
