using System;
using System.IO;

namespace Pfim
{
    /// <summary>
    /// Defines a series of functions that can decode a compressed targa image
    /// </summary>
    public class CompressedTarga : IDecodeTarga
    {
        /// <summary>Fills data starting from the bottom left</summary>
        public byte[] BottomLeft(Stream str, TargaHeader header)
        {
            var stride = Util.Stride(header.Width, header.PixelDepth);
            var data = new byte[header.Height * stride];
            byte[] filebuffer = new byte[Util.BUFFER_SIZE];
            int dataIndex = data.Length - stride;
            int workingSize = str.Read(filebuffer, 0, Util.BUFFER_SIZE);
            int bytesPerPixel = header.PixelDepth / 8;
            int fileBufferIndex = 0;

            while (dataIndex >= 0)
            {
                int colIndex = 0;

                do
                {
                    int count;
                    bool isRunLength = false;

                    // Make sure we have enough information in the file buffer to read the
                    // next packet of pixel information.
                    if (fileBufferIndex == workingSize)
                    {
                        workingSize = str.Read(data, 0, Util.BUFFER_SIZE);
                        fileBufferIndex = 0;
                        isRunLength = (filebuffer[fileBufferIndex] & 128) != 0;
                        count = isRunLength ? bytesPerPixel + 1 : filebuffer[fileBufferIndex] + 1;
                    }
                    else
                    {
                        isRunLength = (filebuffer[fileBufferIndex] & 128) != 0;
                        count = isRunLength ? bytesPerPixel + 1 : filebuffer[fileBufferIndex] + 1;

                        if (fileBufferIndex + count > workingSize)
                        {
                            workingSize = Util.Translate(str, filebuffer, fileBufferIndex);
                            fileBufferIndex = 0;
                        }
                    }

                    // If the first bit is on, it means that the next packet is run length encoded
                    if (isRunLength)
                    {
                        RunLength(data, filebuffer, dataIndex, fileBufferIndex, bytesPerPixel);
                        dataIndex += (filebuffer[fileBufferIndex] - 127) * bytesPerPixel;
                        colIndex += filebuffer[fileBufferIndex] - 127;
                        fileBufferIndex += count;
                    }
                    else
                    {
                        int bytcount = count * bytesPerPixel;
                        fileBufferIndex++;
                        Buffer.BlockCopy(filebuffer, fileBufferIndex, data, dataIndex, bytcount);
                        fileBufferIndex += bytcount;
                        colIndex += count;
                        dataIndex += bytcount;
                    }
                } while (colIndex < header.Width);
                dataIndex -= bytesPerPixel * header.Width + stride;
            }

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

        /// <summary>Not implemented</summary>
        public byte[] TopLeft(Stream str, TargaHeader header)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Compressed Targa images contain Run Length Packets of length 1 + <paramref name="colorDepth"/>.
        /// The first byte contains how long many pixels are encoded in the packet and the following bytes
        /// determine what colors will be. This function will expand this packet and store the expansion
        /// in the data buffer.
        /// </summary>
        /// <param name="data">Where the expanded run length data will be stored.</param>
        /// <param name="streamBuffer">Buffer that contains the compressed run length info.</param>
        /// <param name="dataIndex">Index of where to start storing the expanded data.</param>
        /// <param name="streamBufferIndex">Index of where the compressed data.</param>
        /// <param name="colorDepth">The number of bytes in a pixel</param>
        public unsafe static void RunLength(byte[] data, byte[] streamBuffer,
            int dataIndex, int streamBufferIndex, int colorDepth)
        {
            int runLength = streamBuffer[streamBufferIndex++] - 127;
            byte color0 = streamBuffer[streamBufferIndex++];
            byte color1 = streamBuffer[streamBufferIndex++];
            byte color2 = streamBuffer[streamBufferIndex++];
            fixed (byte* ptr = &data[dataIndex])
            {
                if (colorDepth == 3)
                {
                    // If the color depth is three, we are able to do a unique optimization.
                    // Four pixels in a row is twelve bytes long. Thus we construct three ints
                    // that will contain all orderings of the pixels. So when set the data equal
                    // to one of these ints, we are really placing 1 (1/3) pixels. We are safe
                    // using this method because the specification states that run length
                    // packets cannot span more than one stride.
                    int* workingptr = (int*)ptr;
                    int size = 3 * runLength;
                    int comb = (color0 | (color1 << 8) | (color2 << 16) | (color0 << 24));
                    int comb1 = (color1 | (color2 << 8) | (color0 << 16) | (color1 << 24));
                    int comb2 = (color2 | (color0 << 8) | (color1 << 16) | (color2 << 24));

                    if (size > 12)
                    {
                        do
                        {
                            *workingptr++ = comb;
                            *workingptr++ = comb1;
                            *workingptr++ = comb2;
                        } while ((size -= 12) >= 12);
                    }

                    byte* byteWorkingPtr = (byte*)workingptr;
                    while (size > 0)
                    {
                        *byteWorkingPtr++ = color0;
                        *byteWorkingPtr++ = color1;
                        *byteWorkingPtr++ = color2;
                        size -= 3;
                    }
                }
                else if (colorDepth == 4)
                {
                    byte color3 = streamBuffer[streamBufferIndex++];
                    var comb = (color0 | color1 << 8 | color2 << 16 | color3 << 24);
                    Util.memset((int*)ptr, comb, runLength * 4);
                }
            }
        }
    }
}
