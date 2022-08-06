using System.IO;
using System.Runtime.InteropServices;

namespace OpenSage.FileFormats.W3d
{
    [StructLayout(LayoutKind.Sequential)]
    public struct W3dRingOpacityChunk
    {
        public byte Version;
        public byte Size;
        public float Opacity;
        public float Position;

        internal static W3dRingOpacityChunk Parse(BinaryReader reader)
        {
            return new W3dRingOpacityChunk
            {
                Version = reader.ReadByte(),
                Size = reader.ReadByte(),
                Opacity = reader.ReadSingle(),
                Position = reader.ReadSingle()
            };
        }

        internal void Write(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(Size);
            writer.Write(Opacity);
            writer.Write(Position);
        }
    }
}
