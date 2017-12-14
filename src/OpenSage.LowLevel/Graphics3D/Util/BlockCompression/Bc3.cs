namespace OpenSage.LowLevel.Graphics3D.Util.BlockCompression
{
    internal sealed class Bc3 : BcBase
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
            var alpha0 = data[startIndex + 0];
            var alpha1 = data[startIndex + 1];

            byte alpha2, alpha3, alpha4, alpha5, alpha6, alpha7;
            if (alpha0 > alpha1)
            {
                // 6 interpolated alpha values.
                alpha2 = (byte) ((6.0 / 7.0) * alpha0 + (1.0 / 7.0) * alpha1); // bit code 010
                alpha3 = (byte) ((5.0 / 7.0) * alpha0 + (2.0 / 7.0) * alpha1); // bit code 011
                alpha4 = (byte) ((4.0 / 7.0) * alpha0 + (3.0 / 7.0) * alpha1); // bit code 100
                alpha5 = (byte) ((3.0 / 7.0) * alpha0 + (4.0 / 7.0) * alpha1); // bit code 101
                alpha6 = (byte) ((2.0 / 7.0) * alpha0 + (5.0 / 7.0) * alpha1); // bit code 110
                alpha7 = (byte) ((1.0 / 7.0) * alpha0 + (6.0 / 7.0) * alpha1); // bit code 111
            }
            else
            {
                // 4 interpolated alpha values.
                alpha2 = (byte) ((4.0 / 5.0) * alpha0 + (1.0 / 5.0) * alpha1); // bit code 010
                alpha3 = (byte) ((3.0 / 5.0) * alpha0 + (2.0 / 5.0) * alpha1); // bit code 011
                alpha4 = (byte) ((2.0 / 5.0) * alpha0 + (3.0 / 5.0) * alpha1); // bit code 100
                alpha5 = (byte) ((1.0 / 5.0) * alpha0 + (4.0 / 5.0) * alpha1); // bit code 101
                alpha6 = 0;                                                    // bit code 110
                alpha7 = 255;                                                  // bit code 111
            }

            var alphas = new[]
            {
                alpha0,
                alpha1,
                alpha2,
                alpha3,
                alpha4,
                alpha5,
                alpha6,
                alpha7
            };

            var y = resultY;
            for (var i = 2; i < 8; i += 3)
            {
                var uintValue = data[startIndex + i]
                    | (data[startIndex + i + 1] << 8)
                    | (data[startIndex + i + 2] << 16);

                // 8 x 3-bit index values in each set of 24 bits.
                var x = resultX;
                for (var j = 0; j < 8; j++)
                {
                    var indexValue = (uintValue >> (j * 3)) & 0b111;

                    var alpha = alphas[indexValue];

                    result[(y * resultStride) + (x + 3)] = alpha;

                    x += 4;
                    if (j > 0 && j % 4 == 0)
                    {
                        x = resultX;
                        y++;
                    }
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