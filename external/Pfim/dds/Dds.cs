using System;
using System.IO;

namespace Pfim
{
    /// <summary>
    /// Class that represents direct draw surfaces
    /// </summary>
    public class Dds : IImage
    {
        private readonly DdsHeader header;
        private readonly DdsLoadInfo info;
        private readonly byte[] data;

        /// <summary>
        /// Instantiates a direct draw surface image from a header, the data,
        /// and additional info.
        /// </summary>
        internal Dds(DdsHeader header, byte[] data, DdsLoadInfo info)
        {
            this.header = header;
            this.data = data;
            this.info = info;
        }

        /// <summary>Calculates the number of bytes to hold image data</summary>
        internal static int CalcSize(DdsLoadInfo info, DdsHeader header)
        {
            int width = (int)Math.Max(info.divSize, header.Width);
            int height = (int)Math.Max(info.divSize, header.Height);
            return (int)(width / info.divSize * height / info.divSize * info.blockBytes);
        }

        /// <summary>Number of bits that compose a pixel</summary>
        public int BitsPerPixel
        {
            get { return header.Depth != 0 ? (int)header.Depth : info.depth; }
        }

        /// <summary>Number of bytes that compose a pixel</summary>
        public int BytesPerPixel { get { return BitsPerPixel / 8; } }

        /// <summary>The number of bytes that compose one line</summary>
        public int Stride
        {
            get { return (int)(4 * ((header.Width * BytesPerPixel + 3) / 4)); }
        }

        /// <summary>The raw image data</summary>
        public byte[] Data { get { return data; } }

        /// <summary>Width of the image in pixels</summary>
        public int Width { get { return (int)header.Width; } }

        /// <summary>Height of the image in pixels</summary>
        public int Height { get { return (int)header.Height; } }

        /// <summary>The format of the raw data</summary>
        public ImageFormat Format
        {
            get
            {
                switch (BitsPerPixel)
                {
                    case 24: return ImageFormat.Rgb24;
                    case 32: return ImageFormat.Rgba32;
                    default: throw new Exception(
                        "Unrecognized pixel depth: " + BitsPerPixel.ToString());
                }
            }
        }

        /// <summary>Create a direct draw image from a stream</summary>
        public static Dds Create(Stream stream)
        {
            DdsHeader header = new DdsHeader(stream);
            IDecodeDds decoder;
            switch (header.PixelFormat.FourCC)
            {
                case CompressionAlgorithm.D3DFMT_DXT1:
                    decoder = new Dxt1Dds();
                    break;

                case CompressionAlgorithm.D3DFMT_DXT2:
                case CompressionAlgorithm.D3DFMT_DXT4:
                    throw new ArgumentException("Cannot support DXT2 or DXT4");
                case CompressionAlgorithm.D3DFMT_DXT3:
                    decoder = new Dxt3Dds();
                    break;

                case CompressionAlgorithm.D3DFMT_DXT5:
                    decoder = new Dxt5Dds();
                    break;

                case CompressionAlgorithm.None:
                    decoder = new UncompressedDds();
                    break;

                default:
                    throw new ArgumentException("FourCC: " + header.PixelFormat.FourCC + " not supported.");
            }

            return new Dds(header, decoder.Decode(stream, header), decoder.ImageInfo(header));
        }
    }
}
