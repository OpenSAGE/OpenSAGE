using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dEmitterRotationHeader
    {
        public uint KeyframeCount { get; private set; }

        /// <summary>
        /// Random initial rotational velocity (rotations/sec)
        /// </summary>
        public float Random { get; private set; }

        /// <summary>
        /// Random initial orientation (rotations 1.0=360deg)
        /// </summary>
        public float OrientationRandom { get; private set; }

        internal static W3dEmitterRotationHeader Parse(BinaryReader reader)
        {
            var result = new W3dEmitterRotationHeader
            {
                KeyframeCount = reader.ReadUInt32(),
                Random = reader.ReadSingle(),
                OrientationRandom = reader.ReadSingle()
            };

            reader.ReadBytes(sizeof(uint)); // Pad

            return result;
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(KeyframeCount);
            writer.Write(Random);
            writer.Write(OrientationRandom);

            writer.Write((uint) 0); // Pad
        }
    }
}
