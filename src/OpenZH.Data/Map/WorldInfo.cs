using System.IO;
using OpenZH.Data.Ini;

namespace OpenZH.Data.Map
{
    public sealed class WorldInfo : Asset
    {
        public const string AssetName = "WorldInfo";

        public AssetPropertyCollection Properties { get; private set; }

        internal static WorldInfo Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                return new WorldInfo
                {
                    Properties = AssetPropertyCollection.Parse(reader, context)
                };
            });
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            WriteAssetTo(writer, () =>
            {
                Properties.WriteTo(writer, assetNames);
            });
        }
    }

    public enum MapWeatherType : uint
    {
        [IniEnum("NORMAL")]
        Normal,

        Snowy
    }

    public enum MapCompressionType : uint
    {
        None,
        RefPack
    }
}
