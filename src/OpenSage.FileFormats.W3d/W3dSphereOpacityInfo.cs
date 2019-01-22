using System.Collections.Generic;
using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dSphereOpacityInfo
    {
        public uint ChunkType { get; private set; }

        public uint ChunkSize { get; private set; }

        public uint Version { get; private set; }

        public List<W3dSphereOpacity> OpacityInfo { get; private set; }

        internal static W3dSphereOpacityInfo Parse(BinaryReader reader)
        {
            var result = new W3dSphereOpacityInfo
            {
                ChunkType = reader.ReadUInt32(),
                ChunkSize = reader.ReadUInt32() & 0x7FFFFFFF,
                Version = reader.ReadUInt32(),                      // ? Version or something else?
                OpacityInfo = new List<W3dSphereOpacity>()
            };

            var arraySize = reader.ReadUInt32();

            var arrayCount = arraySize / 10; // 10 = Size of OpacityInfo Array Chunk + Header Info
            for (var i = 0; i < arrayCount; i++)
            {
                var opacity = W3dSphereOpacity.Parse(reader);
                result.OpacityInfo.Add(opacity);
            }

            return result;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(ChunkType);
            writer.Write(ChunkSize);
            writer.Write(Version);

            foreach (var opacity in OpacityInfo)
            {
                opacity.Write(writer);
            }
        }
    }
}