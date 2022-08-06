using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public struct W3dRingOpacityInfo
    {
        public uint ChunkType { get; private set; }

        public uint ChunkSize { get; private set; }

        public uint Version { get; private set; }

        public List<W3dRingOpacityChunk> OpacityChunks { get; private set; }

        internal static W3dRingOpacityInfo Parse(BinaryReader reader)
        {
            var result = new W3dRingOpacityInfo
            {
                ChunkType = reader.ReadUInt32(),
                ChunkSize = reader.ReadUInt32() & 0x7FFFFFFF,
                Version = reader.ReadUInt32(),
                OpacityChunks = new List<W3dRingOpacityChunk>()
            };

            var arraySize = reader.ReadUInt32();

            var arrayCount = arraySize / 10; // 10 = Size of Array Chunk + Header Info
            for (var i = 0; i < arrayCount; i++)
            {
                var opacity = W3dRingOpacityChunk.Parse(reader);
                result.OpacityChunks.Add(opacity);
            }

            return result;
        }

        internal void Write(BinaryWriter writer)
        {
            writer.Write(ChunkType);
            writer.Write(ChunkSize);
            writer.Write(Version);

            foreach (var opacityChunk in OpacityChunks)
            {
                opacityChunk.Write(writer);
            }
        }
    }
}
