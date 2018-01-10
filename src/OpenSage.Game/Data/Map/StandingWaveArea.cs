using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Map
{
    [AddedIn(SageGame.BattleForMiddleEarthII)]
    public sealed class StandingWaveArea
    {
        public uint UniqueID { get; private set; }
        public string Name { get; private set; }
        public string LayerName { get; private set; }

        // Unused?
        public float UvScrollSpeed { get; private set; }
        public bool UseAdditiveBlending { get; private set; }

        public Vector2[] Points { get; private set; }

        public uint FinalWidth { get; private set; }
        public uint FinalHeight { get; private set; }
        public uint InitialWidthFraction { get; private set; }
        public uint InitialHeightFraction { get; private set; }
        public uint InitialVelocity { get; private set; }
        public uint TimeToFade { get; private set; }
        public uint TimeToCompress { get; private set; }
        public uint TimeOffset2ndWave { get; private set; }
        public uint DistanceFromShore { get; private set; }
        public string Texture { get; private set; }
        public bool EnablePcaWave { get; private set; }

        internal static StandingWaveArea Parse(BinaryReader reader, ushort version)
        {
            var result = new StandingWaveArea
            {
                UniqueID = reader.ReadUInt32(),
                Name = reader.ReadUInt16PrefixedAsciiString(),
                LayerName = reader.ReadUInt16PrefixedAsciiString(),
                UvScrollSpeed = reader.ReadSingle(),
                UseAdditiveBlending = reader.ReadBooleanChecked()
            };

            var numPoints = reader.ReadUInt32();
            result.Points = new Vector2[numPoints];

            for (var i = 0; i < numPoints; i++)
            {
                result.Points[i] = reader.ReadVector2();
            }

            var unknown = reader.ReadUInt32();
            if (unknown != 0)
            {
                throw new InvalidDataException();
            }

            if (version < 3)
            {
                result.FinalWidth = reader.ReadUInt32();
                result.FinalHeight = reader.ReadUInt32();
                result.InitialWidthFraction = reader.ReadUInt32();
                result.InitialHeightFraction = reader.ReadUInt32();
                result.InitialVelocity = reader.ReadUInt32();
                result.TimeToFade = reader.ReadUInt32();
                result.TimeToCompress = reader.ReadUInt32();
                result.TimeOffset2ndWave = reader.ReadUInt32();
                result.DistanceFromShore = reader.ReadUInt32();
                result.Texture = reader.ReadUInt16PrefixedAsciiString();

                if (version == 2)
                {
                    result.EnablePcaWave = reader.ReadBooleanUInt32Checked();
                }
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

            writer.Write((uint) Points.Length);
            foreach (var point in Points)
            {
                writer.Write(point);
            }

            writer.Write((uint) 0);

            if (version < 3)
            {
                writer.Write(FinalWidth);
                writer.Write(FinalHeight);
                writer.Write(InitialWidthFraction);
                writer.Write(InitialHeightFraction);
                writer.Write(InitialVelocity);
                writer.Write(TimeToFade);
                writer.Write(TimeToCompress);
                writer.Write(TimeOffset2ndWave);
                writer.Write(DistanceFromShore);
                writer.WriteUInt16PrefixedAsciiString(Texture);

                if (version == 2)
                {
                    writer.WriteBooleanUInt32(EnablePcaWave);
                }
            }
        }
    }
}
