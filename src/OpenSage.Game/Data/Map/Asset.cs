using System;
using System.IO;

namespace OpenSage.Data.Map
{
    public abstract class Asset
    {
        internal static T ParseAsset<T>(BinaryReader reader, MapParseContext context, AssetParseCallback<T> parseCallback)
            where T : Asset
        {
            var assetVersion = reader.ReadUInt16();

            var dataSize = reader.ReadUInt32();
            var startPosition = reader.BaseStream.Position;
            var endPosition = dataSize + startPosition;

            context.PushAsset(typeof(T).Name, endPosition);

            var result = parseCallback(assetVersion);

            result.StartPosition = startPosition;
            result.EndPosition = endPosition;
            result.Version = assetVersion;

            context.PopAsset();

            if (reader.BaseStream.Position != endPosition)
            {
                throw new InvalidDataException($"Error while parsing asset '{typeof(T).Name}'. Expected reader to be at position {endPosition}, but was at {reader.BaseStream.Position}.");
            }

            return result;
        }

        internal static void ParseAssets(BinaryReader reader, MapParseContext context, AssetsParseCallback parseCallback)
        {
            while (reader.BaseStream.Position < context.CurrentEndPosition)
            {
                var assetIndex = reader.ReadUInt32();

                var assetName = context.GetAssetName(assetIndex);

                parseCallback(assetName);
            }
        }

        // For debugging.
        internal long StartPosition { get; private set; }
        internal long EndPosition { get; private set; }

        public ushort Version { get; private set; }

        protected void WriteAssetTo(BinaryWriter writer, Action writeCallback)
        {
            writer.Write(Version);

            var dataSizePosition = writer.BaseStream.Position;

            writer.Write(0u); // Placeholder, we'll back up and overwrite this later.

            var startPosition = writer.BaseStream.Position;

            writeCallback();

            var endPosition = writer.BaseStream.Position;

            var dataSize = endPosition - startPosition;

            // Back up and write data size.
            writer.BaseStream.Position = dataSizePosition;
            writer.Write((uint) dataSize);
            writer.BaseStream.Position = endPosition;
        }
    }

    internal delegate T AssetParseCallback<T>(ushort assetVersion)
        where T : Asset;

    internal delegate void AssetsParseCallback(string assetName);
}
