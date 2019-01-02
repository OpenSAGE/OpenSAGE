using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dSphereOpacity
    {
        public uint ChunkType { get; private set; }

        public uint ChunkSize { get; private set; }

        public float Opacity { get; private set; }

        public float Position { get; private set; }

        internal static W3dSphereOpacity Parse(BinaryReader reader)
        {
            var result = new W3dSphereOpacity
            {
                ChunkType = reader.ReadByte(),
                ChunkSize = reader.ReadByte(),
                Opacity = reader.ReadSingle(),
                Position = reader.ReadSingle()
            };

            return result;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(ChunkType);
            writer.Write(ChunkSize);
            writer.Write(Opacity);
            writer.Write(Position);
        }
    }
}