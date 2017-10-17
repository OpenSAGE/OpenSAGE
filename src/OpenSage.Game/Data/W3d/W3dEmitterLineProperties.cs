using System.IO;

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

        public static W3dEmitterLineProperties Parse(BinaryReader reader)
        {
            var result = new W3dEmitterLineProperties
            {
                Flags = (W3dEmitterLineFlags) reader.ReadUInt32(),
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
    }
}
