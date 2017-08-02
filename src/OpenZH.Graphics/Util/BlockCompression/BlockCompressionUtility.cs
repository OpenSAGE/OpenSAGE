using System;

namespace OpenZH.Graphics.Util.BlockCompression
{
    internal static class BlockCompressionUtility
    {
        private static readonly BcBase Bc1 = new Bc1();
        private static readonly BcBase Bc2 = new Bc2();
        private static readonly BcBase Bc3 = new Bc3();

        public static byte[] Decompress(PixelFormat format, byte[] data, int rowPitch, out int decompressedRowPitch)
        {
            BcBase bc;
            switch (format)
            {
                case PixelFormat.Bc1:
                    bc = Bc1;
                    break;

                case PixelFormat.Bc2:
                    bc = Bc2;
                    break;

                case PixelFormat.Bc3:
                    bc = Bc3;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(format));
            }

            const int pixelsPerBlockSide = 4;
            const int pixelsPerBlock = pixelsPerBlockSide * pixelsPerBlockSide;

            var bytesPerBlock = bc.BytesPerBlock;

            var numBlocks = data.Length / bytesPerBlock;
            var numPixels = numBlocks * pixelsPerBlock;

            var result = new byte[numPixels * ColorRgba.SizeInBytes];
            decompressedRowPitch = (rowPitch / bytesPerBlock) * pixelsPerBlockSide * ColorRgba.SizeInBytes;

            var resultX = 0;
            var resultY = 0;

            for (var i = 0; i < data.Length; i += bytesPerBlock)
            {
                bc.DecompressBlock(data, i, result, resultX, resultY, decompressedRowPitch);

                resultX += pixelsPerBlockSide;

                if (i > 0 && resultX % rowPitch == 0)
                {
                    resultX = 0;
                    resultY += pixelsPerBlockSide;
                }
            }

            return result;
        }
    }
}
