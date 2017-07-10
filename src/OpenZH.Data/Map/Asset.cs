using System.IO;

namespace OpenZH.Data.Map
{
    public abstract class Asset
    {
        public static T ParseAsset<T>(BinaryReader reader, MapParseContext context, AssetParseCallback<T> parseCallback)
            where T : Asset
        {
            var assetVersion = reader.ReadUInt16();

            var dataSize = reader.ReadUInt32();
            var startPosition = reader.BaseStream.Position;
            var endPosition = dataSize + startPosition;

            context.PushAsset(typeof(T).Name, endPosition);

            var result = parseCallback(assetVersion);

            context.PopAsset();

            if (reader.BaseStream.Position != endPosition)
            {
                throw new InvalidDataException($"Error while parsing asset '{typeof(T).Name}'. Expected reader to be at position {endPosition}, but was at {reader.BaseStream.Position}.");
            }

            return result;
        }

        public static void ParseAssets(BinaryReader reader, MapParseContext context, AssetsParseCallback parseCallback)
        {
            while (reader.BaseStream.Position < context.CurrentEndPosition)
            {
                var assetIndex = reader.ReadUInt32();

                var assetName = context.GetAssetName(assetIndex);

                parseCallback(assetName);
            }
        }

        public static AssetProperty[] ParseProperties(BinaryReader reader, MapParseContext context)
        {
            var numProperties = reader.ReadUInt16();
            var result = new AssetProperty[numProperties];

            for (var i = 0; i < numProperties; i++)
            {
                result[i] = AssetProperty.Parse(reader, context);
            }

            return result;
        }
    }

    public delegate T AssetParseCallback<T>(uint assetVersion)
        where T : Asset;

    public delegate void AssetsParseCallback(string assetName);
}
