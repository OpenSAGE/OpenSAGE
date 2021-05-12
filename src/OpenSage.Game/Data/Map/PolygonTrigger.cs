using System;
using System.IO;
using System.Numerics;
using OpenSage.Data.Sav;
using OpenSage.FileFormats;
using OpenSage.Mathematics;

namespace OpenSage.Data.Map
{
    public sealed class PolygonTrigger
    {
        public string Name { get; private set; }
        public string LayerName { get; private set; }
        public uint UniqueId { get; private set; }
        public PolygonTriggerType TriggerType { get; private set; }

        /// <summary>
        /// For rivers, this is the index into the array of Points
        /// for the point where the river flows from.
        /// </summary>
        public uint RiverStartControlPoint { get; private set; }

        // Shared water options
        public bool UseAdditiveBlending { get; private set; }
        public Vector2 UvScrollSpeed { get; private set; }

        // River-specific options
        public string RiverTexture { get; private set; }
        public string NoiseTexture { get; private set; }
        public string AlphaEdgeTexture { get; private set; }
        public string SparkleTexture { get; private set; }
        public string BumpMapTexture { get; private set; }
        public string SkyTexture { get; private set; }
        public byte Unknown { get; private set; }
        public ColorRgb RiverColor { get; private set; }
        public float RiverAlpha { get; private set; }

        public MapVector3i[] Points { get; private set; }

        public Rectangle Bounds { get; private set; }

        public float Radius { get; private set; }

        internal static PolygonTrigger Parse(BinaryReader reader, ushort version)
        {
            var result = new PolygonTrigger
            {
                Name = reader.ReadUInt16PrefixedAsciiString()
            };

            if (version >= 4)
            {
                result.LayerName = reader.ReadUInt16PrefixedAsciiString();
            }

            result.UniqueId = reader.ReadUInt32();

            result.TriggerType = reader.ReadUInt16AsEnum<PolygonTriggerType>();

            result.RiverStartControlPoint = reader.ReadUInt32();

            if (version >= 5)
            {
                result.RiverTexture = reader.ReadUInt16PrefixedAsciiString();
                result.NoiseTexture = reader.ReadUInt16PrefixedAsciiString();
                result.AlphaEdgeTexture = reader.ReadUInt16PrefixedAsciiString();
                result.SparkleTexture = reader.ReadUInt16PrefixedAsciiString();
                result.BumpMapTexture = reader.ReadUInt16PrefixedAsciiString();
                result.SkyTexture = reader.ReadUInt16PrefixedAsciiString();

                result.UseAdditiveBlending = reader.ReadBooleanChecked();

                result.RiverColor = reader.ReadColorRgb();

                result.Unknown = reader.ReadByte(); // 0
                if (result.Unknown != 0)
                {
                    throw new InvalidDataException();
                }

                result.UvScrollSpeed = reader.ReadVector2();

                result.RiverAlpha = reader.ReadSingle();
            }

            var numPoints = reader.ReadUInt32();
            result.Points = new MapVector3i[numPoints];

            for (var i = 0; i < numPoints; i++)
            {
                result.Points[i] = MapVector3i.Parse(reader);
            }

            return result;
        }

        internal void WriteTo(BinaryWriter writer, ushort version)
        {
            writer.WriteUInt16PrefixedAsciiString(Name);

            if (version >= 4)
            {
                writer.WriteUInt16PrefixedAsciiString(LayerName);
            }

            writer.Write(UniqueId);

            writer.Write((ushort) TriggerType);

            writer.Write(RiverStartControlPoint);

            if (version >= 5)
            {
                writer.WriteUInt16PrefixedAsciiString(RiverTexture);
                writer.WriteUInt16PrefixedAsciiString(NoiseTexture);
                writer.WriteUInt16PrefixedAsciiString(AlphaEdgeTexture);
                writer.WriteUInt16PrefixedAsciiString(SparkleTexture);
                writer.WriteUInt16PrefixedAsciiString(BumpMapTexture);
                writer.WriteUInt16PrefixedAsciiString(SkyTexture);

                writer.Write(UseAdditiveBlending);

                writer.Write(RiverColor);

                writer.Write(Unknown);

                writer.Write(UvScrollSpeed);

                writer.Write(RiverAlpha);
            }

            writer.Write((uint) Points.Length);

            foreach (var point in Points)
            {
                point.WriteTo(writer);
            }
        }

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var numPoints = reader.ReadUInt32();
            if (numPoints != Points.Length)
            {
                throw new InvalidDataException();
            }

            for (var i = 0; i < numPoints; i++)
            {
                Points[i] = MapVector3i.Parse(reader.Inner);
            }

            var topLeft = reader.ReadPoint2D();
            var bottomRight = reader.ReadPoint2D();

            Bounds = Rectangle.FromCorners(topLeft, bottomRight);

            // The following value is what you get if you do this calculation:
            // width = (bottomRight.X - topLeft.X) * 0.5
            // height = (bottomRight.Y + topLeft.Y) * 0.5
            // value = sqrt(width * width + height * height)
            //
            // This looks like it's supposed to be a radius for this polygon trigger,
            // presumably used for quick distance tests prior to testing if
            // a point is inside the actual polygon.
            //
            // But there's a mistake... the height should instead be:
            // height = (bottomRight.Y - topLeft.Y) * 0.5
            //
            // As it is, this "radius" is significantly larger than it should be.
            var _ = reader.ReadSingle();

            Radius = MathF.Sqrt(Bounds.Width * Bounds.Width + Bounds.Height * Bounds.Height);

            var unknown = reader.ReadBoolean();
            if (unknown)
            {
                throw new InvalidDataException();
            }
        }
    }

    [Flags]
    public enum PolygonTriggerType : ushort
    {
        Area = 0,
        Water = 1,
        River = 256,

        WaterAndRiver = Water | River,
    }
}
