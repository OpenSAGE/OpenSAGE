using System.IO;

namespace OpenZH.Data.Map
{
    public sealed class WorldInfo : Asset
    {
        public const string AssetName = "WorldInfo";

        public AssetPropertyCollection Properties { get; private set; }

        public static WorldInfo Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                return new WorldInfo
                {
                    Properties = AssetPropertyCollection.Parse(reader, context)
                };
            });
        }

        public void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            WriteAssetTo(writer, () =>
            {
                Properties.WriteTo(writer, assetNames);
            });
        }
    }

    public enum MapWeatherType : uint
    {
        Normal,
        Snowy
    }

    public enum MapCompressionType : uint
    {
        None,
        RefPack
    }
}
