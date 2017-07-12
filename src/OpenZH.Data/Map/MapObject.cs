using System;
using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public sealed class MapObject : Asset
    {
        public const string AssetName = "Object";

        public MapVector3 Position { get; private set; }

        /// <summary>
        /// Angle of the object in radians.
        /// </summary>
        public float Angle { get; private set; }

        public RoadType RoadType { get; private set; }

        public string TypeName { get; private set; }

        public AssetPropertyCollection Properties { get; private set; }

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
                    Properties = AssetPropertyCollection.Parse(reader, context)
                };
            });
        }

        public void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            WriteAssetTo(writer, () =>
            {
                Position.WriteTo(writer);
                writer.Write(Angle);
                writer.Write((uint) RoadType);
                writer.WriteUInt16PrefixedAsciiString(TypeName);
                Properties.WriteTo(writer, assetNames);
            });
        }
    }

    // TODO: Figure these out properly.
    [Flags]
    public enum RoadType : uint
    {
        None = 0,

        Start = 2,
        End = 4,

        Angled = 8,

        Unknown1 = 16,
        Unknown2 = 32,

        Unknown1_Angled = Unknown1 | Angled,
        Unknown2_Angled = Unknown2 | Angled,

        TightCurve = 64,

        Unknown1_TightCurve = Unknown1 | TightCurve,
        Unknown2_TightCurve = Unknown2 | TightCurve,

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
