using System;
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

        public RoadType RoadType { get; private set; }

        public string TypeName { get; private set; }

        public AssetProperty[] Properties { get; private set; }

        public static MapObject Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                return new MapObject
                {
                    Position = MapVector3.Parse(reader),
                    Angle = reader.ReadSingle(),
                    RoadType = reader.ReadUInt32AsEnum<RoadType>(),
                    TypeName = reader.ReadUInt16PrefixedAsciiString(),
                    Properties = ParseProperties(reader, context)
                };
            });
        }
    }

    [Flags]
    public enum RoadType : uint
    {
        None = 0,

        Start = 2,
        End = 4,

        Angled = 8,

        Unknown1 = 16,
        Unknown2 = 32,

        TightCurve = 64,

        EndCap = 128,

        BroadCurveStart = Start,
        BroadCurveEnd = End,

        AngledStart = Angled | Start,
        AngledEnd = Angled | End,

        TightCurveStart = TightCurve | Start,
        TightCurveEnd = TightCurve | End,

        BroadCurveEndCapStart = BroadCurveStart | EndCap,
        BroadCurveEndCapEnd = BroadCurveEnd | EndCap,

        AngledEndCapStart = AngledStart | EndCap,
        AngledEndCapEnd = AngledEnd | EndCap,

        TightCurveEndCapStart = TightCurveStart | EndCap,
        TightCurveEndCapEnd = TightCurveEnd | EndCap
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
