using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dSphereColors
    {
        public uint ChunkType { get; private set; }

        public uint ChunkSize { get; private set; }

        public uint Version { get; private set; }

        public List<W3dSphereColor> Colors { get; private set; }

        internal static W3dSphereColors Parse(BinaryReader reader)
        {
            var result = new W3dSphereColors
            {
                ChunkType = reader.ReadUInt32(),
                ChunkSize = reader.ReadUInt32() & 0x7FFFFFFF,
                Version = reader.ReadUInt32(),                      // ? Version or something else?
                Colors = new List<W3dSphereColor>()
            };

            var arraySize = reader.ReadUInt32();

            var arrayCount = arraySize / 18; // 18 = Size of Color Array Chunk + Header Info
            for (var i = 0; i < arrayCount; i++)
            {
                var color = W3dSphereColor.Parse(reader);
                result.Colors.Add(color);
            }

            return result;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(ChunkType);
            writer.Write(ChunkSize);
            writer.Write(Version);

            foreach (var color in Colors)
            {
                color.Write(writer);
            }
        }
    }
}