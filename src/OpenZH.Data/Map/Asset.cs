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

            context.PushAsset(endPosition);

            var result = parseCallback(assetVersion);

            context.PopAsset();

            if (reader.BaseStream.Position != endPosition)
            {
                throw new InvalidDataException($"Expected reader to be at position {endPosition}, but was at {reader.BaseStream.Position}.");
            }

            return result;
        }

        public static void ParseAssets(BinaryReader reader, MapParseContext context, AssetsParseCallback parseCallback)
        {
            while (reader.BaseStream.Position < context.CurrentEndPosition)
            {
                var assetIndex = reader.ReadUInt32();

                var assetName = context.AssetNames[assetIndex];

                parseCallback(assetName);
            }
        }
    }

    public delegate T AssetParseCallback<T>(uint assetVersion)
        where T : Asset;

    public delegate void AssetsParseCallback(string assetName);
}
