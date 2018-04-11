using System;
using System.Collections.Generic;
using System.IO;
using OpenSage.Data;
using OpenSage.Data.Dds;
using OpenSage.Data.Tga;
using OpenSage.Utilities;
using OpenSage.Utilities.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Veldrid.ImageSharp;

namespace OpenSage.Content
{
    internal sealed class TextureLoader : ContentLoader<Texture>
    {
        public override object PlaceholderValue { get; }

        public TextureLoader(GraphicsDevice graphicsDevice)
        {
            var texture = AddDisposable(graphicsDevice.CreateStaticTexture2D(
                1, 1,
                new TextureMipMapData(
                    new byte[] { 255, 105, 180, 255 },
                    4, 4, 1, 1),
                PixelFormat.R8_G8_B8_A8_UNorm));
            texture.Name = "Placeholder Texture";
            PlaceholderValue = AddDisposable(texture);
        }

        public override IEnumerable<string> GetPossibleFilePaths(string filePath)
        {
            yield return Path.ChangeExtension(filePath, ".dds");
            yield return Path.ChangeExtension(filePath, ".tga");
        }

        protected override Texture LoadEntry(FileSystemEntry entry, ContentManager contentManager, Game game, LoadOptions loadOptions)
        {
            var generateMipMaps = (loadOptions as TextureLoadOptions)?.GenerateMipMaps ?? true;

            Texture applyDebugName(Texture texture)
            {
                texture.Name = entry.FilePath;
                return texture;
            }

            switch (Path.GetExtension(entry.FilePath).ToLower())
            {
                case ".dds":
                    if (!DdsFile.IsDdsFile(entry))
                    {
                        goto case ".tga";
                    }
                    var ddsFile = DdsFile.FromFileSystemEntry(entry);
                    return applyDebugName(CreateTextureFromDds(
                        contentManager.GraphicsDevice,
                        ddsFile));

                case ".tga":
                    var tgaFile = TgaFile.FromFileSystemEntry(entry);
                    return applyDebugName(CreateTextureFromTga(
                        contentManager.GraphicsDevice,
                        tgaFile,
                        generateMipMaps));

                default:
                    throw new InvalidOperationException();
            }
        }

        private static Texture CreateTextureFromDds(
            GraphicsDevice graphicsDevice,
            DdsFile ddsFile)
        {
            var width = ddsFile.Header.Width;
            var height = ddsFile.Header.Height;

            return graphicsDevice.CreateStaticTexture2D(
                width,
                height,
                ddsFile.MipMaps,
                ddsFile.PixelFormat);
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

                return imageSharpTexture.CreateDeviceTexture(
                    graphicsDevice,
                    graphicsDevice.ResourceFactory);
            }
        }
    }

    public sealed class TextureLoadOptions : LoadOptions
    {
        public bool GenerateMipMaps { get; set; }
    }
}
