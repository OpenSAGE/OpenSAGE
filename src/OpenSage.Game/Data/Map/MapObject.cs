﻿using System.Diagnostics;
using System.IO;
using System.Numerics;
using OpenSage.FileFormats;

namespace OpenSage.Data.Map
{
    [DebuggerDisplay("{TypeName} ({Position})")]
    public sealed class MapObject : Asset
    {
        public const string AssetName = "Object";

        public Vector3 Position { get; set; }

        /// <summary>
        /// Angle of the object in radians.
        /// </summary>
        public float Angle { get; private set; }

        public RoadType RoadType { get; private set; }

        public string TypeName { get; private set; }

        public AssetPropertyCollection Properties { get; private set; }

        internal static MapObject Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                return new MapObject
                {
                    Position = reader.ReadVector3(),
                    Angle = reader.ReadSingle(),
                    RoadType = reader.ReadUInt32AsEnumFlags<RoadType>(),
                    TypeName = reader.ReadUInt16PrefixedAsciiString(),
                    Properties = AssetPropertyCollection.Parse(reader, context)
                };
            });
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write(Position);
                writer.Write(Angle);
                writer.Write((uint) RoadType);
                writer.WriteUInt16PrefixedAsciiString(TypeName);
                Properties.WriteTo(writer, assetNames);
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
