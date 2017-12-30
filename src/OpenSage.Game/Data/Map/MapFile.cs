using System;
using System.IO;
using System.IO.Compression;
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

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public MPPositionList MPPositionList { get; private set; }

        public SidesList SidesList { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public LibraryMapLists LibraryMapLists { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public Teams Teams { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public PlayerScriptsList PlayerScriptsList { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public BuildLists BuildLists { get; private set; }

        public ObjectsList ObjectsList { get; private set; }
        public PolygonTriggers PolygonTriggers { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public TriggerAreas TriggerAreas { get; private set; }

        public GlobalLighting GlobalLighting { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public EnvironmentData EnvironmentData { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public NamedCameras NamedCameras { get; private set; }

        public WaypointsList WaypointsList { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public SkyboxSettings SkyboxSettings { get; private set; }

        public static Stream Decompress(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                var compressionFlag = reader.ReadUInt32().ToFourCcString();

                uint decompressedSize;
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
                        decompressedSize = reader.ReadUInt32();
                        return new RefPackStream(reader.BaseStream);

                    // Zlib. Only found this on C&C Generals "Woodcrest Circle" map.
                    // Thanks to OmniBlade for figuring out that it's zlib copression.
                    case "ZL5\0":
                        decompressedSize = reader.ReadUInt32();
                        // We have the zlib header bytes, but .NET's DeflateStream only supports
                        // the Deflate section of the zlib container.
                        var zlibHeader1 = reader.ReadByte();
                        var zlibHeader2 = reader.ReadByte();
                        if (zlibHeader1 != 0x78 || zlibHeader2 != 0x9C)
                        {
                            throw new InvalidDataException();
                        }
                        return new DeflateStream(reader.BaseStream, CompressionMode.Decompress);

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

                    case MPPositionList.AssetName:
                        result.MPPositionList = MPPositionList.Parse(reader, context);
                        break;

                    case SidesList.AssetName:
                        result.SidesList = SidesList.Parse(reader, context);
                        break;

                    case LibraryMapLists.AssetName:
                        result.LibraryMapLists = LibraryMapLists.Parse(reader, context);
                        break;

                    case Teams.AssetName:
                        result.Teams = Teams.Parse(reader, context);
                        break;

                    case PlayerScriptsList.AssetName:
                        result.PlayerScriptsList = PlayerScriptsList.Parse(reader, context);
                        break;

                    case BuildLists.AssetName:
                        result.BuildLists = BuildLists.Parse(reader, context);
                        break;

                    case ObjectsList.AssetName:
                        result.ObjectsList = ObjectsList.Parse(reader, context);
                        break;

                    case PolygonTriggers.AssetName:
                        result.PolygonTriggers = PolygonTriggers.Parse(reader, context);
                        break;

                    case TriggerAreas.AssetName:
                        result.TriggerAreas = TriggerAreas.Parse(reader, context);
                        break;

                    case GlobalLighting.AssetName:
                        result.GlobalLighting = GlobalLighting.Parse(reader, context);
                        break;

                    case EnvironmentData.AssetName:
                        result.EnvironmentData = EnvironmentData.Parse(reader, context);
                        break;

                    case NamedCameras.AssetName:
                        result.NamedCameras = NamedCameras.Parse(reader, context);
                        break;

                    case WaypointsList.AssetName:
                        result.WaypointsList = WaypointsList.Parse(reader, context);
                        break;

                    case SkyboxSettings.AssetName:
                        result.SkyboxSettings = SkyboxSettings.Parse(reader, context);
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

            if (MPPositionList != null)
            {
                writer.Write(assetNames.GetOrCreateAssetIndex(MPPositionList.AssetName));
                MPPositionList.WriteTo(writer, assetNames);
            }

            writer.Write(assetNames.GetOrCreateAssetIndex(SidesList.AssetName));
            SidesList.WriteTo(writer, assetNames);

            if (LibraryMapLists != null)
            {
                writer.Write(assetNames.GetOrCreateAssetIndex(LibraryMapLists.AssetName));
                LibraryMapLists.WriteTo(writer, assetNames);
            }

            if (Teams != null)
            {
                writer.Write(assetNames.GetOrCreateAssetIndex(Teams.AssetName));
                Teams.WriteTo(writer, assetNames);
            }

            if (PlayerScriptsList != null)
            {
                writer.Write(assetNames.GetOrCreateAssetIndex(PlayerScriptsList.AssetName));
                PlayerScriptsList.WriteTo(writer, assetNames);
            }

            if (BuildLists != null)
            {
                writer.Write(assetNames.GetOrCreateAssetIndex(BuildLists.AssetName));
                BuildLists.WriteTo(writer, assetNames);
            }

            writer.Write(assetNames.GetOrCreateAssetIndex(ObjectsList.AssetName));
            ObjectsList.WriteTo(writer, assetNames);

            writer.Write(assetNames.GetOrCreateAssetIndex(PolygonTriggers.AssetName));
            PolygonTriggers.WriteTo(writer);

            writer.Write(assetNames.GetOrCreateAssetIndex(GlobalLighting.AssetName));
            GlobalLighting.WriteTo(writer);

            if (EnvironmentData != null)
            {
                writer.Write(assetNames.GetOrCreateAssetIndex(EnvironmentData.AssetName));
                EnvironmentData.WriteTo(writer);
            }

            if (NamedCameras != null)
            {
                writer.Write(assetNames.GetOrCreateAssetIndex(NamedCameras.AssetName));
                NamedCameras.WriteTo(writer);
            }

            writer.Write(assetNames.GetOrCreateAssetIndex(WaypointsList.AssetName));
            WaypointsList.WriteTo(writer);

            if (SkyboxSettings != null)
            {
                writer.Write(assetNames.GetOrCreateAssetIndex(SkyboxSettings.AssetName));
                SkyboxSettings.WriteTo(writer);
            }
        }
    }
}
