using System;
using System.IO;
using System.Text;
using OpenSage.Data.RefPack;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Map
{
    public class MapFile
    {
        internal const string FourCcUncompressed = "CkMp";

        public HeightMapData HeightMapData { get; private set; }
        public BlendTileData BlendTileData { get; private set; }
        public WorldInfo WorldInfo { get; private set; }
        public SidesList SidesList { get; private set; }
        public ObjectsList ObjectsList { get; private set; }
        public PolygonTriggers PolygonTriggers { get; private set; }
        public GlobalLighting GlobalLighting { get; private set; }
        public WaypointsList WaypointsList { get; private set; }

        public static Stream Decompress(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                var compressionFlag = reader.ReadUInt32().ToFourCcString();

                switch (compressionFlag)
                {
                    // Uncompressed
                    case FourCcUncompressed:
                        // Back up, so we can read this value again in Parse.
                        stream.Seek(-4, SeekOrigin.Current);
                        return stream;

                    // EA RefPack
                    case "EAR\0":
                        // Compressed (after decompression, contents are exactly the same
                        // as uncompressed format, so we call back into this method)
                        var decompressedSize = reader.ReadUInt32();
                        return new RefPackStream(reader.BaseStream);

                    // Only found this on C&C Generals "Woodcrest Circle" map. Looks like a compressed
                    // or encrypted format, but I can't find any info about it online.
                    case "ZL5\0":
                        throw new NotSupportedException("ZL5-compressed maps are not supported.");

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        internal static T Parse<T>(Stream stream, Func<BinaryReader, T> parseCallback)
        {
            var dataStream = Decompress(stream);

            using (var reader = new BinaryReader(dataStream, Encoding.ASCII, true))
            {
                var compressionFlag = reader.ReadUInt32().ToFourCcString();

                if (compressionFlag != FourCcUncompressed)
                {
                    throw new InvalidDataException();
                }

                return parseCallback(reader);
            }
        }

        public static MapFile FromFileSystemEntry(FileSystemEntry entry)
        {
            using (var stream = entry.Open())
            {
                return FromStream(stream);
            }
        }

        public static MapFile FromStream(Stream stream)
        {
            return Parse(stream, reader => ParseMapData(reader));
        }

        private static MapFile ParseMapData(BinaryReader reader)
        {
            var assetNames = AssetNameCollection.Parse(reader);

            var result = new MapFile();

            var context = new MapParseContext(assetNames);

            context.PushAsset(nameof(MapFile), reader.BaseStream.Length);

            Asset.ParseAssets(reader, context, assetName =>
            {
                switch (assetName)
                {
                    case HeightMapData.AssetName:
                        result.HeightMapData = HeightMapData.Parse(reader, context);
                        break;

                    case BlendTileData.AssetName:
                        result.BlendTileData = BlendTileData.Parse(reader, context, result.HeightMapData);
                        break;

                    case WorldInfo.AssetName:
                        result.WorldInfo = WorldInfo.Parse(reader, context);
                        break;

                    case SidesList.AssetName:
                        result.SidesList = SidesList.Parse(reader, context);
                        break;

                    case ObjectsList.AssetName:
                        result.ObjectsList = ObjectsList.Parse(reader, context);
                        break;

                    case PolygonTriggers.AssetName:
                        result.PolygonTriggers = PolygonTriggers.Parse(reader, context);
                        break;

                    case GlobalLighting.AssetName:
                        result.GlobalLighting = GlobalLighting.Parse(reader, context);
                        break;

                    case WaypointsList.AssetName:
                        result.WaypointsList = WaypointsList.Parse(reader, context);
                        break;

                    default:
                        throw new NotImplementedException(assetName);
                }
            });

            context.PopAsset();

            return result;
        }

        public void WriteTo(Stream stream)
        {
            using (var writer = new BinaryWriter(stream))
            {
                // Always writes an uncompressed map, until (and if) we implement refpack compression.

                writer.Write(FourCcUncompressed.ToFourCc());

                WriteMapDataTo(writer);
            }
        }

        private void WriteMapDataTo(BinaryWriter writer)
        {
            var assetNames = new AssetNameCollection();

            // Do a first pass just to collect the asset names.
            var tempWriter = BinaryWriter.Null;
            WriteMapDataTo(tempWriter, assetNames);

            // Now write out the asset names to the real writer.
            assetNames.WriteTo(writer);

            // And write out the data to the real writer.
            WriteMapDataTo(writer, assetNames);
        }

        private void WriteMapDataTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            writer.Write(assetNames.GetOrCreateAssetIndex(HeightMapData.AssetName));
            HeightMapData.WriteTo(writer);

            writer.Write(assetNames.GetOrCreateAssetIndex(BlendTileData.AssetName));
            BlendTileData.WriteTo(writer);

            writer.Write(assetNames.GetOrCreateAssetIndex(WorldInfo.AssetName));
            WorldInfo.WriteTo(writer, assetNames);

            writer.Write(assetNames.GetOrCreateAssetIndex(SidesList.AssetName));
            SidesList.WriteTo(writer, assetNames);

            writer.Write(assetNames.GetOrCreateAssetIndex(ObjectsList.AssetName));
            ObjectsList.WriteTo(writer, assetNames);

            writer.Write(assetNames.GetOrCreateAssetIndex(PolygonTriggers.AssetName));
            PolygonTriggers.WriteTo(writer);

            writer.Write(assetNames.GetOrCreateAssetIndex(GlobalLighting.AssetName));
            GlobalLighting.WriteTo(writer);

            writer.Write(assetNames.GetOrCreateAssetIndex(WaypointsList.AssetName));
            WaypointsList.WriteTo(writer);
        }
    }
}
