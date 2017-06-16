using System;
using System.IO;

namespace Pfim
{
    /// <summary>
    /// Defines a series of functions that can decode a uncompressed targa image
    /// </summary>
    public class UncompressedTarga : IDecodeTarga
    {
        /// <summary>Fills data starting from the bottom left</summary>
        public byte[] BottomLeft(Stream str, TargaHeader header)
        {
            var stride = Util.Stride(header.Width, header.PixelDepth);
            var data = new byte[header.Width * stride];
            var pixelWidth = header.PixelDepth * header.Width;
            var padding = stride * 8 - pixelWidth;
            Util.FillBottomLeft(str, data, pixelWidth / 8, padding: padding);
            return data;
        }

        /// <summary>Not implemented</summary>
        public byte[] BottomRight(Stream str, TargaHeader header)
        {
            throw new NotImplementedException();
        }

        /// <summary>Not implemented</summary>
        public byte[] TopRight(Stream str, TargaHeader header)
        {
            throw new NotImplementedException();
        }

        /// <summary>Fills data starting from the top left</summary>
        public byte[] TopLeft(Stream str, TargaHeader header)
        {
            // This isn't as easy as just reading in the stream. The data is
            // such that the individual pixel colors have been swapped, so
            // instead of the traditional rgb, it is bgr

            var stride = Util.Stride(header.Width, header.PixelDepth);
            var data = new byte[header.Width * stride];
            var buffer = new byte[Util.BUFFER_SIZE];
            int dataIndex = 0, bufferIndex = 0;
            int depth = header.PixelDepth / 8;

            int workingSize = str.Read(buffer, 0, Util.BUFFER_SIZE);
            do
            {
                // If there isn't enough data to fill a stride, read more into
                // buffer
                if (buffer.Length - bufferIndex < stride)
                {
                    workingSize = Util.Translate(str, buffer, workingSize);
                    bufferIndex = 0;
                }

                var bufferStrides = (workingSize - bufferIndex) / stride;
                for (int i = 0; i < bufferStrides; i++)
                {
                    for (int k = 0; k < header.Width; k++)
                    {
                        for (int j = 0; j < depth; j++)
                        {
                            data[dataIndex + j] = buffer[bufferIndex + (depth - j - 1)];
                        }
                        bufferIndex += depth;
                        dataIndex += depth;
                    }
                }

            } while (workingSize != 0 && dataIndex < data.Length);
            
            return data;
        }
    }
}
