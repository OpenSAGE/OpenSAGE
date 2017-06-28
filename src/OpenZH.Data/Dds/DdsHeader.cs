using System.Diagnostics;
using System.IO;

namespace OpenZH.Data.Dds
{
    public sealed class DdsHeader
    {
        public uint Size { get; private set; }
        public DdsHeaderFlags Flags { get; private set; }
        public uint Height { get; private set; }
        public uint Width { get; private set; }
        public uint PitchOrLinearSize { get; private set; }
        public uint Depth { get; private set; }
        public uint MipMapCount { get; private set; }
        public DdsPixelFormat PixelFormat { get; private set; }
        public DdsCaps Caps { get; private set; }
        public DdsCaps2 Caps2 { get; private set; }

        public static DdsHeader Parse(BinaryReader reader)
        {
            var size = reader.ReadUInt32();
            Debug.Assert(size == 124);

            var flags = (DdsHeaderFlags) reader.ReadUInt32();
            var height = reader.ReadUInt32();
            var width = reader.ReadUInt32();
            var pitchOrLinearSize = reader.ReadUInt32();
            var depth = reader.ReadUInt32();
            var mipMapCount = reader.ReadUInt32();

            reader.ReadBytes(sizeof(uint) * 11); // Unused

            var pixelFormat = DdsPixelFormat.Parse(reader);

            var caps = (DdsCaps) reader.ReadUInt32();
            var caps2 = (DdsCaps2) reader.ReadUInt32();

            reader.ReadUInt32(); // Unused
            reader.ReadUInt32(); // Unused
            reader.ReadUInt32(); // Unused

            return new DdsHeader
            {
                Size = size,
                Flags = flags,
                Height = height,
                Width = width,
                PitchOrLinearSize = pitchOrLinearSize,
                Depth = depth,
                MipMapCount = mipMapCount,
                PixelFormat = pixelFormat,
                Caps = caps,
                Caps2 = caps2
            };
        }
    }
}
