using System;
using System.IO;
using OpenSage.Data.Dds;
using OpenSage.Data.IO;
using OpenSage.Data.Tga;
using OpenSage.Gui;
using OpenSage.Utilities.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Veldrid.ImageSharp;

namespace OpenSage.Content.Loaders
{
    // TODO: Dedupe this with OnDemandTextureLoader
    internal sealed class OnDemandGuiTextureLoader : IOnDemandAssetLoader<GuiTextureAsset>
    {
        private static readonly string[] PossibleFileExtensions = new[]
        {
            ".dds",
            ".tga",
            ".jpg"
        };

        private readonly bool _generateMipMaps;
        private readonly IPathResolver _pathResolver;

        public OnDemandGuiTextureLoader(
            bool generateMipMaps,
            IPathResolver pathResolver)
        {
            _generateMipMaps = generateMipMaps;
            _pathResolver = pathResolver;
        }

        public GuiTextureAsset Load(string name, AssetLoadContext context)
        {
            // Find it in the file system.
            string url = null;
            foreach (var path in _pathResolver.GetPaths(name, context.Language))
            {
                foreach (var possibleFileExtension in PossibleFileExtensions)
                {
                    var possibleFilePath = Path.ChangeExtension(path, possibleFileExtension);
                    if (FileSystem.FileExists(possibleFilePath))
                    {
                        url = possibleFilePath;
                        break;
                    }
                }

                if (url != null)
                {
                    break;
                }
            }

            if (url is null)
            {
                return null;
            }

            var texture = LoadImpl(url, context.GraphicsDevice);
            texture.Name = url;
            return new GuiTextureAsset(texture, name);
        }

        private Texture LoadImpl(string url, GraphicsDevice graphicsDevice)
        {
            switch (Path.GetExtension(url).ToLowerInvariant())
            {
                case ".dds":
                    if (!DdsFile.IsDdsFile(url))
                    {
                        goto case ".tga";
                    }
                    var ddsFile = DdsFile.FromUrl(url);
                    return CreateTextureFromDds(ddsFile, graphicsDevice);

                case ".tga":
                    var tgaFile = TgaFile.FromUrl(url);
                    return CreateTextureFromTga(tgaFile, graphicsDevice);

                case ".jpg":
                    using (var stream = FileSystem.OpenStream(url, Data.IO.FileMode.Open))
                    {
                        var jpgFile = new ImageSharpTexture(stream);
                        return CreateFromImageSharpTexture(jpgFile, graphicsDevice);
                    }

                default:
                    throw new InvalidOperationException();
            }
        }

        private Texture CreateTextureFromDds(DdsFile ddsFile, GraphicsDevice graphicsDevice)
        {
            return graphicsDevice.CreateStaticTexture2D(
                ddsFile.Header.Width,
                ddsFile.Header.Height,
                1,
                ddsFile.MipMaps,
                ddsFile.PixelFormat,
                false);
        }

        private Texture CreateTextureFromTga(TgaFile tgaFile, GraphicsDevice graphicsDevice)
        {
            var rgbaData = TgaFile.ConvertPixelsToRgba8(tgaFile);

            using (var tgaImage = Image.LoadPixelData<Rgba32>(
                rgbaData,
                tgaFile.Header.Width,
                tgaFile.Header.Height))
            {
                var imageSharpTexture = new ImageSharpTexture(
                    tgaImage,
                    _generateMipMaps);

                return CreateFromImageSharpTexture(imageSharpTexture, graphicsDevice);
            }
        }

        private Texture CreateFromImageSharpTexture(ImageSharpTexture imageSharpTexture, GraphicsDevice graphicsDevice)
        {
            return imageSharpTexture.CreateDeviceTexture(
                graphicsDevice,
                graphicsDevice.ResourceFactory);
        }
    }
}
