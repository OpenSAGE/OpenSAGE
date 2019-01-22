using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dSphereScaleKeyFrame
    {
        public uint ChunkType { get; private set; }

        public uint ChunkSize { get; private set; }

        public Vector3 ScaleKeyFrame { get; private set; }

        public float Position { get; private set; }

        internal static W3dSphereScaleKeyFrame Parse(BinaryReader reader)
        {
            var result = new W3dSphereScaleKeyFrame
            {
                ChunkType = reader.ReadByte(),
                ChunkSize = reader.ReadByte(),
                ScaleKeyFrame = reader.ReadVector3(),
                Position = reader.ReadSingle()
            };

            return result;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(ChunkType);
            writer.Write(ChunkSize);
            writer.Write(ScaleKeyFrame);
            writer.Write(Position);
        }
    }
}