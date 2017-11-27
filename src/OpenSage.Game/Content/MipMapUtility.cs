using System;
using LLGfx;

namespace OpenSage.Content
{
    internal static class MipMapUtility
    {
        public static int CalculateMipMapCount(int width, int height)
        {
            return 1 + (int) Math.Floor(Math.Log(Math.Max(width, height), 2));
        }

        public static TextureMipMapData[] GenerateMipMaps(
            int width,
            int height,
            byte[] rgbaData)
        {
            var numLevels = CalculateMipMapCount(width, height);

            var mipMapData = new TextureMipMapData[numLevels];

            int previousWidth = -1, previousHeight = -1;
            for (int level = 0; level < numLevels; level++)
            {
                if (level > 0)
                {
                    var levelData = new byte[width * height * ByteRgba.BytesPerPixel];

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int previousLevelX = x * (previousWidth / width);
                            int previousLevelY = y * (previousHeight / height);

                            var moreDetailedMipLevel = mipMapData[level - 1];
                            var c00 = ByteRgba.FromByteArray(moreDetailedMipLevel.Data, previousLevelX, previousLevelY, previousWidth);
                            var c10 = ByteRgba.FromByteArray(moreDetailedMipLevel.Data, ClampToDimension(previousLevelX + 1, previousWidth), previousLevelY, previousWidth);
                            var c01 = ByteRgba.FromByteArray(moreDetailedMipLevel.Data, previousLevelX, ClampToDimension(previousLevelY + 1, previousHeight), previousWidth);
                            var c11 = ByteRgba.FromByteArray(moreDetailedMipLevel.Data, ClampToDimension(previousLevelX + 1, previousWidth), ClampToDimension(previousLevelY + 1, previousHeight), previousWidth);
                            var interpolatedColor = ByteRgba.Average(c00, c10, c01, c11);

                            interpolatedColor.CopyTo(levelData, x, y, width);
                        }
                    }

                    mipMapData[level] = new TextureMipMapData
                    {
                        Data = levelData,
                        BytesPerRow = width * ByteRgba.BytesPerPixel
                    };
                }
                else
                {
                    mipMapData[level] = new TextureMipMapData
                    {
                        Data = rgbaData,
                        BytesPerRow = width * ByteRgba.BytesPerPixel
                    };
                }

                previousWidth = width;
                previousHeight = height;

                width = Math.Max((int) Math.Floor(width / 2.0), 1);
                height = Math.Max((int) Math.Floor(height / 2.0), 1);
            }

            return mipMapData;
        }

        /// <summary>
        /// Takes care of non-power-of-two textures not necessarily having double the size in all dimensions.
        /// </summary>
        private static int ClampToDimension(int value, int dimension)
        {
            return Math.Min(value, dimension - 1);
        }

        private struct ByteRgba
        {
            public const int BytesPerPixel = 4;

            public byte R;
            public byte G;
            public byte B;
            public byte A;

            public static ByteRgba FromByteArray(byte[] data, int x, int y, int width)
            {
                var index = (y * width * BytesPerPixel) + (x * BytesPerPixel);

                return new ByteRgba
                {
                    R = data[index + 0],
                    G = data[index + 1],
                    B = data[index + 2],
                    A = data[index + 3],
                };
            }

            public static ByteRgba Average(ByteRgba v0, ByteRgba v1, ByteRgba v2, ByteRgba v3)
            {
                return new ByteRgba
                {
                    R = (byte) ((v0.R + v1.R + v2.R + v3.R) / 4),
                    G = (byte) ((v0.G + v1.G + v2.G + v3.G) / 4),
                    B = (byte) ((v0.B + v1.B + v2.B + v3.B) / 4),
                    A = (byte) ((v0.A + v1.A + v2.A + v3.A) / 4)
                };
            }

            public void CopyTo(byte[] data, int x, int y, int width)
            {
                var index = (y * width * BytesPerPixel) + (x * BytesPerPixel);

                data[index + 0] = R;
                data[index + 1] = G;
                data[index + 2] = B;
                data[index + 3] = A;
            }
        }
    }
}
