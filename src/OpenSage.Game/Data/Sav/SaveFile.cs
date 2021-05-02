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
            using (var reader = new BinaryReader(stream, Encoding.Unicode, true))
            {
                var chunkHeaders = new List<SaveChunkHeader>();
                var chunkHeader = SaveChunkHeader.Parse(reader);

                while (!chunkHeader.IsEof)
                {
                    chunkHeaders.Add(chunkHeader);

                    if (chunkHeader.Name == "CHUNK_GameState")
                    {
                        return GameState.Parse(reader);
                    }
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
            using (var reader = new BinaryReader(stream, Encoding.Unicode, true))
            {
                var chunkHeaders = new List<SaveChunkHeader>();
                var chunkHeader = SaveChunkHeader.Parse(reader);

                GameState gameState = null;
                MapFile map = null;
                GameLogic gameLogic = null;
                GameClient gameClient = null;

                while (!chunkHeader.IsEof)
                {
                    chunkHeaders.Add(chunkHeader);

                    var end = stream.Position + chunkHeader.Length;

                    switch (chunkHeader.Name)
                    {
                        case "CHUNK_GameState":
                            {
                                gameState = GameState.Parse(reader);
                                // TODO: Start game after parsing players.
                                break;
                            }

                        case "CHUNK_Campaign":
                            {
                                var version = reader.ReadByte();
                                var side = reader.ReadBytePrefixedAsciiString();
                                var missionName = reader.ReadBytePrefixedAsciiString();
                                var unknown = reader.ReadUInt32();
                                var maybeDifficulty = reader.ReadUInt32();

                                if (version >= 5)
                                {
                                    var unknown2 = reader.ReadBytes(5);
                                }

                                break;
                            }

                        case "CHUNK_GameStateMap":
                            {
                                var version = reader.ReadByte();
                                var mapPath1 = reader.ReadBytePrefixedAsciiString();
                                var mapPath2 = reader.ReadBytePrefixedAsciiString();
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
                                    isMultiPlayer: true,
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
                            game.Scene3D.TeamFactory.Load(new SaveFileReader(reader));
                            break;

                        case "CHUNK_Players":
                            {
                                var version = reader.ReadByte();
                                //var bytes = reader.ReadBytes((int)chunkHeader.DataLength);
                                //File.WriteAllBytes($"CHUNK_Players_{gameState.Timestamp.Ticks}", bytes);

                                var numPlayers = reader.ReadUInt32();
                                var players = new Logic.Player[numPlayers];
                                for (var i = 0; i < numPlayers; i++)
                                {
                                    players[i] = new Logic.Player(null, new ColorRgb(), game.AssetStore.Ranks);
                                    players[i].Load(reader, game.AssetStore);
                                }
                                break;
                            }

                        case "CHUNK_GameLogic":
                            gameLogic = new GameLogic(game.Scene3D);
                            gameLogic.Load(new SaveFileReader(reader));
                            break;

                        case "CHUNK_ParticleSystem":
                            {
                                var version = reader.ReadByte();
                                var unknown = reader.ReadUInt32();
                                var count = reader.ReadUInt32();
                                for (var i = 0; i < count; i++)
                                {
                                    var name = reader.ReadBytePrefixedAsciiString();
                                    if (name != string.Empty)
                                    {
                                        reader.ReadBytes(11);
                                        var texture = reader.ReadBytePrefixedAsciiString();
                                        var angleX = reader.ReadRandomVariable();
                                        var angleY = reader.ReadRandomVariable();
                                        var angleZ = reader.ReadRandomVariable();
                                        var unknown1 = reader.ReadRandomVariable(); // Maybe AngularRateX, Y, Z, if same order as ini files
                                        var unknown2 = reader.ReadRandomVariable();
                                        var unknown3 = reader.ReadRandomVariable();
                                        var angularDamping = reader.ReadRandomVariable();
                                        var velocityDamping = reader.ReadRandomVariable();
                                        var lifetime = reader.ReadRandomVariable();
                                        var unknown4 = reader.ReadUInt32();
                                        var size = reader.ReadRandomVariable();
                                        var unknown5 = reader.ReadRandomVariable(); // Maybe StartSizeRate, if same order as ini files
                                        var sizeRate = reader.ReadRandomVariable();
                                        var sizeRateDamping = reader.ReadRandomVariable();
                                        for (var j = 0; j < 8; j++)
                                        {
                                            var alphaKeyframe = RandomAlphaKeyframe.ReadFromSaveFile(reader);
                                        }
                                        for (var j = 0; j < 8; j++)
                                        {
                                            var colorKeyframe = RgbColorKeyframe.ReadFromSaveFile(reader);
                                        }
                                        var unknown6 = reader.ReadRandomVariable(); // Maybe ColorScale, if same order as ini files, but value doesn't match ini file
                                        var burstDelay = reader.ReadRandomVariable();
                                        var burstCount = reader.ReadRandomVariable();
                                        var unknown7 = reader.ReadRandomVariable(); // Maybe InitialDelay, if same order as ini files
                                        var unknown8 = reader.ReadVector3(); // Maybe DriftVelocity, if same order as ini files
                                        var gravity = reader.ReadSingle();
                                        var unknown9 = reader.ReadBytes(14);
                                        var velocityType = reader.ReadUInt32AsEnum<ParticleVelocityType>();
                                        var unknown10 = reader.ReadUInt32();
                                        switch (velocityType)
                                        {
                                            case ParticleVelocityType.Ortho:
                                                var velocityOrthoX = reader.ReadRandomVariable();
                                                var velocityOrthoY = reader.ReadRandomVariable();
                                                var velocityOrthoZ = reader.ReadRandomVariable();
                                                break;
                                            case ParticleVelocityType.Spherical:
                                                var velocitySpherical = reader.ReadRandomVariable();
                                                break;
                                            case ParticleVelocityType.Hemispherical:
                                                var velocityHemispherical = reader.ReadRandomVariable();
                                                break;
                                            case ParticleVelocityType.Cylindrical:
                                                var velocityCylindricalRadial = reader.ReadRandomVariable();
                                                var velocityCylindricalNormal = reader.ReadRandomVariable();
                                                break;
                                            case ParticleVelocityType.Outward:
                                                var velocityOutward = reader.ReadRandomVariable();
                                                var velocityOutwardOther = reader.ReadRandomVariable();
                                                break;
                                            default:
                                                throw new NotImplementedException();
                                        }
                                        var volumeType = reader.ReadUInt32AsEnum<ParticleVolumeType>();
                                        switch (volumeType)
                                        {
                                            case ParticleVolumeType.Point:
                                                break;
                                            case ParticleVolumeType.Line:
                                                var lineStartPoint = reader.ReadVector3();
                                                var lineEndPoint = reader.ReadVector3();
                                                break;
                                            case ParticleVolumeType.Box:
                                                var halfSize = reader.ReadVector3();
                                                break;
                                            case ParticleVolumeType.Sphere:
                                                var volumeSphereRadius = reader.ReadSingle(); // Interesting, value doesn't match ini file
                                                break;
                                            case ParticleVolumeType.Cylinder:
                                                var volumeCylinderRadius = reader.ReadSingle();
                                                var volumeCylinderLength = reader.ReadSingle();
                                                break;
                                            default:
                                                throw new NotImplementedException();
                                        }
                                        var unknown11 = reader.ReadUInt32();
                                        var windMotion = reader.ReadUInt32AsEnum<ParticleSystemWindMotion>();
                                        var unknown12 = reader.ReadSingle();
                                        var unknown13 = reader.ReadSingle(); // Almost same as WindAngleChangeMin
                                        var windAngleChangeMin = reader.ReadSingle();
                                        var windAngleChangeMax = reader.ReadSingle();
                                        var unknown14 = reader.ReadSingle();
                                        var windPingPongStartAngleMin = reader.ReadSingle();
                                        var windPingPongStartAngleMax = reader.ReadSingle();
                                        var unknown15 = reader.ReadSingle();
                                        var windPingPongEndAngleMin = reader.ReadSingle();
                                        var windPingPongEndAngleMax = reader.ReadSingle();
                                        var unknown16 = reader.ReadBooleanChecked();
                                        var unknown17 = reader.ReadUInt32();
                                        var unknown18 = reader.ReadBytes(9);
                                        var transform = reader.ReadMatrix4x3Transposed();
                                        var unknown19 = reader.ReadBooleanChecked();
                                        var transform2 = reader.ReadMatrix4x3Transposed();
                                        var unknown20 = reader.ReadUInt32(); // Maybe _nextBurst
                                        var unknown21 = reader.ReadUInt32();
                                        var unknown22 = reader.ReadUInt32();
                                        var unknown23 = reader.ReadUInt32();
                                        var unknown24 = reader.ReadUInt32();
                                        reader.ReadBytes(6);
                                        for (var j = 0; j < 6; j++)
                                        {
                                            var unknown25 = reader.ReadSingle(); // All 1
                                        }
                                        reader.ReadBytes(33);
                                        var numParticles = reader.ReadUInt32();
                                        for (var j = 0; j < numParticles; j++)
                                        {
                                            var unknown26 = reader.ReadBooleanChecked();
                                            var unknown27 = reader.ReadBooleanChecked();
                                            var unknown28 = reader.ReadVector3();
                                            var particlePosition = reader.ReadVector3();
                                            var anotherPosition = reader.ReadVector3();
                                            var particleVelocityDamping = reader.ReadSingle();
                                            var unknown29 = reader.ReadSingle(); // 0
                                            var unknown30 = reader.ReadSingle(); // 0
                                            var unknown31 = reader.ReadSingle(); // 3.78, maybe AngleZ
                                            var unknown32 = reader.ReadSingle(); // 0
                                            var unknown33 = reader.ReadSingle(); // 0
                                            var unknown34 = reader.ReadSingle(); // 0
                                            var unknown34_ = reader.ReadSingle();
                                            var unknown35 = reader.ReadSingle(); // 17.8
                                            var unknown36 = reader.ReadSingle(); // 0.04
                                            var particleSizeRateDamping = reader.ReadSingle();
                                            for (var k = 0; k < 8; k++)
                                            {
                                                var alphaKeyframeAlpha = reader.ReadSingle();
                                                var alphaKeyframeTime = reader.ReadUInt32();
                                                var alphaKeyframe = new ParticleAlphaKeyframe(
                                                    alphaKeyframeTime,
                                                    alphaKeyframeAlpha);
                                            }
                                            for (var k = 0; k < 8; k++)
                                            {
                                                var colorKeyframeColor = reader.ReadColorRgbF();
                                                var colorKeyframeTime = reader.ReadUInt32();
                                                var colorKeyframe = new ParticleColorKeyframe(
                                                    colorKeyframeTime,
                                                    colorKeyframeColor.ToVector3());
                                            }
                                            var unknown37 = reader.ReadSingle();
                                            var unknown38 = reader.ReadBooleanChecked();
                                            var unknown39 = reader.ReadSingle();
                                            reader.ReadBytes(28); // All 0
                                            var unknown40 = reader.ReadUInt32(); // 49
                                            var unknown41 = reader.ReadUInt32(); // 1176
                                            var particleAlpha = reader.ReadSingle(); // 1.0
                                            var unknown42 = reader.ReadUInt32(); // 0
                                            var unknown43 = reader.ReadUInt32(); // 1
                                            var unknown44 = reader.ReadVector3(); // (0.35, 0.35, 0.35)
                                            reader.ReadBytes(12); // All 0
                                            var unknown45 = reader.ReadUInt32(); // 1
                                            reader.ReadBytes(8); // All 0
                                        }
                                    }
                                }

                                break;
                            }

                        case "CHUNK_Radar":
                            {
                                var version = reader.ReadByte();
                                game.Scene3D.Radar.Load(reader);
                                break;
                            }

                        case "CHUNK_ScriptEngine":
                            {
                                var version = reader.ReadByte();
                                game.Scripting.Load(reader);
                                break;
                            }

                        case "CHUNK_SidesList":
                            {
                                var version = reader.ReadByte();
                                game.Scene3D.Terrain.Map.SidesList.Load(reader);
                                break;
                            }

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
                            gameClient.Load(new SaveFileReader(reader));
                            break;

                        case "CHUNK_InGameUI":
                            {
                                var version = reader.ReadByte();
                                reader.ReadUInt32(); // 0
                                reader.ReadBooleanChecked();
                                reader.ReadBooleanChecked();
                                reader.ReadBooleanChecked();
                                reader.ReadUInt32(); // 0
                                var something = reader.ReadUInt32();
                                while (something != uint.MaxValue) // A way to store things the engine doesn't know the length of?
                                {
                                    var someString1 = reader.ReadBytePrefixedAsciiString();
                                    var someString2 = reader.ReadBytePrefixedAsciiString();
                                    var unknown1 = reader.ReadUInt32();
                                    var unknown2 = reader.ReadUInt32(); // 0xFFFFFFFF
                                    reader.ReadBooleanChecked();
                                    reader.ReadBooleanChecked();
                                    reader.ReadBooleanChecked();

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
                                    reader.ReadBytes(65);
                                }
                                var someOtherCount = reader.ReadUInt32();
                                for (var i = 0; i < someOtherCount; i++)
                                {
                                    reader.ReadBooleanChecked();
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
                                reader.ReadBytes(6);
                                for (var i = 0; i < map.HeightMapData.Area; i++)
                                {
                                    var unknown = reader.ReadByte();
                                }
                                break;
                            }

                        case "CHUNK_GhostObject":
                            {
                                var version = reader.ReadByte();
                                reader.ReadBooleanChecked();
                                reader.ReadUInt32();
                                var count = reader.ReadUInt16();
                                for (var i = 0; i < count; i++)
                                {
                                    var someId = reader.ReadUInt32();
                                    reader.ReadBooleanChecked(); // 1
                                    reader.ReadBooleanChecked(); // 1
                                    var someId2 = reader.ReadUInt32(); // Same as someId
                                    reader.ReadUInt32();
                                    reader.ReadByte();
                                    reader.ReadSingle();
                                    reader.ReadSingle();
                                    reader.ReadSingle();
                                    reader.ReadSingle();
                                    reader.ReadSingle();
                                    reader.ReadSingle();
                                    reader.ReadBytes(14);
                                    var otherCount = reader.ReadByte();
                                    for (var j = 0; j < otherCount; j++)
                                    {
                                        var modelName = reader.ReadBytePrefixedAsciiString();
                                        var someFloat = reader.ReadSingle();
                                        var someInt = reader.ReadUInt32();
                                        var someBool = reader.ReadBooleanChecked();
                                        var modelTransform = reader.ReadMatrix4x3Transposed();
                                        var numMeshes = reader.ReadUInt32();
                                        for (var k = 0; k < numMeshes; k++)
                                        {
                                            var meshName = reader.ReadBytePrefixedAsciiString();
                                            var meshBool = reader.ReadBooleanChecked();
                                            var meshTransform = reader.ReadMatrix4x3Transposed();
                                        }
                                    }
                                    reader.ReadBooleanChecked();
                                    reader.ReadUInt32();
                                    reader.ReadUInt32();
                                    reader.ReadUInt32();
                                    var unknown = reader.ReadBooleanChecked();
                                    if (unknown)
                                    {
                                        reader.ReadByte();
                                        reader.ReadUInt32();
                                    }
                                }
                                break;
                            }

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
    }

    public sealed class SaveChunkHeader
    {
        public string Name { get; private set; }
        public bool IsEof { get; private set; }
        public uint Length { get; private set; }

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
                Length = reader.ReadUInt32()
            };
        }
    }

    public enum SaveGameType : uint
    {
        Skirmish,
        SinglePlayer
    }
}
