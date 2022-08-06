using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public struct W3dRingScaleKeyFrames
    {
        public uint ChunkType { get; private set; }

        public uint ChunkSize { get; private set; }

        public uint Version { get; private set; }

        public List<W3dRingScaleKeyFrame> ScaleKeyFrames { get; private set; }

        internal static W3dRingScaleKeyFrames Parse(BinaryReader reader)
        {
            var result = new W3dRingScaleKeyFrames
            {
                ChunkType = reader.ReadUInt32(),
                ChunkSize = reader.ReadUInt32() & 0x7FFFFFFF,
                Version = reader.ReadUInt32(),
                ScaleKeyFrames = new List<W3dRingScaleKeyFrame>()
            };

            var arraySize = reader.ReadUInt32();

            var arrayCount = arraySize / 14; // 14 = Size of Array Chunk + Header Info
            for (var i = 0; i < arrayCount; i++)
            {
                var scaleKeyFrame = W3dRingScaleKeyFrame.Parse(reader);
                result.ScaleKeyFrames.Add(scaleKeyFrame);
            }

            return result;
        }

        internal void Write(BinaryWriter writer)
        {
            writer.Write(ChunkType);
            writer.Write(ChunkSize);
            writer.Write(Version);

            foreach (var scaleKeyFrame in ScaleKeyFrames)
            {
                scaleKeyFrame.Write(writer);
            }
        }
    }
}
