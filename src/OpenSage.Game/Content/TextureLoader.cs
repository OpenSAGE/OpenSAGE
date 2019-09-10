using System;
using System.Collections.Generic;
using System.IO;
using OpenSage.Data;
using OpenSage.Data.Dds;
using OpenSage.Data.Tga;
using OpenSage.Utilities.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Veldrid.ImageSharp;

namespace OpenSage.Content
{
    internal static class TextureLoader
    {
        public static IEnumerable<string> GetPossibleFilePaths(string filePath)
        {
            yield return Path.ChangeExtension(filePath, ".dds");
            yield return Path.ChangeExtension(filePath, ".tga");
            yield return Path.ChangeExtension(filePath, ".jpg");
        }

        public static Texture Load(FileSystemEntry entry, GraphicsDevice graphicsDevice, bool generateMipMaps)
        {
            var texture = LoadImpl(entry, graphicsDevice, generateMipMaps);
            texture.Name = entry.FilePath;
            return texture;
        }

        private static Texture LoadImpl(FileSystemEntry entry, GraphicsDevice graphicsDevice, bool generateMipMaps)
        {
            switch (Path.GetExtension(entry.FilePath).ToLowerInvariant())
            {
                case ".dds":
                    if (!DdsFile.IsDdsFile(entry))
                    {
                        goto case ".tga";
                    }
                    var ddsFile = DdsFile.FromFileSystemEntry(entry);
                    return CreateTextureFromDds(
                        graphicsDevice,
                        ddsFile);

                case ".tga":
                    var tgaFile = TgaFile.FromFileSystemEntry(entry);
                    return CreateTextureFromTga(
                        graphicsDevice,
                        tgaFile,
                        generateMipMaps);

                case ".jpg":
                    using (var stream = entry.Open())
                    {
                        var jpgFile = new ImageSharpTexture(stream);
                        return CreateFromImageSharpTexture(graphicsDevice, jpgFile);
                    }

                default:
                    throw new InvalidOperationException();
            }
        }

        private static Texture CreateTextureFromDds(
            GraphicsDevice graphicsDevice,
            DdsFile ddsFile)
        {
            return graphicsDevice.CreateStaticTexture2D(
                ddsFile.Header.Width,
                ddsFile.Header.Height,
                1,
                ddsFile.MipMaps,
                ddsFile.PixelFormat,
                false);
        }

        private static Texture CreateTextureFromTga(
            GraphicsDevice graphicsDevice,
            TgaFile tgaFile,
            bool generateMipMaps)
        {
            var rgbaData = TgaFile.ConvertPixelsToRgba8(tgaFile);

            using (var tgaImage = Image.LoadPixelData<Rgba32>(
                rgbaData,
                tgaFile.Header.Width,
                tgaFile.Header.Height))
            {
                var imageSharpTexture = new ImageSharpTexture(
                    tgaImage,
                    generateMipMaps);

                return CreateFromImageSharpTexture(graphicsDevice, imageSharpTexture);
            }
        }

        private static Texture CreateFromImageSharpTexture(
            GraphicsDevice graphicsDevice,
            ImageSharpTexture imageSharpTexture)
        {
            return imageSharpTexture.CreateDeviceTexture(
                    graphicsDevice,
                    graphicsDevice.ResourceFactory);
        }
    }
}
