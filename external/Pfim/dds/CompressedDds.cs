using System.IO;

namespace Pfim
{
    /// <summary>
    /// Class representing decoding compressed direct draw surfaces
    /// </summary>
    internal abstract class CompressedDds : IDecodeDds
    {
        /// <summary>Determine image info from header</summary>
        public abstract DdsLoadInfo ImageInfo(DdsHeader header);

        /// <summary>Uncompress a given block</summary>
        protected abstract int Decode(byte[] stream, byte[] data, int streamIndex, uint dataIndex, uint width);

        /// <summary>Number of bytes for a pixel in the decoded data</summary>
        protected abstract byte PixelDepth { get; }

        /// <summary>Decode data into raw rgb format</summary>
        public byte[] Decode(Stream stream, DdsHeader header)
        {
            byte[] data = new byte[header.Width * header.Height * PixelDepth];
            DdsLoadInfo loadInfo = ImageInfo(header);
            uint dataIndex = 0;

            int bufferSize;
            int workingSize;
            uint pixelsLeft = header.Width * header.Height;

            // The number of bytes that represent a stride in the image
            int bytesPerStride = (int)((header.Width / loadInfo.divSize) * loadInfo.blockBytes);
            int blocksPerStride = (int)(header.Width / loadInfo.divSize);

            byte[] streamBuffer = new byte[Util.BUFFER_SIZE];

            do
            {
                bufferSize = workingSize = stream.Read(streamBuffer, 0, Util.BUFFER_SIZE);
                int bIndex = 0;
                while (workingSize > 0 && pixelsLeft > 0)
                {
                    // If there is not enough of the buffer to fill the next
                    // set of 16 square pixels Get the next buffer
                    if (workingSize < bytesPerStride)
                    {
                        bufferSize = workingSize = Util.Translate(stream, streamBuffer, workingSize);
                        bIndex = 0;
                    }

                    // Now that we have enough pixels to fill a stride (and
                    // this includes the normally 4 pixels below the stride)
                    for (uint i = 0; i < blocksPerStride; i++)
                    {
                        bIndex = Decode(streamBuffer, data, bIndex, dataIndex, header.Width);

                        // Advance to the next block, which is (pixel depth *
                        // divSize) bytes away
                        dataIndex += loadInfo.divSize * PixelDepth;
                    }

                    // Each decoded block is divSize by divSize so pixels left
                    // is Width * multiplied by block height
                    pixelsLeft -= header.Width * loadInfo.divSize;
                    workingSize -= bytesPerStride;

                    // Jump down to the block that is exactly (divSize - 1)
                    // below the current row we are on
                    dataIndex += (PixelDepth * (loadInfo.divSize - 1) * header.Width);
                }
            } while (bufferSize != 0 && pixelsLeft != 0);

            return data;
        }
    }
}
