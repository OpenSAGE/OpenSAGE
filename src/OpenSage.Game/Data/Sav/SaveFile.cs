using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenSage.Client;
using OpenSage.Data.Map;
using OpenSage.Data.Rep;
using OpenSage.FileFormats;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Logic;
using OpenSage.Mathematics;
using OpenSage.Network;

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
                        return GameState.Parse(reader);
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

                GameState gameState = null;
                MapFile map = null;
                GameLogic gameLogic = null;
                GameClient gameClient = null;
                CampaignManager campaignManager = null;

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
                            gameState = GameState.Parse(reader);
                            break;

                        case "CHUNK_Campaign":
                            campaignManager = new CampaignManager();
                            campaignManager.Load(reader);
                            break;

                        case "CHUNK_GameStateMap":
                            {
                                var version = reader.ReadByte();
                                var mapPath1 = reader.ReadAsciiString();
                                var mapPath2 = reader.ReadAsciiString();
                                var unknown = reader.ReadUInt32();
                                if (unknown != 0u && unknown != 2u)
                                {
                                    throw new InvalidDataException();
                                }

                                var mapFileSize = reader.ReadUInt32();
                                var mapEnd = stream.Position + mapFileSize;
                                map = MapFile.FromStream(stream);

                                game.StartGame( // TODO: Do this after parsing players.
                                    mapPath2.Replace("userdata\\", ""),
                                    new EchoConnection(), // TODO
                                    new PlayerSetting?[]
                                    {
                                        new PlayerSetting(
                                            null,
                                            game.AssetStore.PlayerTemplates.GetByName("FactionAmerica"), // TODO
                                            new ColorRgb(0, 0, 255), 0)
                                    },
                                    localPlayerIndex: 0, // TODO
                                    isMultiPlayer: false, // TODO
                                    seed: Environment.TickCount, // TODO
                                    map); // TODO

                                // This seems to be aligned, so it's sometimes more than what we just read.
                                stream.Seek(mapEnd, SeekOrigin.Begin);

                                var unknown2 = reader.ReadUInt32(); // 586
                                var unknown3 = reader.ReadUInt32(); // 3220

                                if (unknown == 2u)
                                {
                                    var unknown4 = reader.ReadUInt32(); // 2
                                    var unknown5 = reader.ReadUInt32(); // 25600 (160^2)
                                    var unknown6 = reader.ReadBoolean();
                                    var unknown7 = reader.ReadBoolean();
                                    var unknown8 = reader.ReadBoolean();
                                    var unknown9 = reader.ReadBoolean();
                                    var unknown10 = reader.ReadUInt32(); // 0

                                    var numPlayers = reader.ReadUInt32(); // 8
                                    var unknown11 = reader.ReadUInt32(); // 5

                                    var players = new GameStateMapPlayer[numPlayers];
                                    for (var i = 0; i < numPlayers; i++)
                                    {
                                        players[i] = GameStateMapPlayer.Parse(reader);
                                    }

                                    var mapPath3 = reader.ReadAsciiString();
                                    var mapFileCrc = reader.ReadUInt32();
                                    var mapFileSize2 = reader.ReadUInt32();
                                    if (mapFileSize != mapFileSize2)
                                    {
                                        throw new InvalidDataException();
                                    }

                                    var unknown12 = reader.ReadUInt32();
                                    var unknown13 = reader.ReadUInt32();
                                }

                                break;
                            }

                        case "CHUNK_TerrainLogic":
                        {
                            var version = reader.ReadByte();
                            var unknown = reader.ReadInt32();
                            if (unknown != 2u)
                            {
                                throw new InvalidDataException();
                            }

                            var unknown2 = reader.ReadInt32();
                            if (unknown2 != 0u)
                            {
                                throw new InvalidDataException();
                            }

                            var unknown3 = reader.ReadByte();
                            if (unknown3 != 0)
                            {
                                throw new InvalidDataException();
                            }

                            break;
                        }

                        case "CHUNK_TeamFactory":
                            game.Scene3D.TeamFactory.Load(reader);
                            break;

                        case "CHUNK_Players":
                            game.Scene3D.PlayerManager.Load(reader);
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
                            {
                                var version = reader.ReadByte();
                                var partitionCellSize = reader.ReadSingle();
                                var count = reader.ReadUInt32();
                                for (var i = 0; i < count; i++)
                                {
                                    reader.__Skip(65);
                                }
                                var someOtherCount = reader.ReadUInt32();
                                for (var i = 0; i < someOtherCount; i++)
                                {
                                    reader.ReadBoolean();
                                    reader.ReadSingle();
                                    reader.ReadSingle();
                                    reader.ReadSingle();
                                    reader.ReadSingle();
                                    reader.ReadUInt16();
                                    reader.ReadUInt32();
                                }
                                break;
                            }

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

        private sealed class GameStateMapPlayer
        {
            public string Name { get; private set; }
            public ushort Unknown1 { get; private set; }
            public ReplaySlotColor Color { get; private set; }
            /// <summary>
            /// Start waypoint name is $"Player_{StartPosition + 1}_Start"
            /// </summary>
            public int StartPosition { get; private set; }
            /// <summary>
            /// Normally the same as <see cref="PlayerTemplateIndexChosen"/> except when
            /// "Random" is chosen.
            /// </summary>
            public int PlayerTemplateIndex { get; private set; }
            public int Team { get; private set; }
            public ReplaySlotColor ColorChosen { get; private set; }
            public int StartPositionChosen { get; private set; }
            public int PlayerTemplateIndexChosen { get; private set; }
            public uint Unknown2 { get; private set; }

            internal static GameStateMapPlayer Parse(SaveFileReader reader)
            {
                var result = new GameStateMapPlayer
                {
                    Name = reader.ReadUnicodeString(),
                    Unknown1 = reader.ReadUInt16(),
                    Color = reader.ReadEnum<ReplaySlotColor>(),
                    StartPosition = reader.ReadInt32(),
                    PlayerTemplateIndex = reader.ReadInt32(),
                    Team = reader.ReadInt32(),
                    ColorChosen = reader.ReadEnum<ReplaySlotColor>(),
                    StartPositionChosen = reader.ReadInt32(),
                    PlayerTemplateIndexChosen = reader.ReadInt32(),
                    Unknown2 = reader.ReadUInt32()
                };

                if (result.Unknown1 != 1u)
                {
                    throw new InvalidDataException();
                }

                //TODO: Check for result.Unknown2

                return result;
            }
        }
    }

    public enum SaveGameType : uint
    {
        Skirmish,
        SinglePlayer
    }
}
