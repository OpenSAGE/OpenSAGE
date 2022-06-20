using System;
using System.IO;
using OpenSage.Data.Dds;
using OpenSage.Data.Tga;
using OpenSage.Gui;
using OpenSage.IO;
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
            FileSystemEntry entry = null;
            foreach (var path in _pathResolver.GetPaths(name, context.Language))
            {
                foreach (var possibleFileExtension in PossibleFileExtensions)
                {
                    var possibleFilePath = Path.ChangeExtension(path, possibleFileExtension);
                    entry = context.FileSystem.GetFile(possibleFilePath);
                    if (entry != null)
                    {
                        break;
                    }

                    // Temporary workaround for user maps
                    if (Path.IsPathRooted(possibleFilePath))
                    {
                        var info = new FileInfo(possibleFilePath);
                        if (info.Exists)
                        {
                            entry = new FileSystemEntry(context.FileSystem, possibleFilePath, (uint)info.Length, info.OpenRead);
                            break;
                        }
                    }
                }

                if (entry != null)
                {
                    break;
                }
            }

            if (entry == null)
            {
                return null;
            }

            var texture = LoadImpl(entry, context.GraphicsDevice);
            texture.Name = entry.FilePath;
            return new GuiTextureAsset(texture, name);
        }

        private Texture LoadImpl(FileSystemEntry entry, GraphicsDevice graphicsDevice)
        {
            switch (Path.GetExtension(entry.FilePath).ToLowerInvariant())
            {
                case ".dds":
                    if (!DdsFile.IsDdsFile(entry))
                    {
                        goto case ".tga";
                    }
                    var ddsFile = DdsFile.FromFileSystemEntry(entry);
                    return CreateTextureFromDds(ddsFile, graphicsDevice);

                case ".tga":
                    var tgaFile = TgaFile.FromFileSystemEntry(entry);
                    return CreateTextureFromTga(tgaFile, graphicsDevice);

                case ".jpg":
                    using (var stream = entry.Open())
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

            using var tgaImage = Image.LoadPixelData<Rgba32>(
                rgbaData,
                tgaFile.Header.Width,
                tgaFile.Header.Height);
            var imageSharpTexture = new ImageSharpTexture(
                tgaImage,
                _generateMipMaps);

            return CreateFromImageSharpTexture(imageSharpTexture, graphicsDevice);
        }

        private Texture CreateFromImageSharpTexture(ImageSharpTexture imageSharpTexture, GraphicsDevice graphicsDevice)
        {
            return imageSharpTexture.CreateDeviceTexture(
                graphicsDevice,
                graphicsDevice.ResourceFactory);
        }
    }
}
