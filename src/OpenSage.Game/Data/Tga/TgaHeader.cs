using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Tga
{
    public sealed class TgaHeader
    {
        public byte IdLength { get; private set; }
        public bool HasColorMap { get; private set; }
        public TgaImageType ImageType { get; private set; }
        public short ColorMapOrigin { get; private set; }
        public short ColorMapLength { get; private set; }
        public byte ColorMapEntrySize { get; private set; }
        public short XOrigin { get; private set; }
        public short YOrigin { get; private set; }
        public short Width { get; private set; }
        public short Height { get; private set; }
        public byte ImagePixelSize { get; private set; }
        public byte ImageDescriptor { get; private set; }
        public string Id { get; private set; }

        internal static TgaHeader Parse(BinaryReader reader)
        {
            var idLength = reader.ReadByte();

            return new TgaHeader
            {
                IdLength = idLength,
                HasColorMap = reader.ReadByte() == 1,
                ImageType = (TgaImageType) reader.ReadByte(),
                ColorMapOrigin = reader.ReadInt16(),
                ColorMapLength = reader.ReadInt16(),
                ColorMapEntrySize = reader.ReadByte(),
                XOrigin = reader.ReadInt16(),
                YOrigin = reader.ReadInt16(),
                Width = reader.ReadInt16(),
                Height = reader.ReadInt16(),
                ImagePixelSize = reader.ReadByte(),
                ImageDescriptor = reader.ReadByte(),
                Id = reader.ReadFixedLengthString(idLength)
            };
        }
    }
}
