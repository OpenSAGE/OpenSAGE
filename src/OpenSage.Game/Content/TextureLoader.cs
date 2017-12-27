using System;
using System.Collections.Generic;
using System.IO;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.Data;
using OpenSage.Data.Dds;
using OpenSage.Data.Tga;

namespace OpenSage.Content
{
    internal sealed class TextureLoader : ContentLoader<Texture>
    {
        public override object PlaceholderValue { get; }

        public TextureLoader(GraphicsDevice graphicsDevice)
        {
            var texture = AddDisposable(Texture.CreatePlaceholderTexture2D(graphicsDevice));
            texture.DebugName = "Placeholder Texture";
            PlaceholderValue = AddDisposable(texture);
        }

        public override IEnumerable<string> GetPossibleFilePaths(string filePath)
        {
            yield return Path.ChangeExtension(filePath, ".dds");
            yield return Path.ChangeExtension(filePath, ".tga");
        }

        protected override Texture LoadEntry(FileSystemEntry entry, ContentManager contentManager, LoadOptions loadOptions)
        {
            var generateMipMaps = (loadOptions as TextureLoadOptions)?.GenerateMipMaps ?? true;

            Texture applyDebugName(Texture texture)
            {
                texture.DebugName = entry.FilePath;
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
            var mipMapData = new TextureMipMapData[ddsFile.Header.MipMapCount];

            for (var i = 0; i < ddsFile.Header.MipMapCount; i++)
            {
                mipMapData[i] = new TextureMipMapData
                {
                    Data = ddsFile.MipMaps[i].Data,
                    BytesPerRow = (int) ddsFile.MipMaps[i].RowPitch
                };
            }

            var width = (int) ddsFile.Header.Width;
            var height = (int) ddsFile.Header.Height;

            // BC3 texture dimensions need to be aligned to a multiple of 4.
            if (ddsFile.ImageFormat == DdsImageFormat.Bc3)
            {
                width = Math.Max(width, 4);
                height = Math.Max(height, 4);
            }

            return Texture.CreateTexture2D(
                graphicsDevice,
                ddsFile.PixelFormat,
                width,
                height,
                mipMapData);
        }

        public static TextureMipMapData[] GetData(TgaFile tgaFile, bool generateMipMaps)
        {
            var data = TgaFile.ConvertPixelsToRgba8(tgaFile);

            if (generateMipMaps)
            {
                return MipMapUtility.GenerateMipMaps(
                    tgaFile.Header.Width,
                    tgaFile.Header.Height,
                    data);
            }
            else
            {
                return new[]
                {
                    new TextureMipMapData
                    {
                        Data = data,
                        BytesPerRow = tgaFile.Header.Width * 4
                    }
                };
            }
        }

        private static Texture CreateTextureFromTga(
            GraphicsDevice graphicsDevice,
            TgaFile tgaFile,
            bool generateMipMaps)
        {
            var mipMapData = GetData(tgaFile, generateMipMaps);

            return Texture.CreateTexture2D(
                graphicsDevice,
                PixelFormat.Rgba8UNorm,
                tgaFile.Header.Width,
                tgaFile.Header.Height,
                mipMapData);
        }
    }

    public sealed class TextureLoadOptions : LoadOptions
    {
        public bool GenerateMipMaps { get; set; }
    }
}
