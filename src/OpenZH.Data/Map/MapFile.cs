using System;
using System.IO;
using OpenZH.Data.RefPack;

namespace OpenZH.Data.Map
{
    public sealed class MapFile
    {
        public HeightMapData HeightMapData { get; private set; }
        public BlendTileData BlendTileData { get; private set; }
        public WorldInfo WorldInfo { get; private set; }
        public SidesList SidesList { get; private set; }

        public static MapFile Parse(BinaryReader reader)
        {
            var compressionFlag = reader.ReadUInt32();

            switch (compressionFlag)
            {
                case 1884121923u:
                    // Uncompressed
                    return ParseMapData(reader);

                case 5390661u:
                    // Compressed (after decompression, contents are exactly the same
                    // as uncompressed format, so we call back into this method)
                    var decompressedSize = reader.ReadUInt32();
                    var innerReader = new BinaryReader(new RefPackStream(reader.BaseStream));
                    return Parse(innerReader);

                default:
                    throw new NotSupportedException();
            }
        }

        private static MapFile ParseMapData(BinaryReader reader)
        {
            var assetStringsLength = reader.ReadUInt32();
            var assetStrings = new string[assetStringsLength];

            for (var i = (int) (assetStringsLength - 1); i >= 0; i--)
            {
                assetStrings[i] = reader.ReadString();
                var assetIndex = reader.ReadUInt32();
                if (assetIndex != i + 1)
                {
                    throw new InvalidDataException();
                }
            }

            var result = new MapFile();

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var assetIndex = reader.ReadUInt32(); // Asset index?

                var unknown = reader.ReadUInt16(); // TODO

                var dataSize = reader.ReadUInt32();

                var startPosition = reader.BaseStream.Position;

                var key = assetStrings[assetIndex - 1];

                switch (key)
                {
                    case "HeightMapData":
                        result.HeightMapData = HeightMapData.Parse(reader);
                        break;

                    case "BlendTileData":
                        if (result.HeightMapData == null)
                        {
                            throw new InvalidDataException("Expected HeightMapData block before BlendTileData block.");
                        }
                        result.BlendTileData = BlendTileData.Parse(reader, result.HeightMapData);
                        break;

                    case "WorldInfo":
                        result.WorldInfo = WorldInfo.Parse(reader, assetStrings);
                        break;

                    case "SidesList":
                        result.SidesList = SidesList.Parse(reader, assetStrings);
                        break;

                    default:
                        // TODO
                        reader.ReadBytes((int) dataSize);
                        break;
                }

                if (startPosition + dataSize != reader.BaseStream.Position)
                {
                    throw new Exception($"Parsed the wrong number of bytes in {key} chunk. Parsed {reader.BaseStream.Position - startPosition}, expected {dataSize}.");
                }
            }

            return result;
        }
    }
}
