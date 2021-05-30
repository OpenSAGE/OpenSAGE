using System.IO;
using System.Text;
using OpenSage.Client;
using OpenSage.Data.Map;
using OpenSage.Graphics.Cameras;
using OpenSage.IO;
using OpenSage.Logic;
using OpenSage.Terrain;

namespace OpenSage.Data.Sav
{
    public static class SaveFile
    {
        public static GameState GetGameState(FileSystemEntry entry)
        {
            using (var stream = entry.Open())
            using (var binaryReader = new BinaryReader(stream, Encoding.Unicode, true))
            {
                var reader = new SaveFileReader(binaryReader);

                while (true)
                {
                    var chunkName = reader.ReadAsciiString();

                    reader.BeginSegment();

                    if (chunkName == "CHUNK_GameState")
                    {
                        var gameState = new GameState();
                        gameState.Load(reader);
                        return gameState;
                    }

                    reader.EndSegment();
                }
            }

            throw new InvalidDataException();
        }

        public static void Load(FileSystemEntry entry, Game game)
        {
            using var stream = entry.Open();
            LoadFromStream(stream, game);
        }

        public static void LoadFromStream(Stream stream, Game game)
        {
            using var binaryReader = new BinaryReader(stream, Encoding.Unicode, true);

            var reader = new SaveFileReader(binaryReader);

            var gameState = new GameState();
            GameLogic gameLogic = null;
            GameClient gameClient = null;
            CampaignManager campaignManager = null;
            var terrainLogic = new TerrainLogic();
            var terrainVisual = new TerrainVisual();
            var partitionCellManager = new PartitionCellManager(game);
            var ghostObjectManager = new GhostObjectManager();

            while (true)
            {
                var chunkName = reader.ReadAsciiString();
                if (chunkName == "SG_EOF")
                {
                    if (stream.Position != stream.Length)
                    {
                        throw new InvalidDataException();
                    }
                    break;
                }

                reader.BeginSegment();

                switch (chunkName)
                {
                    case "CHUNK_GameState":
                        gameState.Load(reader);
                        break;

                    case "CHUNK_Campaign":
                        campaignManager = new CampaignManager();
                        campaignManager.Load(reader);
                        break;

                    case "CHUNK_GameStateMap":
                        GameStateMap.Load(reader, game);
                        partitionCellManager.OnNewGame();
                        break;

                    case "CHUNK_TerrainLogic":
                        terrainLogic.Load(reader);
                        break;

                    case "CHUNK_TeamFactory":
                        game.Scene3D.TeamFactory.Load(reader);
                        break;

                    case "CHUNK_Players":
                        game.Scene3D.PlayerManager.Load(reader, game);
                        break;

                    case "CHUNK_GameLogic":
                        gameLogic = new GameLogic(game.Scene3D);
                        gameLogic.Load(reader);
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
                        gameClient = new GameClient(game.Scene3D, gameLogic);
                        gameClient.Load(reader);
                        break;

                    case "CHUNK_InGameUI":
                        game.AssetStore.InGameUI.Current.Load(reader);
                        break;

                    case "CHUNK_Partition":
                        partitionCellManager.Load(reader);
                        break;

                    case "CHUNK_TerrainVisual":
                        terrainVisual.Load(reader, game);
                        break;

                    case "CHUNK_GhostObject":
                        ghostObjectManager.Load(reader, gameLogic, game);
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
                    campaignManager.CampaignName,
                    campaignManager.MissionName);
            }
        }
    }

    public enum SaveGameType : uint
    {
        Skirmish,
        SinglePlayer
    }
}
