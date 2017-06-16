namespace Pfim
{
    /// <summary>Contains additional info about the image</summary>
    internal struct DdsLoadInfo
    {
        internal bool compressed;
        internal bool swap;
        internal bool palette;

        /// <summary>
        /// The length of a block is in pixels.
        /// This mainly affects compressed images as they are
        /// encoded in blocks that are divSize by divSize.
        /// Uncompressed DDS do not need this value.
        /// </summary>
        internal uint divSize;

        /// <summary>
        /// Number of bytes needed to decode block of pixels
        /// that is divSize by divSize.  This takes into account
        /// how many bytes it takes to extract color and alpha information.
        /// Uncompressed DDS do not need this value.
        /// </summary>
        internal uint blockBytes;

        internal int depth;

        /// <summary>Initialize the load info structure</summary>
        public DdsLoadInfo(bool isCompresed, bool isSwap, bool isPalette, uint aDivSize, uint aBlockBytes, int aDepth)
        {
            compressed = isCompresed;
            swap = isSwap;
            palette = isPalette;
            divSize = aDivSize;
            blockBytes = aBlockBytes;
            depth = aDepth;
        }
    }
}
