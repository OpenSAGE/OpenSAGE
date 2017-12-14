namespace OpenSage.LowLevel.Graphics3D.Util.BlockCompression
{
    internal abstract class BcBase
    {
        public abstract int BytesPerBlock { get; }

        public abstract void DecompressBlock(
            byte[] data, int startIndex,
            byte[] result,
            int resultX, int resultY,
            int resultStride);

        protected void DecompressColors(
            byte[] data, int startIndex,
            bool use1BitAlphaTrick,
            byte[] result,
            int resultX, int resultY,
            int resultStride)
        {
            var packedColor0 = (ushort) (data[startIndex + 0] | (data[startIndex + 1] << 8));
            var packedColor1 = (ushort) (data[startIndex + 2] | (data[startIndex + 3] << 8));

            var color0 = ColorRgba.UnpackRgbFromBgr565(packedColor0);
            var color1 = ColorRgba.UnpackRgbFromBgr565(packedColor1);

            ColorRgba color2, color3;
            if (!use1BitAlphaTrick || packedColor0 > packedColor1)
            {
                color2 = (2.0 / 3.0) * color0 + (1.0 / 3.0) * color1;
                color3 = (1.0 / 3.0) * color0 + (2.0 / 3.0) * color1;
            }
            else
            {
                color2 = 0.5 * color0 + 0.5 * color1;
                color3 = ColorRgba.TransparentBlack;
            }

            var colors = new[]
            {
                color0,
                color1,
                color2,
                color3
            };

            var y = resultY;
            for (var i = 4; i < 8; i++)
            {
                var byteValue = data[startIndex + i];

                // 4 x 2-bit index values in each byte.
                var x = resultX;
                for (var j = 0; j < 4; j++)
                {
                    var indexValue = (byteValue >> (j * 2)) & 0b11;

                    var color = colors[indexValue];

                    result[(y * resultStride) + (x + 0)] = color.R;
                    result[(y * resultStride) + (x + 1)] = color.G;
                    result[(y * resultStride) + (x + 2)] = color.B;

                    if (use1BitAlphaTrick)
                    {
                        result[(y * resultStride) + (x + 3)] = color.A;
                    }

                    x += 4;
                }

                y++;
            }
        }
    }
}