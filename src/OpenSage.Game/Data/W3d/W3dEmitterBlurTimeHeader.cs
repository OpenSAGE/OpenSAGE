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

            reader.ReadBytes(sizeof(uint)); // Pad

            return result;
        }
    }
}
