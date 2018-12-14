using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dEmitterBlurTimeHeader
    {
        public uint KeyframeCount { get; private set; }

        public float Random { get; private set; }

        internal static W3dEmitterBlurTimeHeader Parse(BinaryReader reader)
        {
            var result = new W3dEmitterBlurTimeHeader
            {
                KeyframeCount = reader.ReadUInt32(),
                Random = reader.ReadSingle()
            };

            reader.ReadUInt32(); // Pad

            return result;
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(KeyframeCount);
            writer.Write(Random);
            writer.Write((uint) 0); // Pad
        }
    }
}
