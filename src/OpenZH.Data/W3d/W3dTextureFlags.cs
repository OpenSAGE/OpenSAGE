using System;

namespace OpenZH.Data.W3d
{
    [Flags]
    public enum W3dTextureFlags : ushort
    {
        /// <summary>
        /// this texture should be "published" (indirected so its changeable in code)
        /// </summary>
        Publish = 0x0001,

        /// <summary>
        /// this texture should be resizeable (OBSOLETE!!!)
        /// </summary>
        ResizeObsolete = 0x0002,

        /// <summary>
        /// this texture should not have any LOD (mipmapping or resizing)
        /// </summary>
        NoLod = 0x0004,

        /// <summary>
        /// this texture should be clamped on U
        /// </summary>
        ClampU = 0x0008,

        /// <summary>
        /// this texture should be clamped on V
        /// </summary>
        ClampV = 0x0010,

        /// <summary>
        /// this texture's alpha channel should be collapsed to one bit
        /// </summary>
        AlphaBitmap = 0x0020,



        /// <summary>
        /// generate all mip-levels
        /// </summary>
        MipLevelsAll = 0x0000,

        /// <summary>
        /// generate up to 2 mip-levels (NOTE: use W3DTEXTURE_NO_LOD to generate just 1 mip-level)
        /// </summary>
        MipLevels2 = 0x0040,

        /// <summary>
        /// generate up to 3 mip-levels
        /// </summary>
        MipLevels3 = 0x0080,

        /// <summary>
        /// generate up to 4 mip-levels
        /// </summary>
        MipLevels4 = 0x00c0,



        /// <summary>
        /// base texture
        /// </summary>
        HintBase = 0x0000 << 1,

        /// <summary>
        /// emissive map
        /// </summary>
        HintEmissive = 0x0100 << 1,

        /// <summary>
        /// environment/reflection map
        /// </summary>
        HintEnvironment = 0x0200 << 1,

        /// <summary>
        /// shinyness mask map
        /// </summary>
        HintShinyMask = 0x0300 << 1,



        /// <summary>
        /// Color map.
        /// </summary>
        TypeColorMap = 0x0000,

        /// <summary>
        /// Grayscale heightmap (to be converted to bumpmap).
        /// </summary>
        TypeBumpMap = 0x1000
    }
}
