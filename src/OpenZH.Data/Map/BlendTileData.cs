using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public sealed class BlendTileData
    {
        public static BlendTileData Parse(BinaryReader reader)
        {
            var numTiles = reader.ReadUInt32();

            var tiles = reader.ReadUInt16Array(numTiles);
            var blends = reader.ReadUInt16Array(numTiles);
            var singleEdgeBlends = reader.ReadUInt16Array(numTiles);
            var cliffBlends = reader.ReadUInt16Array(numTiles);

            var passability = reader.ReadSingleBitBooleanArray(numTiles);

            //var unknown1 = reader.ReadSingleBitBooleanArray(numTiles);
            //var unknown2 = reader.ReadSingleBitBooleanArray(numTiles);

            //var passageWidth = reader.ReadSingleBitBooleanArray(numTiles);

            //var unknown3 = reader.ReadSingleBitBooleanArray(numTiles);

            //var visibility = reader.ReadSingleBitBooleanArray(numTiles);
            //var buildability = reader.ReadSingleBitBooleanArray(numTiles);

            //var unknown4 = reader.ReadSingleBitBooleanArray(numTiles);

            //// Tiberium growability?
            //var tiberiumGrowability = reader.ReadSingleBitBooleanArray(numTiles);

            var textureCellCount = reader.ReadUInt32();
            var blendsCount = reader.ReadUInt32() - 1;
            var cliffBlendsCount = reader.ReadUInt32() - 1;

            var textureCount = reader.ReadUInt32();
            var textures = new BlendTileTexture[textureCount];
            for (var i = 0; i < textureCount; i++)
            {
                textures[i] = BlendTileTexture.Parse(reader);
            }

            var unknown5 = reader.ReadUInt32();
            var unknown6 = reader.ReadUInt32();

            return new BlendTileData();
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
