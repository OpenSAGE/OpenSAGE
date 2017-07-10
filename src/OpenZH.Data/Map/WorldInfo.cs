using System.IO;

namespace OpenZH.Data.Map
{
    public sealed class WorldInfo : Asset
    {
        public AssetProperty[] Properties { get; private set; }

        public static WorldInfo Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                return new WorldInfo
                {
                    Properties = ParseProperties(reader, context)
                };
            });
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
