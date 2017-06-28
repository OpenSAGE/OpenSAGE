using System;
using System.IO;

namespace OpenZH.Data.Tga
{
    public sealed class TgaFile
    {
        public TgaHeader Header { get; private set; }
        public byte[] Data { get; private set; }

        public static TgaFile Parse(BinaryReader reader)
        {
            var header = TgaHeader.Parse(reader);

            if (header.HasColorMap || header.ImageType != TgaImageType.UncompressedRgb)
                throw new NotSupportedException();

            var data = reader.ReadBytes(header.Width * header.Height * header.ImagePixelSize);

            return new TgaFile
            {
                Header = header,
                Data = data
            };
        }
    }
}
