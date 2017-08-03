using System.IO;

namespace OpenZH.Data.Ani
{
    public sealed class IconDirEntry
    {
        public byte Width { get; private set; }
        public byte Height { get; private set; }
        public byte NumColors { get; private set; }
        public byte Reserved { get; private set; }
        public ushort HotspotX { get; private set; }
        public ushort HotspotY { get; private set; }
        public uint DataSize { get; private set; }

        /// <summary>
        /// Specifies the offset of image data from the beginning of the ICO/CUR file
        /// </summary>
        public uint DataOffset { get; private set; }

        internal static IconDirEntry Parse(BinaryReader reader)
        {
            return new IconDirEntry
            {
                Width = reader.ReadByte(),
                Height = reader.ReadByte(),
                NumColors = reader.ReadByte(),
                Reserved = reader.ReadByte(),
                HotspotX = reader.ReadUInt16(),
                HotspotY = reader.ReadUInt16(),
                DataSize = reader.ReadUInt32(),
                DataOffset = reader.ReadUInt32()
            };
        }
    }
}
