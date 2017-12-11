using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenSage.DataViewer.Framework
{
    internal static class BmpUtility
    {
        public static byte[] PrependBmpHeader(byte[] rgbaData, int width, int height)
        {
            var pixels = new Argb32[width * height];
            var pixelIndex = 0;
            for (var i = 0; i < rgbaData.Length; i += 4)
            {
                pixels[pixelIndex++] = new Argb32(
                    rgbaData[i + 0],
                    rgbaData[i + 1],
                    rgbaData[i + 2],
                    rgbaData[i + 3]);
            }

            using (var outputStream = new MemoryStream())
            {
                var image = Image.LoadPixelData(pixels, width, height);
                image.SaveAsBmp(outputStream);

                return outputStream.ToArray();
            }
        }
    }
}
