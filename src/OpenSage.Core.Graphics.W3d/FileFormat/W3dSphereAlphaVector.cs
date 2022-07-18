using System.IO;
using System.Numerics;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dSphereAlphaVector
    {
        public uint ChunkType { get; private set; }

        public uint ChunkSize { get; private set; }

        public Vector3 Vector { get; private set; }

        public Vector2 Magnitude { get; private set; }

        public float Position { get; private set; }

        internal static W3dSphereAlphaVector Parse(BinaryReader reader)
        {
            var result = new W3dSphereAlphaVector
            {
                ChunkType = reader.ReadByte(),
                ChunkSize = reader.ReadByte(),
                Vector = reader.ReadVector3(),
                Magnitude = reader.ReadVector2(),
                Position = reader.ReadSingle()
            };

            return result;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(ChunkType);
            writer.Write(ChunkSize);
            writer.Write(Vector);
            writer.Write(Magnitude);
            writer.Write(Position);
        }
    }
}