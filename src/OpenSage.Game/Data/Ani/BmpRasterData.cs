using System;
using System.IO;

namespace OpenSage.Data.Ani
{
    public sealed class BmpRasterData
    {
        public byte[] Pixels { get; private set; }

        internal static BmpRasterData Parse(BinaryReader reader, int width, int height, int bitsPerPixel)
        {
            var pixels = new byte[width * height];

            for (var y = 0; y < height; y++)
            {
                var readBytes = 0;

                var numValuesPerByte = 8 / bitsPerPixel;
                var bitMask = 1 << (bitsPerPixel - 1);

                for (var x = 0; x < width; x += numValuesPerByte)
                {
                    var byteValue = reader.ReadByte();
                    for (var i = 0; i < numValuesPerByte; i++)
                    {
                        var shift = 8 - ((i + 1) * bitsPerPixel);

                        // Invert y, because DIB data is stored bottom-to-top.
                        pixels[((height - 1 - y) * width) + x + i] = (byte) ((byteValue >> shift) & bitMask);
                    }
                    readBytes++;
                }

                var paddingBytes = (readBytes % 4 != 0)
                        ? 4 - (readBytes % 4)
                        : 0;
                reader.ReadBytes(paddingBytes);
            }

            return new BmpRasterData
            {
                Pixels = pixels
            };
        }
    }
}
