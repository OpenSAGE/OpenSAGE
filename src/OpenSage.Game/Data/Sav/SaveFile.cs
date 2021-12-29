using System.IO;
using System.Text;
using OpenSage.FileFormats;
using OpenSage.Graphics.Cameras;
using OpenSage.IO;

namespace OpenSage.Data.Sav
{
    public static class SaveFile
    {
        public static GameState GetGameState(FileSystemEntry entry, Game game)
        {
            using (var stream = entry.Open())
            using (var binaryReader = new BinaryReader(stream, Encoding.Unicode, true))
            {
                var reader = new StatePersister(binaryReader, game);

                while (true)
                {
                    var chunkName = "";
                    reader.ReadAsciiString(ref chunkName);

                    reader.BeginSegment(chunkName);

                    if (chunkName == "CHUNK_GameState")
                    {
                        var gameState = new GameState();
                        gameState.Load(reader);
                        return gameState;
                    }

                    reader.EndSegment();
                }
            }

            throw new InvalidStateException();
        }

        public static void Load(FileSystemEntry entry, Game game)
        {
            using var stream = entry.Open();
            LoadFromStream(stream, game);
        }

        public static void LoadFromStream(Stream stream, Game game)
        {
            using var binaryReader = new BinaryReader(stream, Encoding.Unicode, true);

            var reader = new StatePersister(binaryReader, game);

            if (reader.SageGame >= SageGame.Bfme)
            {
                var header1 = reader.Inner.ReadFourCc(bigEndian: true);
                if (header1 != "EALA")
                {
                    throw new InvalidStateException();
                }

                var header2 = reader.Inner.ReadFourCc(bigEndian: true);
                if (header2 != "RTS1")
                {
                    throw new InvalidStateException();
                }

                var header3 = reader.Inner.ReadUInt32();
                if (header3 != 0)
                {
                    throw new InvalidStateException();
                }
            }

            while (true)
            {
                var chunkName = "";
                reader.ReadAsciiString(ref chunkName);

                if (chunkName == "SG_EOF")
                {
                    if (stream.Position != stream.Length)
                    {
                        throw new InvalidStateException();
                    }
                    break;
                }

                var chunkLength = reader.BeginSegment(chunkName);

                switch (chunkName)
                {
                    case "CHUNK_GameState":
                        game.GameState.Load(reader);
                        break;

                    case "CHUNK_Campaign":
                        game.CampaignManager.Load(reader);
                        break;

                    case "CHUNK_GameStateMap":
                        GameStateMap.Load(reader, game);
                        game.Scene3D.PartitionCellManager.OnNewGame();
                        break;

                    case "CHUNK_TerrainLogic":
                        game.TerrainLogic.Load(reader);
                        break;

                    case "CHUNK_TeamFactory":
                        game.Scene3D.TeamFactory.Load(reader, game.Scene3D.PlayerManager);
                        break;

                    case "CHUNK_Players":
                        game.Scene3D.PlayerManager.Load(reader, game);
                        break;

                    case "CHUNK_GameLogic":
                        game.Scene3D.GameLogic.Load(reader);
                        break;

                    case "CHUNK_ParticleSystem":
                        game.Scene3D.ParticleSystemManager.Load(reader);
                        break;

                    case "CHUNK_Radar":
                        game.Scene3D.Radar.Load(reader);
                        break;

                    case "CHUNK_ScriptEngine":
                        game.Scripting.Load(reader);
                        break;

                    case "CHUNK_SidesList":
                        game.Scene3D.PlayerScripts.Load(reader);
                        break;

                    case "CHUNK_TacticalView":
                        ((RtsCameraController) game.Scene3D.CameraController).Load(reader);
                        break;

                    case "CHUNK_GameClient":
                        game.Scene3D.GameClient.Load(reader);
                        break;

                    case "CHUNK_InGameUI":
                        game.AssetStore.InGameUI.Current.Load(reader);
                        break;

                    case "CHUNK_Partition":
                        game.Scene3D.PartitionCellManager.Load(reader);
                        break;

                    case "CHUNK_TerrainVisual":
                        game.TerrainVisual.Load(reader, game);
                        break;

                    case "CHUNK_GhostObject":
                        game.GhostObjectManager.Load(reader, game.Scene3D.GameLogic, game);
                        break;

                    case "CHUNK_LivingWorldLogic":
                        stream.Position += chunkLength;
                        break;

                    case "CHUNK_Audio":
                        stream.Position += chunkLength;
                        break;

                    case "CHUNK_Palantir":
                        stream.Position += chunkLength;
                        break;

                    default:
                        throw new InvalidDataException($"Unknown chunk type '{chunkName}'.");
                }

                reader.EndSegment();
            }

            // If we haven't started a game yet (which will be the case for
            // "mission start" save files), then start it now.
            if (!game.InGame)
            {
                game.StartCampaign(
                    game.CampaignManager.CampaignName,
                    game.CampaignManager.MissionName);
            }
        }
    }

    public enum SaveGameType : uint
    {
        Skirmish,
        SinglePlayer
    }
}
