using System.IO;
using System.Text;
using OpenSage.Client;
using OpenSage.Data.Map;
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
            using (var stream = entry.Open())
            {
                LoadFromStream(stream, game);
            }
        }

        public static void LoadFromStream(Stream stream, Game game)
        {
            using (var binaryReader = new BinaryReader(stream, Encoding.Unicode, true))
            {
                var reader = new SaveFileReader(binaryReader);

                var gameState = new GameState();
                MapFile map = null;
                GameLogic gameLogic = null;
                GameClient gameClient = null;
                CampaignManager campaignManager = null;
                var terrainLogic = new TerrainLogic();
                var partitionCellManager = new PartitionCellManager(game);

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
                            {
                                var version = reader.ReadByte();
                                var cameraAngle = reader.ReadSingle();
                                var cameraPosition = reader.ReadVector2();

                                var unknown2 = reader.ReadUInt32();
                                if (unknown2 != 0)
                                {
                                    throw new InvalidDataException();
                                }

                                break;
                            }

                        case "CHUNK_GameClient":
                            gameClient = new GameClient(game.Scene3D, gameLogic);
                            gameClient.Load(reader);
                            break;

                        case "CHUNK_InGameUI":
                            {
                                var version = reader.ReadByte();
                                reader.ReadUInt32(); // 0
                                reader.ReadBoolean();
                                reader.ReadBoolean();
                                reader.ReadBoolean();
                                reader.ReadUInt32(); // 0
                                var something = reader.ReadUInt32();
                                while (something != uint.MaxValue) // A way to store things the engine doesn't know the length of?
                                {
                                    var someString1 = reader.ReadAsciiString();
                                    var someString2 = reader.ReadAsciiString();
                                    var unknown1 = reader.ReadUInt32();
                                    var unknown2 = reader.ReadUInt32(); // 0xFFFFFFFF
                                    reader.ReadBoolean();
                                    reader.ReadBoolean();
                                    reader.ReadBoolean();

                                    something = reader.ReadUInt32();
                                }
                            }
                            break;

                        case "CHUNK_Partition":
                            partitionCellManager.Load(reader);
                            break;

                        case "CHUNK_TerrainVisual":
                            {
                                var version = reader.ReadByte();
                                reader.__Skip(6);
                                for (var i = 0; i < map.HeightMapData.Area; i++)
                                {
                                    var unknown = reader.ReadByte();
                                }
                                break;
                            }

                        case "CHUNK_GhostObject":
                            {
                                var version = reader.ReadByte();
                                reader.ReadBoolean();
                                reader.ReadUInt32();
                                var count = reader.ReadUInt16();
                                for (var i = 0; i < count; i++)
                                {
                                    var someId = reader.ReadUInt32();
                                    reader.ReadBoolean(); // 1
                                    reader.ReadBoolean(); // 1
                                    var someId2 = reader.ReadUInt32(); // Same as someId
                                    reader.ReadUInt32();
                                    reader.ReadByte();
                                    reader.ReadSingle();
                                    reader.ReadSingle();
                                    reader.ReadSingle();
                                    reader.ReadSingle();
                                    reader.ReadSingle();
                                    reader.ReadSingle();
                                    reader.__Skip(14);
                                    var otherCount = reader.ReadByte();
                                    for (var j = 0; j < otherCount; j++)
                                    {
                                        var modelName = reader.ReadAsciiString();
                                        var someFloat = reader.ReadSingle();
                                        var someInt = reader.ReadUInt32();
                                        var someBool = reader.ReadBoolean();
                                        var modelTransform = reader.ReadMatrix4x3Transposed();
                                        var numMeshes = reader.ReadUInt32();
                                        for (var k = 0; k < numMeshes; k++)
                                        {
                                            var meshName = reader.ReadAsciiString();
                                            var meshBool = reader.ReadBoolean();
                                            var meshTransform = reader.ReadMatrix4x3Transposed();
                                        }
                                    }
                                    reader.ReadBoolean();
                                    reader.ReadUInt32();
                                    reader.ReadUInt32();
                                    reader.ReadUInt32();
                                    var unknown = reader.ReadBoolean();
                                    if (unknown)
                                    {
                                        reader.ReadByte();
                                        reader.ReadUInt32();
                                    }
                                }
                                break;
                            }

                        default:
                            throw new InvalidDataException($"Unknown chunk type '{chunkName}'.");
                    }

                    reader.EndSegment();
                }
            }
        }

        private sealed class GameObjectState
        {
            public string Name;
            public ushort Id;
        }
    }

    public enum SaveGameType : uint
    {
        Skirmish,
        SinglePlayer
    }
}
