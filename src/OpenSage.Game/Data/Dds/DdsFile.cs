using System;
using System.IO;
using System.Text;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.FileFormats;
using OpenSage.Utilities;
using Veldrid;

namespace OpenSage.Data.Dds
{
    public sealed class DdsFile
    {
        public DdsHeader Header { get; private set; }
        public DdsTextureDimension Dimension { get; private set; }
        public DdsImageFormat ImageFormat { get; private set; }
        public uint ArraySize { get; private set; }
        public uint MipMapCount { get; private set; }
        public TextureMipMapData[] MipMaps { get; private set; }

        public PixelFormat PixelFormat
        {
            get
            {
                switch (ImageFormat)
                {
                    // TODO_VELDRID: Should this be Rgba or Rgb?
                    case DdsImageFormat.Bc1:
                        return PixelFormat.BC1_Rgba_UNorm;

                    case DdsImageFormat.Bc2:
                        return PixelFormat.BC2_UNorm;

                    case DdsImageFormat.Bc3:
                        return PixelFormat.BC3_UNorm;

                    case DdsImageFormat.Rg8SNorm:
                        return PixelFormat.R8_G8_SNorm;

                    case DdsImageFormat.Rgba8:
                        return PixelFormat.R8_G8_B8_A8_UNorm;

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        public static bool IsDdsFile(FileSystemEntry entry)
        {
            using (var stream = entry.Open())
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                var magic = reader.ReadFourCc();
                return magic == "DDS ";
            }
        }

        public static DdsFile FromFileSystemEntry(FileSystemEntry entry)
        {
            using (var stream = entry.Open())
            {
                return FromStream(stream);
            }
        }

        public static DdsFile FromStream(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                var magic = reader.ReadFourCc();
                if (magic != "DDS ")
                {
                    throw new InvalidDataException();
                }

                var header = DdsHeader.Parse(reader);

                DdsImageFormat imageFormat;
                if (header.PixelFormat.Flags.HasFlag(DdsPixelFormatFlags.FourCc))
                {
                    imageFormat = GetImageFormat(header.PixelFormat.FourCc);
                }
                else if (header.PixelFormat.Flags.HasFlag(DdsPixelFormatFlags.BumpDuDv))
                {
                    imageFormat = DdsImageFormat.Rg8SNorm;
                }
                else if (header.PixelFormat.Flags.HasFlag(DdsPixelFormatFlags.Rgb))
                {
                    imageFormat = DdsImageFormat.Rgba8;
                }
                else
                {
                    throw new InvalidDataException();
                }

                var dimension = DdsTextureDimension.Texture2D;
                var arraySize = 1u;
                if (header.Flags.HasFlag(DdsHeaderFlags.Depth) && header.Caps2.HasFlag(DdsCaps2.Volume))
                {
                    dimension = DdsTextureDimension.Texture3D;
                }
                else if (header.Caps2.HasFlag(DdsCaps2.CubeMap))
                {
                    dimension = DdsTextureDimension.TextureCube;
                    if (!header.Caps2.HasFlag(DdsCaps2.AllCubeMapFaces))
                    {
                        throw new InvalidDataException();
                    }
                    arraySize = 6;
                }

                var isCompressed = imageFormat == DdsImageFormat.Bc1
                    || imageFormat == DdsImageFormat.Bc2
                    || imageFormat == DdsImageFormat.Bc3;

                var mipMapCount = header.MipMapCount;
                if (mipMapCount == 0)
                {
                    mipMapCount = 1;
                }

                var mipMaps = new TextureMipMapData[mipMapCount * arraySize];

                for (var arrayIndex = 0; arrayIndex < arraySize; arrayIndex++)
                {
                    var width = header.Width;
                    var height = header.Height;

                    if (isCompressed)
                    {
                        // Ensure width and height are multiple of 4.
                        width = (width + 3) / 4 * 4;
                        height = (height + 3) / 4 * 4;
                    }

                    var depth = Math.Max(header.Depth, 1);

                    for (var i = 0; i < mipMapCount; i++)
                    {
                        var surfaceInfo = GetSurfaceInfo(width, height, imageFormat, header);

                        var numSurfaceBytes = surfaceInfo.NumBytes * depth;
                        var mipMapData = reader.ReadBytes((int) numSurfaceBytes);
                        if (mipMapData.Length != numSurfaceBytes)
                        {
                            throw new InvalidDataException();
                        }

                        // Set alpha bytes for 32-bit rgb images that don't include alpha data.
                        if (imageFormat == DdsImageFormat.Rgba8 && !header.PixelFormat.Flags.HasFlag(DdsPixelFormatFlags.AlphaPixels))
                        {
                            for (var j = 0; j < mipMapData.Length; j += 4)
                            {
                                mipMapData[j + 3] = 255;
                            }
                        }

                        mipMaps[(arrayIndex * mipMapCount) + i] = new TextureMipMapData(
                            mipMapData,
                            surfaceInfo.RowBytes,
                            surfaceInfo.NumBytes,
                            width,
                            height);

                        width >>= 1;
                        height >>= 1;
                        depth >>= 1;

                        if (isCompressed)
                        {
                            // Round width and height down to multiple of 4.
                            width -= width % 4;
                            height -= height % 4;

                            width = Math.Max(width, 4);
                            height = Math.Max(height, 4);
                        }
                        else
                        {
                            width = Math.Max(width, 1);
                            height = Math.Max(height, 1);
                        }

                        depth = Math.Max(depth, 1);
                    }
                }

                return new DdsFile
                {
                    Header = header,
                    Dimension = dimension,
                    ImageFormat = imageFormat,
                    ArraySize = arraySize,
                    MipMapCount = mipMapCount,
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
            }

            switch (fourCc)
            {
                case 113:
                    return DdsImageFormat.Rgba16Float;

            }

            throw new NotSupportedException();
        }

        private static SurfaceInfo GetSurfaceInfo(uint width, uint height, DdsImageFormat format, DdsHeader header)
        {
            uint rowBytes = 0;
            switch (format)
            {
                case DdsImageFormat.Rg8SNorm:
                    rowBytes = (width * 16 + 7) / 8; // round up to nearest byte
                    break;

                case DdsImageFormat.Rgba8:
                    rowBytes = width * (header.PixelFormat.RgbBitCount / 8);
                    break;

                case DdsImageFormat.Rgba16Float:
                    rowBytes = width * 4;
                    break;
            }

            if (rowBytes > 0)
            {
                return new SurfaceInfo
                {
                    RowBytes = rowBytes,
                    NumRows = height,
                    NumBytes = rowBytes * height
                };
            }

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

            var numBlocksWide = width / 4;
            var numBlocksHigh = height / 4;

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

    public enum DdsTextureDimension
    {
        Texture2D,
        Texture3D,
        TextureCube
    }
}
