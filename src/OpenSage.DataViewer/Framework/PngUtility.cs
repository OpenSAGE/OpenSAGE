using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenSage.DataViewer.Framework
{
    internal static class PngUtility
    {
        public static MemoryStream ConvertToPng(byte[] rgbaData, int width, int height)
        {
            var outputStream = new MemoryStream();
            var image = Image.LoadPixelData<Rgba32>(rgbaData, width, height);
            image.SaveAsPng(outputStream);

            return outputStream;           
        }
    }
}
