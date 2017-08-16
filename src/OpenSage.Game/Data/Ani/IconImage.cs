using System;
using System.IO;

namespace OpenSage.Data.Ani
{
    public sealed class IconImage
    {
        public BmpInfoHeader Header { get; private set; }
        public BmpColorTable ColorTable { get; private set; }
        public BmpRasterData XorMask { get; private set; }
        public BmpRasterData AndMask { get; private set; }

        internal static IconImage Parse(BinaryReader reader)
        {
            var infoHeader = BmpInfoHeader.Parse(reader);

            if (infoHeader.Compression != 0)
            {
                throw new NotSupportedException();
            }
            if (infoHeader.BitCount != 4)
            {
                throw new NotSupportedException();
            }

            var numColorTableEntries = (int) Math.Pow(2, infoHeader.BitCount);
            var colorTable = BmpColorTable.Parse(reader, numColorTableEntries);

            var xorMask = BmpRasterData.Parse(reader, infoHeader.Width, infoHeader.Height / 2, infoHeader.BitCount);
            var andMask = BmpRasterData.Parse(reader, infoHeader.Width, infoHeader.Height / 2, 1);

            return new IconImage
            {
                Header = infoHeader,
                ColorTable = colorTable,
                XorMask = xorMask,
                AndMask = andMask
            };
        }
    }
}
