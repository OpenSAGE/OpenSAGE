using System;
using System.IO;
using OpenZH.Data;
using OpenZH.Data.Dds;
using OpenZH.Data.Tga;
using OpenZH.Graphics.LowLevel;

namespace OpenZH.Graphics
{
    public static class TextureLoader
    {
        public static Texture LoadTexture(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            FileSystemEntry textureFile)
        {
            switch (Path.GetExtension(textureFile.FilePath).ToLower())
            {
                case ".dds":
                    DdsFile ddsFile;
                    using (var textureStream = textureFile.Open())
                    {
                        ddsFile = DdsFile.FromStream(textureStream);
                    }
                    return CreateTextureFromDds(
                        graphicsDevice,
                        uploadBatch,
                        ddsFile);

                case ".tga":
                    TgaFile tgaFile;
                    using (var textureStream = textureFile.Open())
                    {
                        tgaFile = TgaFile.FromStream(textureStream);
                    }
                    return CreateTextureFromTga(
                        graphicsDevice,
                        uploadBatch,
                        tgaFile);

                default:
                    throw new InvalidOperationException();
            }
        }

        private static Texture CreateTextureFromDds(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            DdsFile ddsFile)
        {
            var mipMapData = new TextureMipMapData[ddsFile.Header.MipMapCount];

            for (var i = 0; i < ddsFile.Header.MipMapCount; i++)
            {
                mipMapData[i] = new TextureMipMapData
                {
                    Data = ddsFile.MipMaps[i].Data,
                    BytesPerRow = (int) ddsFile.MipMaps[i].RowPitch
                };
            }

            return Texture.CreateTexture2D(
                graphicsDevice,
                uploadBatch,
                ToPixelFormat(ddsFile.ImageFormat),
                (int) ddsFile.Header.Width,
                (int) ddsFile.Header.Height,
                mipMapData);
        }

        private static PixelFormat ToPixelFormat(DdsImageFormat imageFormat)
        {
            switch (imageFormat)
            {
                case DdsImageFormat.Bc1:
                    return PixelFormat.Bc1;

                case DdsImageFormat.Bc2:
                    return PixelFormat.Bc2;

                case DdsImageFormat.Bc3:
                    return PixelFormat.Bc3;

                default:
                    throw new ArgumentOutOfRangeException(nameof(imageFormat));
            }
        }

        private static Texture CreateTextureFromTga(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            TgaFile tgaFile)
        {
            if (tgaFile.Header.ImageType != TgaImageType.UncompressedRgb)
            {
                throw new InvalidOperationException();
            }

            var data = ConvertTgaPixels(
                tgaFile.Header.ImagePixelSize, 
                tgaFile.Data);

            return Texture.CreateTexture2D(
                graphicsDevice,
                uploadBatch,
                PixelFormat.Rgba8UNorm,
                tgaFile.Header.Width,
                tgaFile.Header.Height,
                new[]
                {
                    new TextureMipMapData
                    {
                        Data = data,
                        BytesPerRow = tgaFile.Header.Width * 4
                    }
                });
        }

        private static byte[] ConvertTgaPixels(byte pixelSize, byte[] data)
        {
            switch (pixelSize)
            {
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
                            result[resultIndex++] = data[i + 3]; // R
                            result[resultIndex++] = data[i + 2]; // G
                            result[resultIndex++] = data[i + 1]; // B
                            result[resultIndex++] = data[i + 0]; // A
                        }
                        return result;
                    }

                default:
                    throw new ArgumentOutOfRangeException(nameof(pixelSize));
            }
        }
    }
}
