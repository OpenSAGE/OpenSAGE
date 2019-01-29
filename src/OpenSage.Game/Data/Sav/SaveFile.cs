using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using OpenSage.Data.Map;
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

                while (true)
                {
                    var chunkHeader = SaveChunkHeader.Parse(reader);
                    chunkHeaders.Add(chunkHeader);

                    if (chunkHeader.IsEof)
                    {
                        if (stream.Position != stream.Length)
                        {
                            throw new InvalidDataException();
                        }
                        break;
                    }

                    var end = stream.Position + chunkHeader.DataLength;

                    switch (chunkHeader.Name)
                    {
                        case "CHUNK_GameState":
                        {
                            var unknown1 = reader.ReadUInt32();
                            if (unknown1 != 0u)
                            {
                                throw new InvalidDataException();
                            }
                            var mapPath = reader.ReadBytePrefixedAsciiString();
                            var year = reader.ReadUInt16();
                            var month = reader.ReadUInt16();
                            var day = reader.ReadUInt16();
                            var dayOfWeek = reader.ReadUInt16AsEnum<DayOfWeek>();
                            var hour = reader.ReadUInt16();
                            var minute = reader.ReadUInt16();
                            var second = reader.ReadUInt16();
                            var millisecond = reader.ReadUInt16();
                            var timeStamp = new DateTime(year, month, day, hour, minute, second, millisecond);
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
                            var unknownMapPath = reader.ReadBytePrefixedAsciiString();
                            var mapPath = reader.ReadBytePrefixedAsciiString();
                            var unknown = reader.ReadUInt32();
                            if (unknown != 0 && unknown != 2)
                            {
                                throw new InvalidDataException();
                            }
                            var unknownLength = reader.ReadUInt32();
                            var map = MapFile.FromStream(stream);

                            var unknown2 = reader.ReadByte();
                            var unknown3 = reader.ReadUInt32();
                            var unknown4 = reader.ReadUInt32();

                            for (var i = 0; i < 8; i++)
                            {
                                var playerName = reader.ReadBytePrefixedUnicodeString();
                                var unknown5 = reader.ReadUInt16();
                                var unknown6 = reader.ReadUInt32();
                                var unknown7 = reader.ReadUInt32();
                                var unknown8 = reader.ReadUInt32();
                                var unknown9 = reader.ReadUInt32();
                                var unknown10 = reader.ReadUInt32();
                                var unknown11 = reader.ReadUInt32();
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
                            var unknown = reader.ReadUInt32(); // 16

                            stream.Seek(chunkHeader.DataLength, SeekOrigin.Current);
                            break;
                        }

                        case "CHUNK_Players":
                        {
                            var numPlayers = reader.ReadUInt32();
                            var unknownBytes = reader.ReadBytes(47);

                            for (var i = 0; i < numPlayers; i++)
                            {
                                var unknownInt1 = reader.ReadUInt32();
                                var unknownBool1 = reader.ReadBooleanChecked();
                                var unknownBool2 = reader.ReadBooleanChecked();
                                var unknownBool3 = reader.ReadBooleanChecked();
                                var scienceRank = reader.ReadBytePrefixedAsciiString();
                                var unknown1 = reader.ReadUInt32(); // 1
                                var unknown2 = reader.ReadUInt32(); // 2
                                var unknown3 = reader.ReadUInt32(); // 1
                                var unknown4 = reader.ReadUInt32(); // 800
                                var unknown5 = reader.ReadUInt32(); // 0
                                var name = reader.ReadBytePrefixedUnicodeString();
                                var unknownBool = reader.ReadBooleanChecked();
                                var maybePlayerId = reader.ReadByte();
                                var unknownBytes2 = reader.ReadBytes(588);
                            }

                            // Build list
                            var numBuildListItems = reader.ReadUInt16();
                            for (var i = 0; i < numBuildListItems; i++)
                            {
                                var version = reader.ReadByte();
                                var buildingName = reader.ReadBytePrefixedAsciiString();
                                var name = reader.ReadBytePrefixedAsciiString();
                                var position = reader.ReadVector3();
                                var unknown = reader.ReadBytes(8);
                                var angle = reader.ReadSingle();
                                var unknown2 = reader.ReadBytes(6);
                                var health = reader.ReadUInt32();
                                var unknownBool1 = reader.ReadBooleanChecked();
                                var unknownBool2 = reader.ReadBooleanChecked();
                                var unknownBool3 = reader.ReadBooleanChecked();
                                var unknownBool4 = reader.ReadBooleanChecked();
                                var unknown3 = reader.ReadBytes(59);
                            }

                            stream.Seek(chunkHeader.DataLength, SeekOrigin.Current);
                            break;
                        }

                        case "CHUNK_GameLogic":
                        {
                            var start = stream.Position;
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

                            var end2 = stream.Position;
                            var diff = end2 - start;
                            var unknown3 = reader.ReadUInt32();
                            var unknown4 = reader.ReadUInt16();
                            var unknown5 = reader.ReadUInt32();
                            // TODO
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
                        case "CHUNK_ScriptEngine":
                        case "CHUNK_SidesList":
                        case "CHUNK_TacticalView":
                        case "CHUNK_GameClient":
                        case "CHUNK_InGameUI":
                        case "CHUNK_Partition":
                        
                        case "CHUNK_TerrainVisual":
                        case "CHUNK_GhostObject":
                        case "SG_EOF":
                        default:
                            stream.Seek(chunkHeader.DataLength, SeekOrigin.Current);
                            break;
                    }

                    if (stream.Position != end)
                    {
                        throw new InvalidDataException($"Expected stream to be at position {end} but was at {stream.Position}.");
                    }
                }

                var chunkHeaderNames = string.Join(Environment.NewLine, chunkHeaders.Select(x => x.Name));

                return new SaveFile
                {
                    ChunkHeaders = chunkHeaders
                };
            }
        }

        private sealed class GameObjectState
        {
            public string Name;
            public ushort Id;
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
