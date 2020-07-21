using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using OpenSage.Data.Map;
using OpenSage.Data.Rep;
using OpenSage.FileFormats;

namespace OpenSage.Data.Sav
{
    public sealed class SaveFile
    {
        public IReadOnlyList<SaveChunkHeader> ChunkHeaders { get; private set; }

        public static SaveFile FromStream(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.Unicode, true))
            {
                var chunkHeaders = new List<SaveChunkHeader>();
                var chunkHeader = SaveChunkHeader.Parse(reader);

                while (!chunkHeader.IsEof)
                {
                    chunkHeaders.Add(chunkHeader);

                    var end = stream.Position + chunkHeader.DataLength;

                    switch (chunkHeader.Name)
                    {
                        case "CHUNK_GameState":
                        {
                            var gameType = reader.ReadUInt32AsEnum<SaveGameType>();
                            var mapPath = reader.ReadBytePrefixedAsciiString();
                            var timeStamp = reader.ReadDateTime();
                            var displayName = reader.ReadBytePrefixedUnicodeString();
                            var mapFileName = reader.ReadBytePrefixedAsciiString();
                            var side = reader.ReadBytePrefixedAsciiString();
                            var missionIndex = reader.ReadUInt32();
                            break;
                        }

                        case "CHUNK_Campaign":
                        {
                            var side = reader.ReadBytePrefixedAsciiString();
                            var missionName = reader.ReadBytePrefixedAsciiString();
                            var unknown = reader.ReadUInt32();
                            var maybeDifficulty = reader.ReadUInt32();
                            if (chunkHeader.Version >= 5)
                            {
                                var unknown2 = reader.ReadBytes(5);
                            }
                            break;
                        }

                        case "CHUNK_GameStateMap":
                        {
                            var mapPath1 = reader.ReadBytePrefixedAsciiString();
                            var mapPath2 = reader.ReadBytePrefixedAsciiString();
                            var unknown = reader.ReadUInt32();
                            if (unknown != 0u && unknown != 2u)
                            {
                                throw new InvalidDataException();
                            }

                            var mapFileSize = reader.ReadUInt32();
                            var mapEnd = stream.Position + mapFileSize;
                            var map = MapFile.FromStream(stream);

                            // This seems to be aligned, so it's sometimes more than what we just read.
                            stream.Seek(mapEnd, SeekOrigin.Begin);

                            var unknown2 = reader.ReadUInt32(); // 586
                            var unknown3 = reader.ReadUInt32(); // 3220

                            if (unknown == 2u)
                            {
                                var unknown4 = reader.ReadUInt32(); // 2
                                var unknown5 = reader.ReadUInt32(); // 25600 (160^2)
                                var unknown6 = reader.ReadBooleanChecked();
                                var unknown7 = reader.ReadBooleanChecked();
                                var unknown8 = reader.ReadBooleanChecked();
                                var unknown9 = reader.ReadBooleanChecked();
                                var unknown10 = reader.ReadUInt32(); // 0

                                var numPlayers = reader.ReadUInt32(); // 8
                                var unknown11 = reader.ReadUInt32(); // 5

                                var players = new GameStateMapPlayer[numPlayers];
                                for (var i = 0; i < numPlayers; i++)
                                {
                                    players[i] = GameStateMapPlayer.Parse(reader);
                                }

                                var mapPath3 = reader.ReadBytePrefixedAsciiString();
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
                        {
                            stream.Seek(chunkHeader.DataLength, SeekOrigin.Current);
                            break;

                            var unknown = reader.ReadUInt32(); // 46
                            var unknown2 = reader.ReadUInt16(); // 71
                            var unknown3 = reader.ReadUInt32(); // 1
                            var unknown4 = reader.ReadUInt32(); // 2
                            var unknown5 = reader.ReadBytes(19);

                            while (true)
                            {
                                var num = reader.ReadUInt16();
                                var unknownNumbers = new uint[num];
                                for (var i = 0; i < num; i++)
                                {
                                    unknownNumbers[i] = reader.ReadUInt32();
                                }

                                reader.ReadBytes(155);
                            }

                            stream.Seek(chunkHeader.DataLength, SeekOrigin.Current);
                            break;
                        }

                        case "CHUNK_Players":
                        {
                            //var numPlayers = reader.ReadUInt32();
                            //var unknownBytes = reader.ReadBytes(47);

                            //var players = new PlayerState[numPlayers];
                            //for (var i = 0; i < numPlayers; i++)
                            //{
                            //    players[i] = PlayerState.Parse(reader);
                            //}

                            stream.Seek(chunkHeader.DataLength, SeekOrigin.Current);
                            break;
                        }

                        case "CHUNK_GameLogic":
                            {
                                var unknown1 = reader.ReadUInt32();
                                var unknown2 = reader.ReadByte();
                                var numGameObjects = reader.ReadUInt32();
                                var gameObjects = new List<GameObjectState>();
                                for (var i = 0; i < numGameObjects; i++)
                                {
                                    gameObjects.Add(new GameObjectState
                                    {
                                        Name = reader.ReadBytePrefixedAsciiString(),
                                        Id = reader.ReadUInt16()
                                    });
                                }

                                reader.ReadByte(); // 5

                                for (var objectIndex = 0; objectIndex < numGameObjects; objectIndex++)
                                {
                                    reader.ReadBooleanChecked(); // 0
                                    reader.ReadBooleanChecked(); // 1
                                    reader.ReadBooleanChecked(); // 1

                                    var objectID = reader.ReadUInt16();

                                    var unknown3 = reader.ReadUInt32();

                                    reader.ReadByte(); // 7

                                    reader.ReadUInt16(); // Inverse of object ID??

                                    reader.ReadBooleanChecked(); // 0
                                    reader.ReadBooleanChecked(); // 0
                                    reader.ReadBooleanChecked(); // 1

                                    var transform = reader.ReadMatrix4x3Transposed();

                                    var unknown8 = reader.ReadBytes(29);
                                    var unknown9 = reader.ReadSingle(); // 14
                                    var unknown10 = reader.ReadSingle(); // 12
                                    var unknown11 = reader.ReadSingle(); // 11
                                    var unknown12 = reader.ReadSingle(); // 12
                                    var unknown13 = reader.ReadSingle(); // 12
                                    var unknown14 = reader.ReadByte();
                                    var position = reader.ReadVector3();
                                    var unknown = reader.ReadSingle(); // 360
                                    var unknown15 = reader.ReadBytes(29);
                                    var unknown16 = reader.ReadSingle(); // 360
                                    var unknown17 = reader.ReadSingle(); // 360
                                    var unknown18 = reader.ReadBytes(86);
                                    var team = reader.ReadBytePrefixedAsciiString(); // teamPlyrAmerica

                                    reader.ReadBytes(54);

                                    // Modules
                                    var numModules = reader.ReadUInt16();
                                    for (var i = 0; i < numModules; i++)
                                    {
                                        var moduleTag = reader.ReadBytePrefixedAsciiString();
                                        var moduleLengthInBytes = reader.ReadUInt32();

                                        // TODO: Per-module parsing
                                        //if (i == 5) // Body module, but only for first object
                                        //{
                                        //    for (var j = 0; j < 6; j++)
                                        //    {
                                        //        reader.ReadBooleanChecked();
                                        //    }
                                        //    reader.ReadSingle(); // 1.0f
                                        //    var maxHealth = reader.ReadSingle(); // 1000.0f
                                        //    var currentHealth = reader.ReadSingle(); // 1000.0f (or this is maxHealth)
                                        //    reader.ReadSingle(); // 1000.0f
                                        //    reader.ReadSingle(); // 1000.0f
                                        //    reader.ReadBytes(65);
                                        //}
                                        //else
                                        {
                                            reader.ReadBytes((int) moduleLengthInBytes);
                                        }
                                    }

                                    reader.ReadBytes(21);

                                    var objectName = reader.ReadBytePrefixedAsciiString();

                                    reader.ReadBytes(5);

                                    // 5 possible weapons...
                                    for (var i = 0; i < 5; i++)
                                    {
                                        var slotFilled = reader.ReadBooleanChecked(); // 1
                                        if (slotFilled)
                                        {
                                            reader.ReadByte(); // 3
                                            var weaponTemplateName = reader.ReadBytePrefixedAsciiString();
                                            var weaponSlot = reader.ReadByteAsEnum<OpenSage.Logic.Object.WeaponSlot>();
                                            reader.ReadBytes(55);
                                        }
                                    }

                                    reader.ReadBytes(21);

                                    var numSpecialPowers = reader.ReadUInt32();

                                    for (var i = 0; i < numSpecialPowers; i++)
                                    {
                                        var specialPower = reader.ReadBytePrefixedAsciiString();
                                    }
                                }

                                reader.ReadBytes(121);

                                break;
                            }

                        case "CHUNK_ParticleSystem":
                        {
                            var unknown = reader.ReadUInt32();
                            var count = reader.ReadUInt32();
                            for (var i = 0; i < count; i++)
                            {
                                var name = reader.ReadBytePrefixedAsciiString();
                                if (name != string.Empty)
                                {
                                    var unknown2 = reader.ReadByte();

                                }
                            }

                            break;
                        }

                        case "CHUNK_Radar":
                            stream.Seek(chunkHeader.DataLength, SeekOrigin.Current);
                            break;

                        case "CHUNK_ScriptEngine":
                            stream.Seek(chunkHeader.DataLength, SeekOrigin.Current);
                            break;

                        case "CHUNK_SidesList":
                            stream.Seek(chunkHeader.DataLength, SeekOrigin.Current);
                            break;

                        case "CHUNK_TacticalView":
                            {
                                var unknown1 = reader.ReadUInt32();
                                if (unknown1 != 0)
                                {
                                    throw new InvalidDataException();
                                }

                                var position = reader.ReadVector2();

                                var unknown2 = reader.ReadUInt32();
                                if (unknown2 != 0)
                                {
                                    throw new InvalidDataException();
                                }

                                break;
                            }

                        case "CHUNK_GameClient":
                            {
                                var unknown = reader.ReadBytes(5);

                                var numObjects = reader.ReadUInt32();

                                for (var i = 0; i < numObjects; i++)
                                {
                                    var objectName = reader.ReadBytePrefixedAsciiString();
                                    var objectID = reader.ReadUInt16();
                                }

                                reader.ReadByte(); // 5

                                for (var i = 0; i < numObjects; i++)
                                {
                                    reader.ReadBytes(17);

                                    var numModelConditionFlags = reader.ReadUInt32();

                                    for (var j = 0; j < numModelConditionFlags; j++)
                                    {
                                        var modelConditionFlag = reader.ReadBytePrefixedAsciiString();
                                    }

                                    reader.ReadByte();

                                    var transform = reader.ReadMatrix4x3Transposed();

                                    reader.ReadBytes(62);

                                    var numModules = reader.ReadUInt16();
                                    for (var moduleIndex = 0; moduleIndex < numModules; moduleIndex++)
                                    {
                                        var moduleTag = reader.ReadBytePrefixedAsciiString();
                                        var moduleLengthInBytes = reader.ReadUInt32();
                                        reader.ReadBytes((int) moduleLengthInBytes);
                                    }

                                    reader.ReadBytes(82);
                                }

                                //stream.Seek(chunkHeader.DataLength, SeekOrigin.Current);
                                break;
                            }

                        case "CHUNK_InGameUI":
                            stream.Seek(chunkHeader.DataLength, SeekOrigin.Current);
                            break;

                        case "CHUNK_Partition":
                            stream.Seek(chunkHeader.DataLength, SeekOrigin.Current);
                            break;

                        case "CHUNK_TerrainVisual":
                            stream.Seek(chunkHeader.DataLength, SeekOrigin.Current);
                            break;

                        case "CHUNK_GhostObject":
                            stream.Seek(chunkHeader.DataLength, SeekOrigin.Current);
                            break;

                        default:
                            throw new InvalidDataException($"Unknown chunk type '{chunkHeader.Name}'.");
                    }

                    if (stream.Position != end)
                    {
                        throw new InvalidDataException($"Error parsing chunk '{chunkHeader.Name}'. Expected stream to be at position {end} but was at {stream.Position}.");
                    }

                    chunkHeader = SaveChunkHeader.Parse(reader);
                }

                if (stream.Position != stream.Length)
                {
                    throw new InvalidDataException();
                }

                var chunkHeaderNames = string.Join(Environment.NewLine, chunkHeaders.Select(x => x.Name));

                return new SaveFile
                {
                    ChunkHeaders = chunkHeaders
                };
            }
        }

        private enum SaveGameType : uint
        {
            Skirmish,
            SinglePlayer
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

            internal static GameStateMapPlayer Parse(BinaryReader reader)
            {
                var result = new GameStateMapPlayer
                {
                    Name = reader.ReadBytePrefixedUnicodeString(),
                    Unknown1 = reader.ReadUInt16(),
                    Color = reader.ReadInt32AsEnum<ReplaySlotColor>(),
                    StartPosition = reader.ReadInt32(),
                    PlayerTemplateIndex = reader.ReadInt32(),
                    Team = reader.ReadInt32(),
                    ColorChosen = reader.ReadInt32AsEnum<ReplaySlotColor>(),
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

        private sealed class PlayerState
        {
            public uint UnknownInt1 { get; private set; }
            public bool UnknownBool1 { get; private set; }
            public bool UnknownBool2 { get; private set; }
            public bool UnknownBool3 { get; private set; }
            public string ScienceRank { get; private set; }
            public uint Unknown1 { get; private set; }
            public uint Unknown2 { get; private set; }
            public uint Unknown3 { get; private set; }
            public uint Unknown4 { get; private set; }
            public uint Unknown5 { get; private set; }
            public string Name { get; private set; }
            public bool UnknownBool4 { get; private set; }
            public byte MaybePlayerId { get; private set; }
            public byte[] UnknownBytes { get; private set; }

            internal static PlayerState Parse(BinaryReader reader)
            {
                var result = new PlayerState
                {
                    UnknownInt1 = reader.ReadUInt32(),          // 3, 4, 1, 6, 8, 9, 10, 2,
                    UnknownBool1 = reader.ReadBooleanChecked(), // 1, 1, 1, 1, 1, 1,  1, 1,
                    UnknownBool2 = reader.ReadBooleanChecked(), // 1, 1, 2, 3, 3, 3,  3, 1,
                    UnknownBool3 = reader.ReadBooleanChecked(), // 0, 0, 0, 0, 0, 0,  0, 0,
                    ScienceRank = reader.ReadBytePrefixedAsciiString(),
                    Unknown1 = reader.ReadUInt32(), // 1
                    Unknown2 = reader.ReadUInt32(), // 2
                    Unknown3 = reader.ReadUInt32(), // 1
                    Unknown4 = reader.ReadUInt32(), // 800
                    Unknown5 = reader.ReadUInt32(), // 0
                    Name = reader.ReadBytePrefixedUnicodeString(),
                    UnknownBool4 = reader.ReadBooleanChecked(),
                    MaybePlayerId = reader.ReadByte(),
                    //UnknownBytes = reader.ReadBytes(588)
                };

                var numBuildListItems = reader.ReadUInt16();
                var buildListItems = new BuildListItem[numBuildListItems];
                for (var i = 0; i < numBuildListItems; i++)
                {
                    buildListItems[i] = new BuildListItem();
                    buildListItems[i].ReadFromSaveFile(reader);
                }

                return result;
            }
        }
    }

    public sealed class SaveChunkHeader
    {
        public string Name { get; private set; }
        public bool IsEof { get; private set; }
        public uint Length { get; private set; }
        public uint DataLength => Length - 1; // Excluding "Version" byte
        public byte Version { get; private set; }

        internal static SaveChunkHeader Parse(BinaryReader reader)
        {
            var name = reader.ReadBytePrefixedAsciiString();

            if (name == "SG_EOF")
            {
                return new SaveChunkHeader
                {
                    Name = name,
                    IsEof = true
                };
            }

            return new SaveChunkHeader
            {
                Name = name,
                Length = reader.ReadUInt32(),
                Version = reader.ReadByte()
            };
        }
    }
}
