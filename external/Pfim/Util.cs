using System;
using System.IO;

namespace Pfim
{
    /// <summary>
    /// Utility class housing methods used by multiple image decoders
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Buffer size to read data from
        /// </summary>
        public const int BUFFER_SIZE = 0x8000;

        /// <summary>
        /// Takes all the bytes at and after an index and moves them to the front and fills the rest
        /// of the buffer with information from the stream.
        /// </summary>
        /// <remarks>
        /// This function is useful when the buffer doesn't have enough information to process a
        /// certain amount of information thus more information from the stream has to be read. This
        /// preserves the information that hasn't been read by putting it at the front.
        /// </remarks>
        /// <param name="str">Stream where more data will be read to fill in end of the buffer.</param>
        /// <param name="buf">The buffer that contains the data that will be translated.</param>
        /// <param name="bufIndex">
        /// Start of the translation. The value initially at this index will be the value at index 0
        /// in the buffer after translation.
        /// </param>
        /// <returns>
        /// The total number of bytes read into the buffer and translated. May be less than the
        /// buffer's length.
        /// </returns>
        public static int Translate(Stream str, byte[] buf, int bufIndex)
        {
            Buffer.BlockCopy(buf, bufIndex, buf, 0, buf.Length - bufIndex);
            int result = str.Read(buf, buf.Length - bufIndex, bufIndex);
            return result + buf.Length - bufIndex;
        }

        /// <summary>
        /// Sets all the values in a buffer to a single value.
        /// </summary>
        /// <param name="buffer">Buffer that will have its values set</param>
        /// <param name="value">The value that the buffer will contain</param>
        /// <param name="length">The number of bytes to write in the buffer</param>
        public static unsafe void memset(int* buffer, int value, int length)
        {
            if (length > 16)
            {
                do
                {
                    *buffer++ = value;
                    *buffer++ = value;
                    *buffer++ = value;
                    *buffer++ = value;
                } while ((length -= 16) >= 16);
            }

            for (; length > 3; length -= 4)
                *buffer++ = value;
        }

        /// <summary>
        /// Sets all the values in a buffer to a single value.
        /// </summary>
        /// <param name="buffer">Buffer that will have its values set</param>
        /// <param name="value">The value that the buffer will contain</param>
        /// <param name="length">The number of bytes to write in the buffer</param>
        public static unsafe void memset(byte* buffer, byte value, int length)
        {
            memset((int*)buffer, value << 24 | value << 16 | value << 8 | value, length);

            int rem = length % 4;
            for (int i = 0; i < rem; i++)
                buffer[length - i - 1] = value;
        }

        /// <summary>
        /// Fills the buffer all the way up with info from the stream
        /// </summary>
        /// <param name="str">Stream that will be used to fill the buffer</param>
        /// <param name="buf">Buffer that will house the information from the stream</param>
        /// <param name="bufSize">The chunk size of data that will be read from the stream</param>
        public static void Fill(Stream str, byte[] buf, int bufSize = BUFFER_SIZE)
        {
            int bufPosition = 0;
            for (int i = buf.Length / bufSize; i > 0; i--)
                bufPosition += str.Read(buf, bufPosition, bufSize);
            str.Read(buf, bufPosition, buf.Length % bufSize);
        }

        /// <summary>
        /// Fills a data array starting from the bottom left by reading from a stream.
        /// </summary>
        /// <param name="str">The stream to read data from.</param>
        /// <param name="data">The buffer to be filled with stream data.</param>
        /// <param name="rowSize">The size in bytes of each row.</param>
        /// <param name="bufSize">The chunk size of data that will be read from the stream</param>
        /// <param name="padding">
        /// The number of bytes that make up the padding in the stride. rowSize + padding = stride
        /// </param>
        public static void FillBottomLeft(
            Stream str,
            byte[] data,
            int rowSize,
            int bufSize = BUFFER_SIZE,
            int padding = 0)
        {
            int bufferIndex = 0;
            byte[] buffer = new byte[bufSize];
            int stride = rowSize + padding;
            int rowsPerBuffer = Math.Min(bufSize, data.Length) / stride;
            int dataIndex = data.Length - stride;
            int rowsRead = 0;
            int totalRows = data.Length / rowSize;
            int rowsToRead = rowsPerBuffer;

            if (rowsPerBuffer == 0)
                throw new ArgumentOutOfRangeException("rowSize", "Row size must be small enough to fit in the buffer");

            int workingSize = str.Read(buffer, 0, bufSize);
            do
            {
                for (int i = 0; i < rowsToRead; i++)
                {
                    Buffer.BlockCopy(buffer, bufferIndex, data, dataIndex, rowSize);
                    dataIndex -= stride;
                    bufferIndex += rowSize;
                }

                if (dataIndex >= 0)
                {
                    workingSize = Translate(str, buffer, bufferIndex);
                    bufferIndex = 0;
                    rowsRead += rowsPerBuffer;
                    rowsToRead = rowsRead + rowsPerBuffer < totalRows ? rowsPerBuffer : totalRows - rowsRead;
                }
            } while (dataIndex >= 0 && workingSize != 0);
        }

        /// <summary>
        /// Calculates stride of an image in bytes given its width in pixels and pixel depth in bits
        /// </summary>
        /// <param name="width">Width in pixels</param>
        /// <param name="pixelDepth">The number of bits that represents a pixel</param>
        /// <returns>The number of bytes that consists of a single line</returns>
        public static int Stride(int width, int pixelDepth)
        {
            if (width <= 0)
                throw new ArgumentException("Width must be greater than zero", "width");
            else if (pixelDepth <= 0)
                throw new ArgumentException("Pixel depth must be greater than zero", "pixelDepth");

            int bytesPerPixel = (pixelDepth + 7) / 8;

            // Windows GDI+ requires that the stride be a multiple of four.
            // Even if not being used for Windows GDI+ there isn't a anything
            // bad with having extra space.
            return 4 * ((width * bytesPerPixel + 3) / 4);
        }
    }
}
