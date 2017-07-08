using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public sealed class MapObject : Asset
    {
        public MapVector3 Position { get; private set; }

        /// <summary>
        /// Angle of the object in radians.
        /// </summary>
        public float Angle { get; private set; }

        public string TypeName { get; private set; }

        public string Name { get; private set; }
        public uint InitialHealth { get; private set; }
        public bool Enabled { get; private set; }
        public bool Indestructible { get; private set; }
        public bool Unsellable { get; private set; }
        public bool Powered { get; private set; }
        public bool AiRecruitable { get; private set; }
        public bool Targetable { get; private set; }
        public string OriginalOwner { get; private set; }
        public string UniqueId { get; private set; }
        public string Layer { get; private set; }
        public ObjectWeather Weather { get; private set; } = ObjectWeather.UseMapWeather;
        public ObjectTime Time { get; private set; } = ObjectTime.UseMapTime;
        public uint? MaxHitPoints { get; private set; }
        public ObjectAggressiveness Aggressiveness { get; private set; } = ObjectAggressiveness.Normal;
        public ObjectVeterancy Veterancy { get; private set; } = ObjectVeterancy.Normal;
        public uint VisualRange { get; private set; }
        public uint ShroudClearingDistance { get; private set; }
        public float StoppingDistance { get; private set; }

        public string SoundAmbient { get; private set; }
        public bool SoundAmbientCustomized { get; private set; }
        public uint SoundAmbientLoopCount { get; private set; }
        public float SoundAmbientMinVolume { get; private set; }
        public float SoundAmbientVolume { get; private set; }
        public float SoundAmbientMinRange { get; private set; }
        public float SoundAmbientMaxRange { get; private set; }
        public ObjectAmbientSoundPriority SoundAmbientPriority { get; private set; } = ObjectAmbientSoundPriority.Normal;

        public static MapObject Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var position = MapVector3.Parse(reader);
                var angle = reader.ReadSingle();
                
                var unknown = reader.ReadUInt32(); // Road type?

                var typeName = reader.ReadUInt16PrefixedAsciiString();

                var result = new MapObject
                {
                    Position = position,
                    Angle = angle,
                    TypeName = typeName
                };

                ParseProperties(reader, context, propertyName =>
                {
                    switch (propertyName)
                    {
                        case "objectInitialHealth":
                            result.InitialHealth = reader.ReadUInt32();
                            break;

                        case "objectEnabled":
                            result.Enabled = reader.ReadBoolean();
                            break;

                        case "objectIndestructible":
                            result.Indestructible = reader.ReadBoolean();
                            break;

                        case "objectUnsellable":
                            result.Unsellable = reader.ReadBoolean();
                            break;

                        case "objectPowered":
                            result.Powered = reader.ReadBoolean();
                            break;

                        case "objectRecruitableAI":
                            result.AiRecruitable = reader.ReadBoolean();
                            break;

                        case "objectTargetable":
                            result.Targetable = reader.ReadBoolean();
                            break;

                        case "originalOwner":
                            result.OriginalOwner = reader.ReadUInt16PrefixedAsciiString();
                            break;

                        case "uniqueID":
                            result.UniqueId = reader.ReadUInt16PrefixedAsciiString();
                            break;

                        case "objectLayer":
                            result.Layer = reader.ReadUInt16PrefixedAsciiString();
                            break;

                        case "objectWeather":
                            result.Weather = reader.ReadUInt32AsEnum<ObjectWeather>();
                            break;

                        case "objectTime":
                            result.Time = reader.ReadUInt32AsEnum<ObjectTime>();
                            break;

                        case "objectName":
                            result.Name = reader.ReadUInt16PrefixedAsciiString();
                            break;

                        case "objectMaxHPs":
                            result.MaxHitPoints = reader.ReadUInt32();
                            break;

                        case "objectAggressiveness":
                            result.Aggressiveness = reader.ReadUInt32AsEnum<ObjectAggressiveness>();
                            break;

                        case "objectVisualRange":
                            result.VisualRange = reader.ReadUInt32();
                            break;

                        case "objectVeterancy":
                            result.Veterancy = reader.ReadUInt32AsEnum<ObjectVeterancy>();
                            break;

                        case "objectShroudClearingDistance":
                            result.ShroudClearingDistance = reader.ReadUInt32();
                            break;

                        case "objectStoppingDistance":
                            result.StoppingDistance = reader.ReadSingle();
                            break;

                        case "objectSoundAmbient":
                            result.SoundAmbient = reader.ReadUInt16PrefixedAsciiString();
                            break;

                        case "objectSoundAmbientCustomized":
                            result.SoundAmbientCustomized = reader.ReadBoolean();
                            break;

                        case "objectSoundAmbientLoopCount":
                            result.SoundAmbientLoopCount = reader.ReadUInt32();
                            break;

                        case "objectSoundAmbientMinVolume":
                            result.SoundAmbientMinVolume = reader.ReadSingle();
                            break;

                        case "objectSoundAmbientVolume":
                            result.SoundAmbientVolume = reader.ReadSingle();
                            break;

                        case "objectSoundAmbientMinRange":
                            result.SoundAmbientMinRange = reader.ReadSingle();
                            break;

                        case "objectSoundAmbientMaxRange":
                            result.SoundAmbientMaxRange = reader.ReadSingle();
                            break;

                        case "objectSoundAmbientPriority":
                            result.SoundAmbientPriority = reader.ReadUInt32AsEnum<ObjectAmbientSoundPriority>();
                            break;

                        default:
                            throw new InvalidDataException($"Unexpected property name: {propertyName}");
                    }
                });

                return result;
            });
        }
    }

    public enum ObjectWeather : uint
    {
        UseMapWeather,
        UseNormalModel,
        UseSnowModel
    }

    public enum ObjectTime : uint
    {
        UseMapTime,
        UseDayModel,
        UseNightModel
    }

    public enum ObjectAggressiveness : uint
    {
        Aggressive,
        Alert,
        Normal,
        Passive,
        Sleep
    }

    public enum ObjectVeterancy : uint
    {
        Normal,
        Veteran,
        Elite,
        Heroic
    }

    public enum ObjectAmbientSoundPriority : uint
    {
        Lowest,
        Low,
        Normal,
        High,
        Critical
    }
}
