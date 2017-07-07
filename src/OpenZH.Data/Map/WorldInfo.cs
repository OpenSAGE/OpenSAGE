using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public sealed class WorldInfo
    {
        public string MapName { get; private set; }
        public WeatherType Weather { get; private set; }
        public CompressionType Compression { get; private set; }

        public static WorldInfo Parse(BinaryReader reader, string[] assetStrings)
        {
            var numProperties = reader.ReadUInt16();

            var result = new WorldInfo();

            for (var i = 0; i < numProperties; i++)
            {
                var propertyType = reader.ReadUInt32();
                var propertyName = assetStrings[(propertyType >> 8) - 1];

                switch (propertyName)
                {
                    case "mapName":
                        result.MapName = reader.ReadUInt16PrefixedAsciiString();
                        break;

                    case "weather":
                        result.Weather = reader.ReadUInt32AsEnum<WeatherType>();
                        break;

                    case "compression":
                        result.Compression = reader.ReadUInt32AsEnum<CompressionType>();
                        break;

                    default:
                        throw new InvalidDataException($"Unexpected property name: {propertyName}");
                }
            }

            return result;
        }

        public enum WeatherType : uint
        {
            Normal,
            Snowy
        }

        public enum CompressionType : uint
        {
            None,
            RefPack
        }
    }
}
