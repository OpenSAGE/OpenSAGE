using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Map
{
    public sealed class StandingWaterArea
    {
        public uint UniqueID { get; private set; }
        public string Name { get; private set; }
        public string LayerName { get; private set; }

        public float UvScrollSpeed { get; private set; }
        public bool UseAdditiveBlending { get; private set; }

        public string BumpMapTexture { get; private set; }
        public string SkyTexture { get; private set; }

        public Vector2[] Points { get; private set; }

        public uint WaterHeight { get; private set; }

        public string FxShader { get; private set; }
        public string DepthColors { get; private set; }

        internal static StandingWaterArea Parse(BinaryReader reader)
        {
            var result = new StandingWaterArea
            {
                UniqueID = reader.ReadUInt32(),
                Name = reader.ReadUInt16PrefixedAsciiString(),
                LayerName = reader.ReadUInt16PrefixedAsciiString(),
                UvScrollSpeed = reader.ReadSingle(),
                UseAdditiveBlending = reader.ReadBooleanChecked(),
                BumpMapTexture = reader.ReadUInt16PrefixedAsciiString(),
                SkyTexture = reader.ReadUInt16PrefixedAsciiString(),
            };

            var numPoints = reader.ReadUInt32();
            result.Points = new Vector2[numPoints];

            for (var i = 0; i < numPoints; i++)
            {
                result.Points[i] = reader.ReadVector2();
            }

            result.WaterHeight = reader.ReadUInt32();
            result.FxShader = reader.ReadUInt16PrefixedAsciiString();
            result.DepthColors = reader.ReadUInt16PrefixedAsciiString();

            return result;
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(UniqueID);
            writer.WriteUInt16PrefixedAsciiString(Name);
            writer.WriteUInt16PrefixedAsciiString(LayerName);
            writer.Write(UvScrollSpeed);
            writer.Write(UseAdditiveBlending);
            writer.WriteUInt16PrefixedAsciiString(BumpMapTexture);
            writer.WriteUInt16PrefixedAsciiString(SkyTexture);

            writer.Write((uint) Points.Length);
            foreach (var point in Points)
            {
                writer.Write(point);
            }

            writer.Write(WaterHeight);
            writer.WriteUInt16PrefixedAsciiString(FxShader);
            writer.WriteUInt16PrefixedAsciiString(SkyTexture);
        }
    }
}
