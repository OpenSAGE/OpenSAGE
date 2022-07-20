using System;
using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Dds;
using OpenSage.Data.Tga;
using OpenSage.Graphics;
using OpenSage.IO;
using OpenSage.Utilities.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Veldrid.ImageSharp;

namespace OpenSage.Core.Graphics;

public sealed class TextureAssetCache : IDisposable
{
    private static readonly string[] PossibleFileExtensions = new[]
        {
            ".dds",
            ".tga",
            ".jpg"
        };

    private readonly GraphicsDevice _graphicsDevice;
    private readonly FileSystem _fileSystem;
    private readonly ITexturePathResolver _pathResolver;

    private readonly Dictionary<uint, TextureAsset> _cache;

    public TextureAssetCache(
        GraphicsDevice graphicsDevice,
        FileSystem fileSystem,
        ITexturePathResolver pathResolver)
    {
        _graphicsDevice = graphicsDevice;
        _fileSystem = fileSystem;
        _pathResolver = pathResolver;

        _cache = new Dictionary<uint, TextureAsset>();
    }

    public TextureAsset GetByName(string name)
    {
        var instanceId = AssetHash.GetHash(name);
        if (_cache.TryGetValue(instanceId, out var result))
        {
            return result;
        }

        // Find it in the file system.
        FileSystemEntry entry = null;
        foreach (var path in _pathResolver.GetPaths(name))
        {
            foreach (var possibleFileExtension in PossibleFileExtensions)
            {
                var possibleFilePath = Path.ChangeExtension(path, possibleFileExtension);
                entry = _fileSystem.GetFile(possibleFilePath);
                if (entry != null)
                {
                    break;
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

        var texture = LoadImpl(entry);
        texture.Name = entry.FilePath;
        result = new TextureAsset(texture, name);

        _cache.Add(instanceId, result);

        return result;
    }

    private Texture LoadImpl(FileSystemEntry entry)
    {
        switch (Path.GetExtension(entry.FilePath).ToLowerInvariant())
        {
            case ".dds":
                if (!DdsFile.IsDdsFile(entry))
                {
                    goto case ".tga";
                }
                var ddsFile = DdsFile.FromFileSystemEntry(entry);
                return CreateTextureFromDds(ddsFile);

            case ".tga":
                var tgaFile = TgaFile.FromFileSystemEntry(entry);
                return CreateTextureFromTga(tgaFile);

            case ".jpg":
                return CreateTextureFromJpg(entry);

            default:
                throw new InvalidOperationException();
        }
    }

    private Texture CreateTextureFromDds(DdsFile ddsFile)
    {
        return _graphicsDevice.CreateStaticTexture2D(
            ddsFile.Header.Width,
            ddsFile.Header.Height,
            1,
            ddsFile.MipMaps,
            ddsFile.PixelFormat,
            false);
    }

    private Texture CreateTextureFromTga(TgaFile tgaFile)
    {
        var rgbaData = TgaFile.ConvertPixelsToRgba8(tgaFile);

        using (var tgaImage = Image.LoadPixelData<Rgba32>(
            rgbaData,
            tgaFile.Header.Width,
            tgaFile.Header.Height))
        {
            var imageSharpTexture = new ImageSharpTexture(
                tgaImage,
                true);

            return CreateFromImageSharpTexture(imageSharpTexture);
        }
    }

    private Texture CreateTextureFromJpg(FileSystemEntry entry)
    {
        ImageSharpTexture colorTexture;
        using (var stream = entry.Open())
        {
            colorTexture = new ImageSharpTexture(stream);
        }

        // Sometimes there's a .png sidecar file which contains the alpha channel.
        using (var stream = entry.Open())
        {
            var pngFilePath = Path.ChangeExtension(entry.FilePath, ".png");
            var pngEntry = _fileSystem.GetFile(pngFilePath);
            if (pngEntry != null)
            {
                using var pngStream = pngEntry.Open();

                var alphaTexture = new ImageSharpTexture(pngStream);

                if (!colorTexture.Images[0].TryGetSinglePixelSpan(out var colorPixelSpan))
                {
                    throw new InvalidOperationException("Unable to get image pixelspan.");
                }

                if (!alphaTexture.Images[0].TryGetSinglePixelSpan(out var alphaPixelSpan))
                {
                    throw new InvalidOperationException("Unable to get image pixelspan.");
                }

                for (var i = 0; i < colorPixelSpan.Length; i++)
                {
                    colorPixelSpan[i].A = alphaPixelSpan[i].A;
                }
            }
        }

        return CreateFromImageSharpTexture(colorTexture);
    }

    private Texture CreateFromImageSharpTexture(ImageSharpTexture imageSharpTexture)
    {
        return imageSharpTexture.CreateDeviceTexture(
            _graphicsDevice,
            _graphicsDevice.ResourceFactory);
    }

    public IEnumerable<BaseAsset> GetAssets()
    {
        return _cache.Values;
    }

    public void Dispose()
    {
        foreach (var textureAsset in _cache.Values)
        {
            textureAsset.Dispose();
        }
    }
}

public interface ITexturePathResolver
{
    IEnumerable<string> GetPaths(string name);
}

public static class TexturePathResolvers
{
    public static readonly ITexturePathResolver Generals = new StandardTexturePathResolver();
    public static readonly ITexturePathResolver Bfme = new BfmeTexturePathResolver();
    public static readonly ITexturePathResolver Bfme2 = new Bfme2TexturePathResolver();

    private sealed class StandardTexturePathResolver : ITexturePathResolver
    {
        public IEnumerable<string> GetPaths(string name)
        {
            yield return Path.Combine("art", "textures", name);
        }
    }

    private sealed class BfmeTexturePathResolver : ITexturePathResolver
    {
        public IEnumerable<string> GetPaths(string name)
        {
            yield return Path.Combine("art", "textures", name);
        }
    }

    private sealed class Bfme2TexturePathResolver : ITexturePathResolver
    {
        public IEnumerable<string> GetPaths(string name)
        {
            yield return Path.Combine("art", "compiledtextures", name.Substring(0, 2), name);
        }
    }
}
