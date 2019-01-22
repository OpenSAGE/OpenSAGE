using System.IO;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.FileFormats;

namespace OpenSage.Data.Map
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

        public uint MagicValue { get; private set; }

        public string Name { get; private set; }

        internal static BlendTileTexture Parse(BinaryReader reader)
        {
            var cellStart = reader.ReadUInt32();
            var cellCount = reader.ReadUInt32();
            var cellSize = reader.ReadUInt32();

            if (cellSize * cellSize != cellCount)
            {
                throw new InvalidDataException();
            }

            var magicValue = reader.ReadUInt32();
            if (magicValue != 0)
            {
                throw new InvalidDataException();
            }

            var name = reader.ReadUInt16PrefixedAsciiString();

            return new BlendTileTexture
            {
                CellStart = cellStart,
                CellCount = cellCount,
                CellSize = cellSize,
                MagicValue = magicValue,
                Name = name
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(CellStart);
            writer.Write(CellCount);
            writer.Write(CellSize);

            writer.Write(MagicValue);

            writer.WriteUInt16PrefixedAsciiString(Name);
        }
    }
}
