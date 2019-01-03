using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.Mathematics;

namespace OpenSage.Data.W3d
{
    [StructLayout(LayoutKind.Sequential)]
    public struct W3dRingColorChunk
    {
        public byte Version;
        public byte Size;
        public ColorRgbF Color;
        public float Position;

        internal static W3dRingColorChunk Parse(BinaryReader reader)
        {
            return new W3dRingColorChunk
            {
                Version = reader.ReadByte(),
                Size = reader.ReadByte(),
                Color = W3dRgbF.Parse(reader),
                Position = reader.ReadSingle()
            };
        }

        internal void Write(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(Size);

            W3dRgbF.Write(writer, Color);

            writer.Write(Position);
        }
    }
}
