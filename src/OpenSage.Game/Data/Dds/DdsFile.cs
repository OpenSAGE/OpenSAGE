using System;
using System.IO;
using System.Text;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Dds
{
    public sealed class DdsFile
    {
        public DdsHeader Header { get; private set; }
        public DdsImageFormat ImageFormat { get; private set; }
        public DdsMipMap[] MipMaps { get; private set; }

        public static DdsFile FromStream(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                var magic = reader.ReadUInt32();
                if (magic.ToFourCcString() != "DDS ")
                {
                    throw new InvalidDataException();
                }

                var header = DdsHeader.Parse(reader);

                var imageFormat = GetImageFormat(header.PixelFormat.FourCc);

                if (header.Flags.HasFlag(DdsHeaderFlags.Depth)
                    || header.Caps2.HasFlag(DdsCaps2.CubeMap)
                    || header.Caps2.HasFlag(DdsCaps2.Volume))
                {
                    throw new NotSupportedException();
                }

                var mipMapCount = header.MipMapCount;
                var mipMaps = new DdsMipMap[mipMapCount];

                var width = header.Width;
                var height = header.Height;
                for (var i = 0; i < mipMapCount; i++)
                {
                    var surfaceInfo = GetSurfaceInfo(width, height, imageFormat);

                    var mipMapData = reader.ReadBytes((int) surfaceInfo.NumBytes);

                    mipMaps[i] = new DdsMipMap(mipMapData, surfaceInfo.RowBytes);

                    width >>= 1;
                    height >>= 1;

                    width = Math.Max(width, 1);
                    height = Math.Max(height, 1);
                }

                return new DdsFile
                {
                    Header = header,
                    ImageFormat = imageFormat,
                    MipMaps = mipMaps
                };
            }
        }

        private static DdsImageFormat GetImageFormat(uint fourCc)
        {
            switch (fourCc.ToFourCcString())
            {
                case "DXT1":
                    return DdsImageFormat.Bc1;

                case "DXT3":
                    return DdsImageFormat.Bc2;

                case "DXT5":
                    return DdsImageFormat.Bc3;

                default:
                    throw new NotSupportedException();
            }
        }

        private static SurfaceInfo GetSurfaceInfo(uint width, uint height, DdsImageFormat format)
        {
            uint blockSize;
            switch (format)
            {
                case DdsImageFormat.Bc1:
                    blockSize = 8;
                    break;

                case DdsImageFormat.Bc2:
                case DdsImageFormat.Bc3:
                    blockSize = 16;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            var numBlocksWide = 0u;
            if (width > 0)
            {
                numBlocksWide = Math.Max(1, (width + 3) / 4);
            }

            var numBlocksHigh = 0u;
            if (height > 0)
            {
                numBlocksHigh = Math.Max(1, (height + 3) / 4);
            }

            return new SurfaceInfo
            {
                RowBytes = numBlocksWide * blockSize,
                NumRows = numBlocksHigh,
                NumBytes = numBlocksWide * blockSize * numBlocksHigh
            };
        }

        private struct SurfaceInfo
        {
            public uint NumBytes;
            public uint RowBytes;
            public uint NumRows;
        }
    }
}
