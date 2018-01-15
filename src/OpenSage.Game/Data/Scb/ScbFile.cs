using System.IO;
using OpenSage.Data.Map;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Scb
{
    public sealed class ScbFile
    {
        public ScriptImportSize ScriptImportSize { get; private set; }
        public PlayerScriptsList PlayerScripts { get; private set; }
        public NamedCameras NamedCameras { get; private set; }
        public CameraAnimationList CameraAnimationList { get; private set; }
        public ScriptsPlayers Players { get; private set; }
        public ObjectsList ObjectsList { get; private set; }
        public PolygonTriggers PolygonTriggers { get; private set; }
        public ScriptTeams Teams { get; private set; }
        public WaypointsList WaypointsList { get; private set; }

        public static ScbFile FromStream(Stream stream)
        {
            return MapFile.Parse(stream, reader => ParseScbData(reader));
        }

        private static ScbFile ParseScbData(BinaryReader reader)
        {
            var assetNames = AssetNameCollection.Parse(reader);

            var result = new ScbFile();

            var context = new MapParseContext(assetNames);

            context.PushAsset(nameof(ScbFile), reader.BaseStream.Length);

            Asset.ParseAssets(reader, context, assetName =>
            {
                switch (assetName)
                {
                    case ScriptImportSize.AssetName:
                        result.ScriptImportSize = ScriptImportSize.Parse(reader, context);
                        break;

                    case PlayerScriptsList.AssetName:
                        result.PlayerScripts = PlayerScriptsList.Parse(reader, context);
                        break;

                    case NamedCameras.AssetName:
                        result.NamedCameras = NamedCameras.Parse(reader, context);
                        break;

                    case CameraAnimationList.AssetName:
                        result.CameraAnimationList = CameraAnimationList.Parse(reader, context);
                        break;

                    case ScriptsPlayers.AssetName:
                        result.Players = ScriptsPlayers.Parse(reader, context);
                        break;

                    case ObjectsList.AssetName:
                        result.ObjectsList = ObjectsList.Parse(reader, context);
                        break;

                    case PolygonTriggers.AssetName:
                        result.PolygonTriggers = PolygonTriggers.Parse(reader, context);
                        break;

                    case ScriptTeams.AssetName:
                        result.Teams = ScriptTeams.Parse(reader, context);
                        break;

                    case WaypointsList.AssetName:
                        result.WaypointsList = WaypointsList.Parse(reader, context);
                        break;

                    default:
                        throw new InvalidDataException($"Unknown asset name: {assetName}");
                }
            });

            context.PopAsset();

            return result;
        }

        public void WriteTo(Stream stream)
        {
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(MapFile.FourCcUncompressed.ToFourCc());

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
            if (ScriptImportSize != null)
            {
                writer.Write(assetNames.GetOrCreateAssetIndex(ScriptImportSize.AssetName));
                ScriptImportSize.WriteTo(writer, assetNames);
            }

            writer.Write(assetNames.GetOrCreateAssetIndex(PlayerScriptsList.AssetName));
            PlayerScripts.WriteTo(writer, assetNames);

            if (NamedCameras != null)
            {
                writer.Write(assetNames.GetOrCreateAssetIndex(NamedCameras.AssetName));
                NamedCameras.WriteTo(writer);
            }

            if (CameraAnimationList != null)
            {
                writer.Write(assetNames.GetOrCreateAssetIndex(CameraAnimationList.AssetName));
                CameraAnimationList.WriteTo(writer);
            }

            writer.Write(assetNames.GetOrCreateAssetIndex(ScriptsPlayers.AssetName));
            Players.WriteTo(writer);

            writer.Write(assetNames.GetOrCreateAssetIndex(ObjectsList.AssetName));
            ObjectsList.WriteTo(writer, assetNames);

            writer.Write(assetNames.GetOrCreateAssetIndex(PolygonTriggers.AssetName));
            PolygonTriggers.WriteTo(writer);

            writer.Write(assetNames.GetOrCreateAssetIndex(ScriptTeams.AssetName));
            Teams.WriteTo(writer, assetNames);

            writer.Write(assetNames.GetOrCreateAssetIndex(WaypointsList.AssetName));
            WaypointsList.WriteTo(writer);
        }
    }
}
