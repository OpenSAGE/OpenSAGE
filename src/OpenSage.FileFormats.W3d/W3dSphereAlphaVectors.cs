using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dSphereAlphaVectors
    {
        public uint ChunkType { get; private set; }

        public uint ChunkSize { get; private set; }

        public uint Version { get; private set; }

        public List<W3dSphereAlphaVector> AlphaVectors { get; private set; }

        internal static W3dSphereAlphaVectors Parse(BinaryReader reader)
        {
            var result = new W3dSphereAlphaVectors
            {
                ChunkType = reader.ReadUInt32(),
                ChunkSize = reader.ReadUInt32() & 0x7FFFFFFF,
                Version = reader.ReadUInt32(),
                AlphaVectors = new List<W3dSphereAlphaVector>()
            };

            var arraySize = reader.ReadUInt32();

            var arrayCount = arraySize / 26; // 26 = Size of OpacityInfo Array Chunk + Header Info
            for (var i = 0; i < arrayCount; i++)
            {
                var alphaVector = W3dSphereAlphaVector.Parse(reader);
                result.AlphaVectors.Add(alphaVector);
            }

            return result;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(ChunkType);
            writer.Write(ChunkSize);
            writer.Write(Version);

            foreach (var alphaVector in AlphaVectors)
            {
                alphaVector.Write(writer);
            }
        }
    }
}