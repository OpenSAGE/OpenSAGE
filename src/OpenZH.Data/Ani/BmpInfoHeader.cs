using System.IO;

namespace OpenZH.Data.Ani
{
    public sealed class BmpInfoHeader
    {
        public uint HeaderSize { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public ushort NumPlanes { get; private set; }
        public ushort BitCount { get; private set; }
        public uint Compression { get; private set; }
        public uint ImageSizeInBytes { get; private set; }
        public uint XPixelsPerMeter { get; private set; }
        public uint YPixelsPerMeter { get; private set; }
        public uint ColorsUsed { get; private set; }
        public uint ColorsImportant { get; private set; }

        internal static BmpInfoHeader Parse(BinaryReader reader)
        {
            return new BmpInfoHeader
            {
                HeaderSize = reader.ReadUInt32(),
                Width = reader.ReadInt32(),
                Height = reader.ReadInt32(),
                NumPlanes = reader.ReadUInt16(),
                BitCount = reader.ReadUInt16(),
                Compression = reader.ReadUInt32(),
                ImageSizeInBytes = reader.ReadUInt32(),
                XPixelsPerMeter = reader.ReadUInt32(),
                YPixelsPerMeter = reader.ReadUInt32(),
                ColorsUsed = reader.ReadUInt32(),
                ColorsImportant = reader.ReadUInt32()
            };
        }
    }
}
