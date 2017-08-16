namespace OpenSage.Data.Map
{
    public struct BlendTileTextureIndex
    {
        /// <summary>
        /// Index into the BlendTileData.Textures array.
        /// </summary>
        public int TextureIndex;

        /// <summary>
        /// Offset within texture to a 32x32 block. These blocks are laid out as follows
        /// (for a 128x128 texture):
        /// 
        /// | 10 | 11 | 14 | 15 |
        /// |  8 |  9 | 12 | 13 |
        /// |  2 |  3 |  6 |  7 |
        /// |  0 |  1 |  4 |  5 |
        /// </summary>
        public int Offset;
    }
}
