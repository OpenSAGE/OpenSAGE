using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.Mathematics;

namespace OpenSage.Data.W3d
{
    public sealed class W3dSphereColor
    {
        public uint ChunkType { get; private set; }

        public uint ChunkSize { get; private set; }

        public ColorRgbF Color { get; private set; }

        public float Position { get; private set; }

        internal static W3dSphereColor Parse(BinaryReader reader)
        {
            var result = new W3dSphereColor
            {
                ChunkType = reader.ReadByte(),
                ChunkSize = reader.ReadByte(),
                Color = reader.ReadColorRgbF(),
                Position = reader.ReadSingle()
            };

            return result;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(ChunkType);
            writer.Write(ChunkSize);
            writer.Write(Color);
            writer.Write(Position);
        }
    }
}