using System;
using System.IO;
using System.Text;
using OpenSage.Data.IO;

namespace OpenSage.Data.Tga
{
    public sealed class TgaFile
    {
        public TgaHeader Header { get; private set; }
        public byte[] Data { get; private set; }

        public static TgaFile FromUrl(string url)
        {
            using (var stream = FileSystem.OpenStream(url, IO.FileMode.Open))
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                var header = TgaHeader.Parse(reader);

                if (header.HasColorMap)
                {
                    throw new NotSupportedException("TGA files with color maps are not supported.");
                }

                switch (header.ImageType)
                {
                    case TgaImageType.UncompressedRgb:
                    case TgaImageType.UncompressedBlackAndWhite:
                    case TgaImageType.RunLengthEncodedRgb:
                        break;

                    default:
                        throw new NotSupportedException($"TGA file is of an unsupported type: {header.ImageType}.");
                }

                var bytesPerPixel = header.ImagePixelSize / 8;

                byte[] data;
                switch (header.ImageType)
                {
                    case TgaImageType.UncompressedRgb:
                    case TgaImageType.UncompressedBlackAndWhite:
                        data = reader.ReadBytes(header.Width * header.Height * bytesPerPixel);
                        break;

                    case TgaImageType.RunLengthEncodedRgb:
                        data = ReadRunLengthEncodedRgbData(reader, header, bytesPerPixel);
                        break;

                    default:
                        throw new InvalidOperationException();
                }

                var stride = header.Width * bytesPerPixel;

                // Invert y, because data is stored bottom-up in TGA.
                var invertedData = new byte[data.Length];
                var invertedY = 0;
                for (var y = header.Height - 1; y >= 0; y--)
                {
                    Array.Copy(
                        data,
                        y * stride,
                        invertedData,
                        invertedY * stride,
                        stride);

                    invertedY++;
                }

                return new TgaFile
                {
                    Header = header,
                    Data = invertedData
                };
            }
        }

        private static byte[] ReadRunLengthEncodedRgbData(BinaryReader reader, TgaHeader header, int bytesPerPixel)
        {
            var data = new byte[header.Width * header.Height * bytesPerPixel];
            var dataIndex = 0;

            while (dataIndex < data.Length)
            {
                var packet = reader.ReadByte();

                var packetType = (RlePacketType) (packet >> 7);
                var value = (packet & 0b1111111) + 1;

                switch (packetType)
                {
                    case RlePacketType.RawPacket:
                        {
                            var bytes = reader.ReadBytes(bytesPerPixel * value);
                            Array.Copy(bytes, 0, data, dataIndex, bytes.Length);
                            dataIndex += bytes.Length;
                            break;
                        }

                    case RlePacketType.RunLengthEncodedPacket:
                        {
                            var bytes = reader.ReadBytes(bytesPerPixel);
                            for (var i = 0; i < value; i++)
                            {
                                Array.Copy(bytes, 0, data, dataIndex, bytes.Length);
                                dataIndex += bytes.Length;
                            }
                            break;
                        }

                    default:
                        throw new InvalidOperationException();
                }
            }

            return data;
        }

        private enum RlePacketType
        {
            RawPacket = 0,
            RunLengthEncodedPacket = 1
        }

        public static int GetSquareTextureSize(string url)
        {
            using (var stream = FileSystem.OpenStream(url, IO.FileMode.Open))
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                var header = TgaHeader.Parse(reader);

                if (header.Width != header.Height)
                {
                    throw new InvalidOperationException();
                }

                return header.Width;
            }
        }

        public static byte[] ConvertPixelsToRgba8(TgaFile tgaFile, bool alphaToOpaque = false)
        {
            var pixelSize = tgaFile.Header.ImagePixelSize;
            var data = tgaFile.Data;

            switch (pixelSize)
            {
                case 8: // Grayscale
                    {
                        var result = new byte[data.Length * 4];
                        var resultIndex = 0;
                        for (var i = 0; i < data.Length; i++)
                        {
                            result[resultIndex++] = data[i]; // R
                            result[resultIndex++] = data[i]; // G
                            result[resultIndex++] = data[i]; // B
                            result[resultIndex++] = 255;     // A
                        }
                        return result;
                    }

                case 16: // ARRRRRGG GGGBBBBB
                    {
                        var result = new byte[data.Length / 2 * 4];
                        var resultIndex = 0;
                        for (var i = 0; i < data.Length; i += 2)
                        {
                            var gb = data[i + 0];
                            var arg = data[i + 1];

                            var r = (arg >> 2) & 0b11111;
                            var g = (gb >> 5) | ((arg & 0b11) << 3);
                            var b = gb & 0b1111;

                            const float factor = (1.0f / 32.0f) * 255.0f;

                            result[resultIndex++] = (byte) (r * factor); // R
                            result[resultIndex++] = (byte) (g * factor); // G
                            result[resultIndex++] = (byte) (b * factor); // B
                            result[resultIndex++] = 255; // A
                        }
                        return result;
                    }

                case 24: // BGR
                    {
                        var result = new byte[data.Length / 3 * 4];
                        var resultIndex = 0;
                        for (var i = 0; i < data.Length; i += 3)
                        {
                            result[resultIndex++] = data[i + 2]; // R
                            result[resultIndex++] = data[i + 1]; // G
                            result[resultIndex++] = data[i + 0]; // B
                            result[resultIndex++] = 255;         // A
                        }
                        return result;
                    }

                case 32: // BGRA
                    {
                        var result = new byte[data.Length];
                        var resultIndex = 0;
                        for (var i = 0; i < data.Length; i += 4)
                        {
                            result[resultIndex++] = data[i + 2]; // R
                            result[resultIndex++] = data[i + 1]; // G
                            result[resultIndex++] = data[i + 0]; // B
                            if (alphaToOpaque)
                            {
                                result[resultIndex++] = 255; // A
                            }
                            else
                            {
                                result[resultIndex++] = data[i + 3]; // A
                            }
                        }
                        return result;
                    }

                default:
                    throw new ArgumentOutOfRangeException(nameof(pixelSize));
            }
        }
    }
}
