using System;
using System.IO;
using System.Text;

namespace OpenSage.Data.Tga
{
    public sealed class TgaFile
    {
        public TgaHeader Header { get; private set; }
        public byte[] Data { get; private set; }

        public static TgaFile FromFileSystemEntry(FileSystemEntry entry)
        {
            using (var stream = entry.Open())
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                var header = TgaHeader.Parse(reader);

                if (header.HasColorMap || (header.ImageType != TgaImageType.UncompressedRgb && header.ImageType != TgaImageType.UncompressedBlackAndWhite))
                {
                    throw new NotSupportedException();
                }

                var bytesPerPixel = header.ImagePixelSize / 8;
                var stride = header.Width * bytesPerPixel;

                var data = reader.ReadBytes(header.Width * header.Height * bytesPerPixel);

                // Invert y, because data is stored bottom-up in TGA.
                var invertedData = new byte[data.Length];
                var invertedY = 0;
                for (var y = header.Height - 1; y >= 0; y--)
                {
                    Array.Copy(
                        data, 
                        y * stride,
                        invertedData, 
                        invertedY * stride,
                        stride);

                    invertedY++;
                }

                return new TgaFile
                {
                    Header = header,
                    Data = invertedData
                };
            }
        }

        public static int GetSquareTextureSize(FileSystemEntry entry)
        {
            using (var stream = entry.Open())
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                var header = TgaHeader.Parse(reader);

                if (header.Width != header.Height)
                {
                    throw new InvalidOperationException();
                }

                return header.Width;
            }
        }

        public static byte[] ConvertPixelsToRgba8(TgaFile tgaFile)
        {
            var pixelSize = tgaFile.Header.ImagePixelSize;
            var data = tgaFile.Data;

            switch (pixelSize)
            {
                case 8: // Grayscale
                    {
                        var result = new byte[data.Length * 4];
                        var resultIndex = 0;
                        for (var i = 0; i < data.Length; i++)
                        {
                            result[resultIndex++] = data[i]; // R
                            result[resultIndex++] = data[i]; // G
                            result[resultIndex++] = data[i]; // B
                            result[resultIndex++] = 255;     // A
                        }
                        return result;
                    }

                case 24: // BGR
                    {
                        var result = new byte[data.Length / 3 * 4];
                        var resultIndex = 0;
                        for (var i = 0; i < data.Length; i += 3)
                        {
                            result[resultIndex++] = data[i + 2]; // R
                            result[resultIndex++] = data[i + 1]; // G
                            result[resultIndex++] = data[i + 0]; // B
                            result[resultIndex++] = 255;         // A
                        }
                        return result;
                    }

                case 32: // BGRA
                    {
                        var result = new byte[data.Length];
                        var resultIndex = 0;
                        for (var i = 0; i < data.Length; i += 4)
                        {
                            result[resultIndex++] = data[i + 2]; // R
                            result[resultIndex++] = data[i + 1]; // G
                            result[resultIndex++] = data[i + 0]; // B
                            result[resultIndex++] = data[i + 3]; // A
                        }
                        return result;
                    }

                default:
                    throw new ArgumentOutOfRangeException(nameof(pixelSize));
            }
        }
    }
}
