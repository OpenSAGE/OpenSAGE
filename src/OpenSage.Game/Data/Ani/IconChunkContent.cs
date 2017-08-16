using System;
using System.IO;
using System.Text;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Ani
{
    public sealed class IconChunkContent : RiffChunkContent
    {
        public IconType IconType { get; private set; }
        public ushort NumImages { get; private set; }
        public IconDirEntry[] IconDirEntries { get; private set; }
        public IconImage[] Images { get; private set; }

        internal static IconChunkContent Parse(BinaryReader reader, long endPosition)
        {
            var startPosition = reader.BaseStream.Position;

            var reserved = reader.ReadUInt16();
            if (reserved != 0)
            {
                throw new InvalidDataException();
            }

            var type = reader.ReadUInt16AsEnum<IconType>();
            if (type != IconType.Cur)
            {
                throw new InvalidDataException();
            }

            var numImages = reader.ReadUInt16();
            if (numImages != 1)
            {
                throw new NotSupportedException();
            }

            var iconDirEntries = new IconDirEntry[numImages];
            for (var i = 0; i < numImages; i++)
            {
                iconDirEntries[i] = IconDirEntry.Parse(reader);
            }

            var currentPosition = reader.BaseStream.Position;
            var startingOffset = currentPosition - startPosition;

            var rasterDataBytes = reader.ReadBytes((int) (endPosition - currentPosition));

            var images = new IconImage[numImages];

            for (var i = 0; i < numImages; i++)
            {
                using (var rasterDataStream = new MemoryStream(rasterDataBytes, (int) (iconDirEntries[i].DataOffset - startingOffset), (int) iconDirEntries[i].DataSize))
                using (var rasterDataReader = new BinaryReader(rasterDataStream, Encoding.ASCII, true))
                {
                    images[i] = IconImage.Parse(rasterDataReader);

                    if (rasterDataStream.Position != rasterDataStream.Length)
                    {
                        throw new InvalidDataException();
                    }
                }
            }

            return new IconChunkContent
            {
                IconType = type,
                NumImages = numImages,
                IconDirEntries = iconDirEntries,
                Images = images
            };
        }
    }

    public enum IconType : ushort
    {
        Ico = 1,
        Cur = 2
    }
}
