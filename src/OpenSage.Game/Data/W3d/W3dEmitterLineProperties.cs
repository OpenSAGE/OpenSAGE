using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dEmitterLineProperties
    {
        public W3dEmitterLineFlags Flags { get; private set; }
        public uint SubdivisionLevel { get; private set; }
        public float NoiseAmplitude { get; private set; }
        public float MergeAbortFactor { get; private set; }
        public float TextureTileFactor { get; private set; }
        public float UPerSec { get; private set; }
        public float VPerSec { get; private set; }

        internal static W3dEmitterLineProperties Parse(BinaryReader reader)
        {
            var result = new W3dEmitterLineProperties
            {
                Flags = reader.ReadUInt32AsEnum<W3dEmitterLineFlags>(),
                SubdivisionLevel = reader.ReadUInt32(),
                NoiseAmplitude = reader.ReadSingle(),
                MergeAbortFactor = reader.ReadSingle(),
                TextureTileFactor = reader.ReadSingle(),
                UPerSec = reader.ReadSingle(),
                VPerSec = reader.ReadSingle()
            };

            reader.ReadBytes(sizeof(uint) * 9); // Padding

            return result;
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write((uint) Flags);
            writer.Write(SubdivisionLevel);
            writer.Write(NoiseAmplitude);
            writer.Write(MergeAbortFactor);
            writer.Write(TextureTileFactor);
            writer.Write(UPerSec);
            writer.Write(VPerSec);
        }
    }
}
