using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public struct W3dRingColorInfo
    {
        public uint ChunkType { get; private set; }

        public uint ChunkSize { get; private set; }

        public uint Version { get; private set; }

        public List<W3dRingColorChunk> ColorChunks { get; private set; }

        internal static W3dRingColorInfo Parse(BinaryReader reader)
        {
            var result = new W3dRingColorInfo
            {
                ChunkType = reader.ReadUInt32(),
                ChunkSize = reader.ReadUInt32() & 0x7FFFFFFF,
                Version = reader.ReadUInt32(),                      // ? Version or something else
                ColorChunks = new List<W3dRingColorChunk>()
            };

            var arraySize = reader.ReadUInt32();

            var arrayCount = arraySize / 18; // 18 = Size of Array Chunk + Header Info
            for (var i = 0; i < arrayCount; i++)
            {
                var color = W3dRingColorChunk.Parse(reader);
                result.ColorChunks.Add(color);
            }

            return result;
        }
    
        internal void Write(BinaryWriter writer)
        {
            writer.Write(ChunkType);
            writer.Write(ChunkSize);
            writer.Write(Version);

            foreach (var colorChunk in ColorChunks)
            {
                colorChunk.Write(writer);
            }
        }
    }
}
