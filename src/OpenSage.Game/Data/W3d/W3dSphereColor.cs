using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dSphereColor
    {
        public uint ChunkType { get; private set; }

        public uint ChunkSize { get; private set; }

        public W3dRgbF Color { get; private set; }

        public float Position { get; private set; }

        internal static W3dSphereColor Parse(BinaryReader reader)
        {
            var result = new W3dSphereColor
            {
                ChunkType = reader.ReadByte(),
                ChunkSize = reader.ReadByte(),
                Color = W3dRgbF.Parse(reader),
                Position = reader.ReadSingle()
            };

            return result;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(ChunkType);
            writer.Write(ChunkSize);
            W3dRgbF.Write(writer, Color);
            writer.Write(Position);
        }
    }
}