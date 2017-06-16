using System;
using System.IO;

namespace Pfim
{
    /// <summary>
    /// Provides a mechanism for decoding and storing the decoded information
    /// about a targa image
    /// </summary>
    public class Targa : IImage
    {
        /// <summary>Raw data of the image</summary>
        private readonly byte[] data;

        /// <summary>Header of the image</summary>
        private readonly TargaHeader header;

        /// <summary>
        /// Constructs a targa image from a targa image and raw data
        /// </summary>
        /// <param name="header">The targa header</param>
        /// <param name="data">The decoded targa data</param>
        internal Targa(TargaHeader header, byte[] data)
        {
            this.header = header;
            this.data = data;
        }

        /// <summary>
        /// Creates a targa image from a given stream. The type of targa is determined from the
        /// targa header, which is assumed to be a part of the stream
        /// </summary>
        /// <param name="str">Stream to read the targa image from</param>
        /// <returns>A targa image</returns>
        public static Targa Create(Stream str)
        {
            var header = new TargaHeader(str);
            var targa = (header.IsCompressed) ? (IDecodeTarga)(new CompressedTarga())
                : new UncompressedTarga();

            byte[] data;
            switch (header.Orientation)
            {
                case TargaHeader.TargaOrientation.BottomLeft:
                    data = targa.BottomLeft(str, header);
                    break;

                case TargaHeader.TargaOrientation.BottomRight:
                    data = targa.BottomRight(str, header);
                    break;

                case TargaHeader.TargaOrientation.TopRight:
                    data = targa.TopRight(str, header);
                    break;

                case TargaHeader.TargaOrientation.TopLeft:
                    data = targa.TopLeft(str, header);
                    break;

                default:
                    throw new Exception("Targa orientation not recognized");
            }

            return new Targa(header, data);
        }

        /// <summary>The raw image data</summary>
        public byte[] Data { get { return data; } }

        /// <summary>Width of the image in pixels</summary>
        public int Width { get { return header.Width; } }

        /// <summary>Height of the image in pixels</summary>
        public int Height { get { return header.Height; } }

        /// <summary>The number of bytes that compose one line</summary>
        public int Stride { get { return Util.Stride(header.Width, header.PixelDepth); } }

        /// <summary>The format of the raw data</summary>
        public ImageFormat Format
        {
            get
            {
                switch (header.PixelDepth)
                {
                    case 24: return ImageFormat.Rgb24;
                    case 32: return ImageFormat.Rgba32;
                    default: throw new Exception(
                        "Unrecognized pixel depth: " + header.PixelDepth.ToString());
                }
            }
        }
    }
}
