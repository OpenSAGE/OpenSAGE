using System;
using System.Collections.Generic;
using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public sealed class BlendTileData : Asset
    {
        public const string AssetName = "BlendTileData";

        public uint NumTiles { get; private set; }

        public ushort[,] Tiles { get; private set; }
        public ushort[,] Blends { get; private set; }
        public ushort[,] ThreeWayBlends { get; private set; }
        public ushort[,] CliffTextures { get; private set; }

        public bool[,] Passability { get; private set; }

        public uint TextureCellCount { get; private set; }

        public BlendTileTexture[] Textures { get; private set; }

        public uint MagicValue1 { get; private set; }
        public uint MagicValue2 { get; private set; }

        public BlendDescription[] BlendDescriptions { get; private set; }

        /// <summary>
        /// When there aren't any cliff blends, some maps have 0 and some have 1.
        /// We need to keep the parsed value around so we can roundtrip correctly.
        /// </summary>
        public uint ParsedCliffTextureMappingsCount { get; private set; }

        public CliffTextureMapping[] CliffTextureMappings { get; private set; }

        private List<BlendTileTextureIndex> _textureIndices;

        /// <summary>
        /// Derived data.
        /// </summary>
        public IReadOnlyList<BlendTileTextureIndex> TextureIndices
        {
            get
            {
                if (_textureIndices == null)
                {
                    _textureIndices = CalculateTextureIndices();
                }
                return _textureIndices;
            }
        }

        public static BlendTileData Parse(BinaryReader reader, MapParseContext context, HeightMapData heightMapData)
        {
            return ParseAsset(reader, context, version =>
            {
                if (version < 6 || version > 8)
                {
                    throw new InvalidDataException();
                }

                if (heightMapData == null)
                {
                    throw new InvalidDataException("Expected HeightMapData asset before BlendTileData asset.");
                }

                var width = heightMapData.Width;
                var height = heightMapData.Height;

                var numTiles = reader.ReadUInt32();
                if (numTiles != width * height)
                {
                    throw new InvalidDataException();
                }

                var tiles = reader.ReadUInt16Array2D(width, height);
                var blends = reader.ReadUInt16Array2D(width, height);
                var threeWayBlends = reader.ReadUInt16Array2D(width, height);
                var cliffTextures = reader.ReadUInt16Array2D(width, height);

                bool[,] passability = null;
                if (version > 6)
                {
                    var passabilityWidth = heightMapData.Width;
                    if (version == 7)
                    {
                        // C&C Generals clips partial bytes from each row of passability data,
                        // if the border width is large enough to fully contain the clipped data.
                        if (passabilityWidth % 8 <= 6 && passabilityWidth % 8 <= heightMapData.BorderWidth)
                        {
                            passabilityWidth -= passabilityWidth % 8;
                        }
                    }

                    // If terrain is passable, there's a 0 in the data file.
                    passability = reader.ReadSingleBitBooleanArray2D(passabilityWidth, heightMapData.Height);
                }

                var textureCellCount = reader.ReadUInt32();

                var blendsCount = reader.ReadUInt32();
                if (blendsCount > 0)
                {
                    // Usually minimum value is 1, but some files (perhaps Generals, not Zero Hour?) have 0.
                    blendsCount--;
                }

                var parsedCliffBlendsCount = reader.ReadUInt32();
                var cliffBlendsCount = parsedCliffBlendsCount;
                if (cliffBlendsCount > 0)
                {
                    // Usually minimum value is 1, but some files (perhaps Generals, not Zero Hour?) have 0.
                    cliffBlendsCount--;
                }

                var textureCount = reader.ReadUInt32();
                var textures = new BlendTileTexture[textureCount];
                for (var i = 0; i < textureCount; i++)
                {
                    textures[i] = BlendTileTexture.Parse(reader);
                }

                var magicValue1 = reader.ReadUInt32();
                if (magicValue1 != 0)
                {
                    throw new InvalidDataException();
                }

                var magicValue2 = reader.ReadUInt32();
                if (magicValue2 != 0)
                {
                    throw new InvalidDataException();
                }

                var blendDescriptions = new BlendDescription[blendsCount];
                for (var i = 0; i < blendsCount; i++)
                {
                    blendDescriptions[i] = BlendDescription.Parse(reader);
                }

                var cliffTextureMappings = new CliffTextureMapping[cliffBlendsCount];
                for (var i = 0; i < cliffBlendsCount; i++)
                {
                    cliffTextureMappings[i] = CliffTextureMapping.Parse(reader);
                }

                return new BlendTileData
                {
                    NumTiles = numTiles,

                    Tiles = tiles,

                    Blends = blends,
                    ThreeWayBlends = threeWayBlends,
                    CliffTextures = cliffTextures,

                    Passability = passability,

                    TextureCellCount = textureCellCount,

                    Textures = textures,

                    MagicValue1 = magicValue1,
                    MagicValue2 = magicValue2,

                    BlendDescriptions = blendDescriptions,

                    ParsedCliffTextureMappingsCount = parsedCliffBlendsCount,
                    CliffTextureMappings = cliffTextureMappings
                };
            });
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

        private List<BlendTileTextureIndex> CalculateTextureIndices()
        {
            // Calculate texture index + offset within texture.
            var textureIndices = new List<BlendTileTextureIndex>();
            var actualCellIndex = 0u;
            for (var i = 0; i < Textures.Length; i++)
            {
                var texture = Textures[i];

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

            return textureIndices;
        }

        public void WriteTo(BinaryWriter writer)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write(NumTiles);

                writer.WriteUInt16Array2D(Tiles);
                writer.WriteUInt16Array2D(Blends);
                writer.WriteUInt16Array2D(ThreeWayBlends);
                writer.WriteUInt16Array2D(CliffTextures);

                if (Version > 6)
                {
                    writer.WriteSingleBitBooleanArray2D(Passability);
                }

                writer.Write(TextureCellCount);

                writer.Write((uint) BlendDescriptions.Length + 1);

                writer.Write(ParsedCliffTextureMappingsCount);

                writer.Write((uint) Textures.Length);

                foreach (var texture in Textures)
                {
                    texture.WriteTo(writer);
                }

                writer.Write(MagicValue1);
                writer.Write(MagicValue2);

                foreach (var blendDescription in BlendDescriptions)
                {
                    blendDescription.WriteTo(writer);
                }

                foreach (var cliffTextureMapping in CliffTextureMappings)
                {
                    cliffTextureMapping.WriteTo(writer);
                }
            });
        }
    }
}
