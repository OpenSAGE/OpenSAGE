using System;
using System.Collections.Generic;
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
        public ObjectsList ObjectsList { get; private set; }
        public PolygonTriggers PolygonTriggers { get; private set; }
        public GlobalLighting GlobalLighting { get; private set; }
        public WaypointsList WaypointsList { get; private set; }

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

            var assetNames = new Dictionary<uint, string>();
            for (var i = (int) (assetStringsLength - 1); i >= 0; i--)
            {
                var assetName = reader.ReadString();
                var assetIndex = reader.ReadUInt32();
                if (assetIndex != i + 1)
                {
                    throw new InvalidDataException();
                }
                assetNames[assetIndex] = assetName;
            }

            var result = new MapFile();

            var context = new MapParseContext(assetNames, result);

            context.PushAsset(reader.BaseStream.Length);

            Asset.ParseAssets(reader, context, assetName =>
            {
                switch (assetName)
                {
                    case "HeightMapData":
                        result.HeightMapData = HeightMapData.Parse(reader, context);
                        break;

                    case "BlendTileData":
                        result.BlendTileData = BlendTileData.Parse(reader, context);
                        break;

                    case "WorldInfo":
                        result.WorldInfo = WorldInfo.Parse(reader, context);
                        break;

                    case "SidesList":
                        result.SidesList = SidesList.Parse(reader, context);
                        break;

                    case "ObjectsList":
                        result.ObjectsList = ObjectsList.Parse(reader, context);
                        break;

                    case "PolygonTriggers":
                        result.PolygonTriggers = PolygonTriggers.Parse(reader, context);
                        break;

                    case "GlobalLighting":
                        result.GlobalLighting = GlobalLighting.Parse(reader, context);
                        break;

                    case "WaypointsList":
                        result.WaypointsList = WaypointsList.Parse(reader, context);
                        break;

                    default:
                        // TODO
                        throw new NotImplementedException(assetName);
                }
            });

            context.PopAsset();

            return result;
        }
    }
}
