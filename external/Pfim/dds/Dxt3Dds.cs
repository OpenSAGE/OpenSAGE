using System.IO;

namespace Pfim
{
    internal class Dxt3Dds : CompressedDds
    {
        const byte PIXEL_DEPTH = 4;
        const byte DIV_SIZE = 4;
        private static DdsLoadInfo loadInfoDXT3 = new DdsLoadInfo(true, false, false, 4, 16, 32);

        protected override byte PixelDepth { get { return PIXEL_DEPTH; } }

        protected override int Decode(byte[] stream, byte[] data, int streamIndex, uint dataIndex, uint width)
        {
            /* 
             * Strategy for decompression:
             * -We're going to decode both alpha and color at the same time 
             * to save on space and time as we don't have to allocate an array 
             * to store values for later use.
             */

            // Remember where the alpha data is stored so we can decode simultaneously
            int alphaPtr = streamIndex;

            // Jump ahead to the color data
            streamIndex += 8;

            // Colors are stored in a pair of 16 bits
            ushort color0 = stream[streamIndex++];
            color0 |= (ushort)(stream[streamIndex++] << 8);

            ushort color1 = (stream[streamIndex++]);
            color1 |= (ushort)(stream[streamIndex++] << 8);

            // Extract R5G6B5 (in that order)
            byte r0 = (byte)((color0 & 0x1f));
            byte g0 = (byte)((color0 & 0x7E0) >> 5);
            byte b0 = (byte)((color0 & 0xF800) >> 11);
            r0 = (byte)(r0 << 3 | r0 >> 2);
            g0 = (byte)(g0 << 2 | g0 >> 3);
            b0 = (byte)(b0 << 3 | b0 >> 2);

            byte r1 = (byte)((color1 & 0x1f));
            byte g1 = (byte)((color1 & 0x7E0) >> 5);
            byte b1 = (byte)((color1 & 0xF800) >> 11);
            r1 = (byte)(r1 << 3 | r1 >> 2);
            g1 = (byte)(g1 << 2 | g1 >> 3);
            b1 = (byte)(b1 << 3 | b1 >> 2);

            // Used the two extracted colors to create two new colors
            // that are slightly different.
            byte r2 = (byte)((2 * r0 + r1) / 3);
            byte g2 = (byte)((2 * g0 + g1) / 3);
            byte b2 = (byte)((2 * b0 + b1) / 3);

            byte r3 = (byte)((r0 + 2 * r1) / 3);
            byte g3 = (byte)((g0 + 2 * g1) / 3);
            byte b3 = (byte)((b0 + 2 * b1) / 3);

            byte rowVal = 0;
            ushort rowAlpha;
            for (int i = 0; i < 4; i++)
            {
                rowVal = stream[streamIndex++];

                // Each row of rgb values have 4 alpha values that  are
                // encoded in 4 bits
                rowAlpha = stream[alphaPtr++];
                rowAlpha |= (ushort)(stream[alphaPtr++] << 8);

                for (int j = 0; j < 8; j += 2)
                {
                    byte currentAlpha = (byte)((rowAlpha >> (j * 2)) & 0x0f);
                    currentAlpha |= (byte)(currentAlpha << 4);

                    // index code
                    switch (((rowVal >> j) & 0x03))
                    {
                        case 0:
                            data[dataIndex++] = r0;
                            data[dataIndex++] = g0;
                            data[dataIndex++] = b0;
                            data[dataIndex++] = currentAlpha;
                            break;
                        case 1:
                            data[dataIndex++] = r1;
                            data[dataIndex++] = g1;
                            data[dataIndex++] = b1;
                            data[dataIndex++] = currentAlpha;
                            break;
                        case 2:
                            data[dataIndex++] = r2;
                            data[dataIndex++] = g2;
                            data[dataIndex++] = b2;
                            data[dataIndex++] = currentAlpha;
                            break;
                        case 3:
                            data[dataIndex++] = r3;
                            data[dataIndex++] = g3;
                            data[dataIndex++] = b3;
                            data[dataIndex++] = currentAlpha;
                            break;
                    }
                }
                dataIndex += PIXEL_DEPTH * (width - DIV_SIZE);
            }
            return streamIndex;
        }

        public override DdsLoadInfo ImageInfo(DdsHeader header)
        {
            return loadInfoDXT3;
        }
    }
}
