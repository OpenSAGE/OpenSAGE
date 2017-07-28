using System;
using System.IO;
using OpenZH.Data;
using OpenZH.Data.Dds;
using OpenZH.Data.Tga;
using OpenZH.Graphics;

namespace OpenZH.Game.Graphics
{
    public static class TextureLoader
    {
        public static Texture LoadTexture(GraphicsDevice graphicsDevice, FileSystemEntry textureFile)
        {
            switch (Path.GetExtension(textureFile.FilePath).ToLower())
            {
                case ".dds":
                    DdsFile ddsFile;
                    using (var textureStream = textureFile.Open())
                    {
                        ddsFile = DdsFile.FromStream(textureStream);
                    }
                    return CreateTextureFromDds(graphicsDevice, ddsFile);

                case ".tga":
                    TgaFile tgaFile;
                    using (var textureStream = textureFile.Open())
                    {
                        tgaFile = TgaFile.FromStream(textureStream);
                    }
                    return CreateTextureFromTga(graphicsDevice, tgaFile);

                default:
                    throw new InvalidOperationException();
            }
        }

        private static Texture CreateTextureFromDds(GraphicsDevice graphicsDevice, DdsFile ddsFile)
        {
            var texture = Texture.CreateTexture2D(
                graphicsDevice,
                ToPixelFormat(ddsFile.ImageFormat),
                (int) ddsFile.Header.Width,
                (int) ddsFile.Header.Height,
                (int) ddsFile.Header.MipMapCount);

            var uploadBatch = new ResourceUploadBatch(graphicsDevice);
            uploadBatch.Begin();

            for (var i = 0; i < ddsFile.Header.MipMapCount; i++)
            {
                texture.SetData(
                    uploadBatch,
                    i,
                    ddsFile.MipMaps[i].Data,
                    (int) ddsFile.MipMaps[i].RowPitch);
            }

            uploadBatch.End(graphicsDevice.CommandQueue);

            return texture;
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

        private static Texture CreateTextureFromTga(GraphicsDevice graphicsDevice, TgaFile tgaFile)
        {
            if (tgaFile.Header.ImageType != TgaImageType.UncompressedRgb)
            {
                throw new InvalidOperationException();
            }

            var texture = Texture.CreateTexture2D(
                graphicsDevice,
                PixelFormat.Rgba8UNorm,
                tgaFile.Header.Width,
                tgaFile.Header.Height,
                1);

            var uploadBatch = new ResourceUploadBatch(graphicsDevice);
            uploadBatch.Begin();

            var data = ConvertTgaPixels(tgaFile.Header.ImagePixelSize, tgaFile.Data);

            texture.SetData(
                uploadBatch,
                0,
                data,
                tgaFile.Header.Width * 4);

            uploadBatch.End(graphicsDevice.CommandQueue);

            return texture;
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
