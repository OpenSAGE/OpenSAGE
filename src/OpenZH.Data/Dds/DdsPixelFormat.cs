using System.IO;

namespace OpenZH.Data.Dds
{
    public sealed class DdsPixelFormat
    {
        public uint Size { get; private set; }
        public DdsPixelFormatFlags Flags { get; private set; }
        public uint FourCc { get; private set; }
        public uint RgbBitCount { get; private set; }
        public uint RBitMask { get; private set; }
        public uint GBitMask { get; private set; }
        public uint BBitMask { get; private set; }
        public uint ABitMask { get; private set; }

        internal static DdsPixelFormat Parse(BinaryReader reader)
        {
            return new DdsPixelFormat
            {
                Size = reader.ReadUInt32(),
                Flags = (DdsPixelFormatFlags) reader.ReadUInt32(),
                FourCc = reader.ReadUInt32(),
                RgbBitCount = reader.ReadUInt32(),
                RBitMask = reader.ReadUInt32(),
                GBitMask = reader.ReadUInt32(),
                BBitMask = reader.ReadUInt32(),
                ABitMask = reader.ReadUInt32()
            };
        }
    }
}
