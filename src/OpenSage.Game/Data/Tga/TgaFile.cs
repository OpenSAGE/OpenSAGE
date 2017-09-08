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

                if (header.HasColorMap || header.ImageType != TgaImageType.UncompressedRgb)
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
    }
}
