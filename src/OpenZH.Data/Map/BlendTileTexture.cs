using System;
using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public sealed class BlendTileTexture
    {
        public uint CellStart { get; private set; }
        public uint CellCount { get; private set; }
        public string Name { get; private set; }

        public static BlendTileTexture Parse(BinaryReader reader)
        {
            var cellStart = reader.ReadUInt32();
            var cellCount = reader.ReadUInt32();

            var unknown1 = reader.ReadUInt32();
            var unknown2 = reader.ReadUInt32();

            var name = reader.ReadUInt16PrefixedString();

            if (cellCount != 4)
            {
                throw new Exception();
            }
            if (unknown1 != 2)
            {
                throw new Exception();
            }
            if (unknown2 != 0)
            {
                throw new Exception();
            }

            return new BlendTileTexture
            {
                CellStart = cellStart,
                CellCount = cellCount,
                Name = name
            };
        }
    }
}
