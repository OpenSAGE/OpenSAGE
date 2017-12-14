namespace OpenSage.LowLevel.Graphics3D.Util.BlockCompression
{
    internal sealed class Bc2 : BcBase
    {
        public override int BytesPerBlock => 16;

        public override void DecompressBlock(
            byte[] data,
            int startIndex,
            byte[] result,
            int resultX,
            int resultY,
            int resultStride)
        {
            var y = resultY;
            var x = resultX;
            for (var i = 0; i < 8; i++)
            {
                var byteValue = data[startIndex + i];

                // 2 x 4-bit alpha values in each byte.
                for (var j = 0; j < 2; j++)
                {
                    var packedAlphaValue = (byteValue >> (j * 4)) & 0b1111;
                    var alphaValue = (byte) (((packedAlphaValue * 255) + 7) / 15.0);

                    result[(y * resultStride) + (x + 3)] = alphaValue;
                }

                x += 4;
                if (i > 0 && i % 2 == 0)
                {
                    x = resultX;
                }

                y++;
            }

            DecompressColors(
                data,
                startIndex + 8,
                false,
                result,
                resultX,
                resultY,
                resultStride);
        }
    }
}