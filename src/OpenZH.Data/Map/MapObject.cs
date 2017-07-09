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

        public string ObjectName { get; private set; }
        public uint ObjectInitialHealth { get; private set; }
        public bool ObjectEnabled { get; private set; }
        public bool ObjectIndestructible { get; private set; }
        public bool ObjectUnsellable { get; private set; }
        public bool ObjectPowered { get; private set; }
        public bool ObjectAiRecruitable { get; private set; }
        public bool ObjectTargetable { get; private set; }
        public bool ObjectSelectable { get; private set; }
        public bool ObjectRepairable { get; private set; }
        public bool ObjectUseNightModels { get; private set; }
        public bool ObjectUseSnowModels { get; private set; }
        public float ObjectRadius { get; private set; }

        public uint ScorchType { get; private set; }

        public string ObjectOriginalOwner { get; private set; }
        public string ObjectUniqueId { get; private set; }
        public string ObjectLayer { get; private set; }
        public ObjectWeather ObjectWeather { get; private set; } = ObjectWeather.UseMapWeather;
        public ObjectTime ObjectTime { get; private set; } = ObjectTime.UseMapTime;
        public uint? ObjectMaxHitPoints { get; private set; }
        public ObjectAggressiveness ObjectAggressiveness { get; private set; } = ObjectAggressiveness.Normal;
        public ObjectVeterancy ObjectVeterancy { get; private set; } = ObjectVeterancy.Normal;
        public uint ObjectVisualRange { get; private set; }
        public uint ObjectShroudClearingDistance { get; private set; }
        public float ObjectStoppingDistance { get; private set; }

        public string SoundAmbient { get; private set; }
        public bool SoundAmbientCustomized { get; private set; }
        public uint SoundAmbientLoopCount { get; private set; }
        public float SoundAmbientMinVolume { get; private set; }
        public float SoundAmbientVolume { get; private set; }
        public float SoundAmbientMinRange { get; private set; }
        public float SoundAmbientMaxRange { get; private set; }
        public ObjectAmbientSoundPriority SoundAmbientPriority { get; private set; } = ObjectAmbientSoundPriority.Normal;

        public string WaypointPathLabel1 { get; private set; }
        public string WaypointPathLabel2 { get; private set; }
        public string WaypointPathLabel3 { get; private set; }
        public uint? WaypointID { get; private set; }
        public string WaypointName { get; private set; }
        public bool WaypointPathBiDirectional { get; private set; }

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
                            result.ObjectInitialHealth = reader.ReadUInt32();
                            break;

                        case "objectEnabled":
                            result.ObjectEnabled = reader.ReadBoolean();
                            break;

                        case "objectIndestructible":
                            result.ObjectIndestructible = reader.ReadBoolean();
                            break;

                        case "objectDestructible":
                            result.ObjectIndestructible = !reader.ReadBoolean();
                            break;

                        case "objectUnsellable":
                            result.ObjectUnsellable = reader.ReadBoolean();
                            break;

                        case "objectSellable":
                            result.ObjectUnsellable = !reader.ReadBoolean();
                            break;

                        case "objectPowered":
                            result.ObjectPowered = reader.ReadBoolean();
                            break;

                        case "objectRecruitable":
                        case "objectRecruitableAI":
                            result.ObjectAiRecruitable = reader.ReadBoolean();
                            break;

                        case "objectTargetable":
                            result.ObjectTargetable = reader.ReadBoolean();
                            break;

                        case "originalOwner":
                            result.ObjectOriginalOwner = reader.ReadUInt16PrefixedAsciiString();
                            break;

                        case "uniqueID":
                            result.ObjectUniqueId = reader.ReadUInt16PrefixedAsciiString();
                            break;

                        case "objectLayer":
                            result.ObjectLayer = reader.ReadUInt16PrefixedAsciiString();
                            break;

                        case "objectWeather":
                            result.ObjectWeather = reader.ReadUInt32AsEnum<ObjectWeather>();
                            break;

                        case "objectTime":
                            result.ObjectTime = reader.ReadUInt32AsEnum<ObjectTime>();
                            break;

                        case "objectName":
                            result.ObjectName = reader.ReadUInt16PrefixedAsciiString();
                            break;

                        case "objectMaxHPs":
                            result.ObjectMaxHitPoints = reader.ReadUInt32();
                            break;

                        case "objectAggressiveness":
                            result.ObjectAggressiveness = reader.ReadInt32AsEnum<ObjectAggressiveness>();
                            break;

                        case "objectVisualRange":
                            result.ObjectVisualRange = reader.ReadUInt32();
                            break;

                        case "objectVeterancy":
                            result.ObjectVeterancy = reader.ReadUInt32AsEnum<ObjectVeterancy>();
                            break;

                        case "objectShroudClearingDistance":
                            result.ObjectShroudClearingDistance = reader.ReadUInt32();
                            break;

                        case "objectStoppingDistance":
                            result.ObjectStoppingDistance = reader.ReadSingle();
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

                        case "objectSelectable":
                            result.ObjectSelectable = reader.ReadBoolean();
                            break;

                        case "objectRepairable":
                            result.ObjectRepairable = reader.ReadBoolean();
                            break;

                        case "objectUseNightModels":
                            result.ObjectUseNightModels = reader.ReadBoolean();
                            break;

                        case "objectUseSnowModels":
                            result.ObjectUseSnowModels = reader.ReadBoolean();
                            break;

                        case "objectRadius":
                            result.ObjectRadius = reader.ReadSingle();
                            break;

                        case "scorchType":
                            result.ScorchType = reader.ReadUInt32();
                            break;

                        case "waypointPathLabel1":
                            result.WaypointPathLabel1 = reader.ReadUInt16PrefixedAsciiString();
                            break;

                        case "waypointPathLabel2":
                            result.WaypointPathLabel2 = reader.ReadUInt16PrefixedAsciiString();
                            break;

                        case "waypointPathLabel3":
                            result.WaypointPathLabel3 = reader.ReadUInt16PrefixedAsciiString();
                            break;

                        case "waypointID":
                            result.WaypointID = reader.ReadUInt32();
                            break;

                        case "waypointName":
                            result.WaypointName = reader.ReadUInt16PrefixedAsciiString();
                            break;

                        case "waypointPathBiDirectional":
                            result.WaypointPathBiDirectional = reader.ReadBoolean();
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

    public enum ObjectAggressiveness : int
    {
        Sleep = -2,
        Passive,
        Normal,
        Alert,
        Aggressive,
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
