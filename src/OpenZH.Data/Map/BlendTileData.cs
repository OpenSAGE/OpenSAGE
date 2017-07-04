using System.Collections.Generic;
using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public sealed class BlendTileData
    {
        public uint NumTiles { get; private set; }

        public ushort[,] Tiles { get; private set; }
        public ushort[,] BlendIndices { get; private set; }
        public ushort[,] SingleEdgeBlends { get; private set; }
        public ushort[,] CliffBlends { get; private set; }

        public bool[,] Passability { get; private set; }

        public BlendTileTexture[] Textures { get; private set; }

        public BlendInfo[] Blends { get; private set; }

        /// <summary>
        /// Derived data.
        /// </summary>
        public BlendTileTextureIndex[] TextureIndices { get; private set; }

        public static BlendTileData Parse(BinaryReader reader, HeightMapData heightMapData)
        {
            var width = heightMapData.Width;
            var height = heightMapData.Height;

            var numTiles = reader.ReadUInt32();
            if (numTiles != width * height)
            {
                throw new InvalidDataException();
            }

            var tiles = reader.ReadUInt16Array2D(width, height);
            var blendIndices = reader.ReadUInt16Array2D(width, height);
            var singleEdgeBlends = reader.ReadUInt16Array2D(width, height);
            var cliffBlends = reader.ReadUInt16Array2D(width, height);

            // If terrain is passable, there's a 0 in the data file.
            var passability = reader.ReadSingleBitBooleanArray2D(heightMapData.Width, heightMapData.Height);
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    passability[x, y] = !passability[x, y];
                }
            }

            var textureCellCount = reader.ReadUInt32();
            var blendsCount = reader.ReadUInt32() - 1;
            var cliffBlendsCount = reader.ReadUInt32() - 1;

            var textureCount = reader.ReadUInt32();
            var textures = new BlendTileTexture[textureCount];
            for (var i = 0; i < textureCount; i++)
            {
                textures[i] = BlendTileTexture.Parse(reader);
            }

            // Calculate texture index + offset within texture.
            var textureIndices = new List<BlendTileTextureIndex>();
            var actualCellIndex = 0u;
            for (var i = 0; i < textures.Length; i++)
            {
                var texture = textures[i];

                var actualCellCount = (texture.CellSize * 2) * (texture.CellSize * 2);
                for (var j = 0; j < actualCellCount; j++)
                {
                    textureIndices.Add(new BlendTileTextureIndex
                    {
                        TextureIndex = i,
                        Offset = j
                    });
                }

                actualCellIndex += actualCellCount;
            }

            var cellTextureIndices = new int[width, height];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    cellTextureIndices[x, y] = GetTextureIndex(tiles[x, y], textures);
                }
            }

            var unknown1 = reader.ReadUInt32();
            if (unknown1 != 0)
            {
                throw new InvalidDataException();
            }

            var unknown2 = reader.ReadUInt32();
            if (unknown2 != 0)
            {
                throw new InvalidDataException();
            }

            var blends = new BlendInfo[blendsCount];
            for (var i = 0; i < blendsCount; i++)
            {
                blends[i] = BlendInfo.Parse(reader);
            }

            // TODO: Cliff blends.

            return new BlendTileData
            {
                NumTiles = numTiles,
                Tiles = tiles,
                BlendIndices = blendIndices,
                SingleEdgeBlends = singleEdgeBlends,
                CliffBlends = cliffBlends,

                Passability = passability,

                Textures = textures,

                TextureIndices = textureIndices.ToArray(),

                Blends = blends,
            };
        }

        private static int GetTextureIndex(ushort tileValue, BlendTileTexture[] textures)
        {
            var actualCellIndex = 0u;
            for (var i = 0; i < textures.Length; i++)
            {
                var texture = textures[i];

                var actualCellCount = (texture.CellSize * 2) * (texture.CellSize * 2);
                if (tileValue < actualCellIndex + actualCellCount)
                    return i;

                actualCellIndex += actualCellCount;
            }

            throw new InvalidDataException();
        }
    }

    public enum Passability : byte
    {
        Passable = 0,
        Impassable,
        ImpassableToPlayers,
        ImpassableToAirUnits,
        ExtraPassable
    }
}
