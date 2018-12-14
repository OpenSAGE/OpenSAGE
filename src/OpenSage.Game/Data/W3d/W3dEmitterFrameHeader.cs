using System.IO;

namespace OpenSage.Data.W3d
{
    /// <summary>
    /// Frames keyframes are for sub-texture indexing.
    /// </summary>
    public sealed class W3dEmitterFrameHeader
    {
        public uint KeyframeCount { get; private set; }

        public float Random { get; private set; }

        internal static W3dEmitterFrameHeader Parse(BinaryReader reader)
        {
            var result = new W3dEmitterFrameHeader
            {
                KeyframeCount = reader.ReadUInt32(),
                Random = reader.ReadSingle()
            };

            reader.ReadBytes(2 * sizeof(uint)); // Pad

            return result;
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(KeyframeCount);
            writer.Write(Random);

            writer.Write((uint) 0); // Pad
            writer.Write((uint) 0);
        }
    }
}
