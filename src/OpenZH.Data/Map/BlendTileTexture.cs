using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public sealed class BlendTileTexture
    {
        public uint CellStart { get; private set; }

        /// <summary>
        /// Texture "cells" are 64x64 blocks within a source texture.
        /// So for example, a 128x128 texture has 4 cells.
        /// </summary>
        public uint CellCount { get; private set; }

        /// <summary>
        /// Size of this texture, in texture cell units.
        /// </summary>
        public uint CellSize { get; private set; }

        public string Name { get; private set; }

        public static BlendTileTexture Parse(BinaryReader reader)
        {
            var cellStart = reader.ReadUInt32();
            var cellCount = reader.ReadUInt32();
            var cellSize = reader.ReadUInt32();

            if (cellSize * cellSize != cellCount)
            {
                throw new InvalidDataException();
            }

            var unknown = reader.ReadUInt32();
            if (unknown != 0)
            {
                throw new InvalidDataException();
            }

            var name = reader.ReadUInt16PrefixedAsciiString();

            return new BlendTileTexture
            {
                CellStart = cellStart,
                CellCount = cellCount,
                CellSize = cellSize,
                Name = name
            };
        }
    }
}
