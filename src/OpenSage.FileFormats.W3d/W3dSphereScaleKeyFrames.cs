using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dSphereScaleKeyFrames
    {
        public uint ChunkType { get; private set; }

        public uint ChunkSize { get; private set; }

        public uint Version { get; private set; }

        public List<W3dSphereScaleKeyFrame> ScaleKeyFrames { get; private set; }

        internal static W3dSphereScaleKeyFrames Parse(BinaryReader reader)
        {
            var result = new W3dSphereScaleKeyFrames
            {
                ChunkType = reader.ReadUInt32(),
                ChunkSize = reader.ReadUInt32() & 0x7FFFFFFF,
                Version = reader.ReadUInt32(),
                ScaleKeyFrames = new List<W3dSphereScaleKeyFrame>()
            };

            var arraySize = reader.ReadUInt32();

            var arrayCount = arraySize / 18; // 18 = Size of OpacityInfo Array Chunk + Header Info
            for (var i = 0; i < arrayCount; i++)
            {
                var keyFrame = W3dSphereScaleKeyFrame.Parse(reader);
                result.ScaleKeyFrames.Add(keyFrame);
            }

            return result;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(ChunkType);
            writer.Write(ChunkSize);
            writer.Write(Version);

            foreach (var keyFrame in ScaleKeyFrames)
            {
                keyFrame.Write(writer);
            }
        }
    }
}