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

            int numColorTableEntries;
            switch (infoHeader.BitCount)
            {
                case 4:
                case 8:
                    numColorTableEntries = (int) MathF.Pow(2, infoHeader.BitCount);
                    break;

                case 24:
                case 32:
                    numColorTableEntries = (int) infoHeader.ColorsUsed;
                    break;

                default:
                    throw new NotSupportedException();
            }

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

        public byte[] GetBgraPixels()
        {
            var width = Header.Width;
            var height = Header.Height / 2;

            var pixels = new byte[width * height * 4];

            var dstIndex = 0;
            var andMaskIndex = 0;
            var xorMaskIndex = 0;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var alpha = 255;

                    byte red, green, blue;

                    switch (Header.BitCount)
                    {
                        case 4:
                        case 8:
                            var color = ColorTable.Entries[XorMask.Pixels[xorMaskIndex++]];
                            blue = color.Blue;
                            green = color.Green;
                            red = color.Red;
                            break;

                        case 24:
                            red = XorMask.Pixels[xorMaskIndex++];
                            green = XorMask.Pixels[xorMaskIndex++];
                            blue = XorMask.Pixels[xorMaskIndex++];
                            break;

                        case 32:
                            red = XorMask.Pixels[xorMaskIndex++];
                            green = XorMask.Pixels[xorMaskIndex++];
                            blue = XorMask.Pixels[xorMaskIndex++];
                            alpha = XorMask.Pixels[xorMaskIndex++];
                            break;

                        default:
                            throw new InvalidOperationException();
                    }

                    pixels[dstIndex++] = blue;
                    pixels[dstIndex++] = green;
                    pixels[dstIndex++] = red;

                    var isTransparent = AndMask.Pixels[andMaskIndex++] == 1;

                    pixels[dstIndex++] = isTransparent ? (byte) 0 : (byte) alpha;
                }
            }

            return pixels;
        }
    }
}
