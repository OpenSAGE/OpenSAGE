using System;
using System.IO;
using System.Text;

namespace OpenSage.Data.Tga
{
    public sealed class TgaFile
    {
        public TgaHeader Header { get; private set; }
        public byte[] Data { get; private set; }

        public static TgaFile FromStream(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                var header = TgaHeader.Parse(reader);

                if (header.HasColorMap || header.ImageType != TgaImageType.UncompressedRgb)
                {
                    throw new NotSupportedException();
                }

                var data = reader.ReadBytes(header.Width * header.Height * header.ImagePixelSize);

                return new TgaFile
                {
                    Header = header,
                    Data = data
                };
            }
        }
    }
}
