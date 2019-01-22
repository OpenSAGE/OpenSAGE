using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.FileFormats;
using OpenSage.Mathematics;

namespace OpenSage.Data.Map
{
    public sealed class RiverArea
    {
        public uint UniqueID { get; private set; }
        public string Name { get; private set; }
        public string LayerName { get; private set; }

        public float UvScrollSpeed { get; private set; }
        public bool UseAdditiveBlending { get; private set; }

        public string RiverTexture { get; private set; }
        public string NoiseTexture { get; private set; }
        public string AlphaEdgeTexture { get; private set; }
        public string SparkleTexture { get; private set; }
        public ColorRgb Color { get; private set; }
        public float Alpha { get; private set; }
        public uint WaterHeight { get; private set; }

        [AddedIn(SageGame.Cnc3)]
        public string RiverType { get; private set; }

        public string MinimumWaterLod { get; private set; }

        public Line2D[] Lines { get; private set; }

        internal static RiverArea Parse(BinaryReader reader, ushort version)
        {
            var result = new RiverArea
            {
                UniqueID = reader.ReadUInt32(),
                Name = reader.ReadUInt16PrefixedAsciiString(),
                LayerName = reader.ReadUInt16PrefixedAsciiString(),
                UvScrollSpeed = reader.ReadSingle(),
                UseAdditiveBlending = reader.ReadBooleanChecked(),
                RiverTexture = reader.ReadUInt16PrefixedAsciiString(),
                NoiseTexture = reader.ReadUInt16PrefixedAsciiString(),
                AlphaEdgeTexture = reader.ReadUInt16PrefixedAsciiString(),
                SparkleTexture = reader.ReadUInt16PrefixedAsciiString(),
                Color = reader.ReadColorRgb()
            };

            var unusedColorA = reader.ReadByte();
            if (unusedColorA != 0)
            {
                throw new InvalidDataException();
            }

            result.Alpha = reader.ReadSingle();
            result.WaterHeight = reader.ReadUInt32();

            if (version >= 3)
            {
                result.RiverType = reader.ReadUInt16PrefixedAsciiString();
            }

            result.MinimumWaterLod = reader.ReadUInt16PrefixedAsciiString();

            var numLines = reader.ReadUInt32();
            result.Lines = new Line2D[numLines];

            for (var i = 0; i < numLines; i++)
            {
                result.Lines[i] = reader.ReadLine2D();
            }

            return result;
        }

        internal void WriteTo(BinaryWriter writer, ushort version)
        {
            writer.Write(UniqueID);
            writer.WriteUInt16PrefixedAsciiString(Name);
            writer.WriteUInt16PrefixedAsciiString(LayerName);
            writer.Write(UvScrollSpeed);
            writer.Write(UseAdditiveBlending);
            writer.WriteUInt16PrefixedAsciiString(RiverTexture);
            writer.WriteUInt16PrefixedAsciiString(NoiseTexture);
            writer.WriteUInt16PrefixedAsciiString(AlphaEdgeTexture);
            writer.WriteUInt16PrefixedAsciiString(SparkleTexture);
            writer.Write(Color);
            writer.Write((byte) 0);
            writer.Write(Alpha);
            writer.Write(WaterHeight);

            if (version >= 3)
            {
                writer.WriteUInt16PrefixedAsciiString(RiverType);
            }

            writer.WriteUInt16PrefixedAsciiString(MinimumWaterLod);

            writer.Write((uint) Lines.Length);
            foreach (var line in Lines)
            {
                writer.Write(line);
            }
        }
    }
}
