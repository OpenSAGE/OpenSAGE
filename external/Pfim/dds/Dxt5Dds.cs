using System.IO;

namespace Pfim
{
    internal class Dxt5Dds : CompressedDds
    {
        private static DdsLoadInfo loadInfoDXT5 = new DdsLoadInfo(true, false, false, 4, 16, 32);
        const byte PIXEL_DEPTH = 4;
        const byte DIV_SIZE = 4;

        private byte[] alpha = new byte[8];

        private int extractAlpha(byte[] workingFilePtr, int bIndex)
        {
            byte alpha0;
            byte alpha1;
            alpha[0] = alpha0 = workingFilePtr[bIndex++];
            alpha[1] = alpha1 = workingFilePtr[bIndex++];

            if (alpha0 > alpha1)
            {
                for (int i = 1; i < 7; i++)
                    alpha[1 + i] = (byte)(((7 - i) * alpha0 + i * alpha1) / 7);
            }
            else
            {
                for (int i = 1; i < 5; ++i)
                    alpha[1 + i] = (byte)(((5 - i) * alpha0 + i * alpha1) / 5);
                alpha[6] = 0;
                alpha[7] = 255;
            }
            return bIndex;
        }

        protected override byte PixelDepth { get { return PIXEL_DEPTH; } }

        protected override int Decode(byte[] stream, byte[] data, int streamIndex, uint dataIndex, uint width)
        {
            streamIndex = extractAlpha(stream, streamIndex);

            ulong alphaCodes = stream[streamIndex++];
            alphaCodes |= ((ulong)stream[streamIndex++] << 8);
            alphaCodes |= ((ulong)stream[streamIndex++] << 16);
            alphaCodes |= ((ulong)stream[streamIndex++] << 24);
            alphaCodes |= ((ulong)stream[streamIndex++] << 32);
            alphaCodes |= ((ulong)stream[streamIndex++] << 40);

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

            byte r2 = (byte)((2 * r0 + r1) / 3);
            byte g2 = (byte)((2 * g0 + g1) / 3);
            byte b2 = (byte)((2 * b0 + b1) / 3);

            byte r3 = (byte)((r0 + 2 * r1) / 3);
            byte g3 = (byte)((g0 + 2 * g1) / 3);
            byte b3 = (byte)((b0 + 2 * b1) / 3);

            byte rowVal = 0;
            for (int alphaShift = 0; alphaShift < 48; alphaShift += 12)
            {
                rowVal = stream[streamIndex++];

                for (int j = 0; j < 4; j++)
                {
                    // 3 bits determine alpha index to use
                    byte alphaIndex = (byte)((alphaCodes >> (alphaShift + 3 * j)) & 0x07);

                    // index code [0-3]
                    switch (((rowVal >> (j * 2)) & 0x03))
                    {
                        case 0:
                            data[dataIndex++] = r0;
                            data[dataIndex++] = g0;
                            data[dataIndex++] = b0;
                            data[dataIndex++] = alpha[alphaIndex];
                            break;
                        case 1:
                            data[dataIndex++] = r1;
                            data[dataIndex++] = g1;
                            data[dataIndex++] = b1;
                            data[dataIndex++] = alpha[alphaIndex];
                            break;
                        case 2:
                            data[dataIndex++] = r2;
                            data[dataIndex++] = g2;
                            data[dataIndex++] = b2;
                            data[dataIndex++] = alpha[alphaIndex];
                            break;
                        case 3:
                            data[dataIndex++] = r3;
                            data[dataIndex++] = g3;
                            data[dataIndex++] = b3;
                            data[dataIndex++] = alpha[alphaIndex];
                            break;
                    }
                }
                dataIndex += PIXEL_DEPTH * (width - DIV_SIZE);
            }
            return streamIndex;
        }

        public override DdsLoadInfo ImageInfo(DdsHeader header)
        {
            return loadInfoDXT5;
        }
    }
}
