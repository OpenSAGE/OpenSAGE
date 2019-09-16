﻿using OpenSage.Data;
using OpenSage.Graphics;
using OpenSage.Graphics.Shaders;
using Veldrid;

namespace OpenSage.Content.Loaders
{
    internal sealed class AssetLoadContext
    {
        public FileSystem FileSystem { get; }
        public string Language { get; }
        public GraphicsDevice GraphicsDevice { get; }
        public StandardGraphicsResources StandardGraphicsResources { get; }
        public ShaderResourceManager ShaderResources { get; }
        public AssetStore AssetStore { get; }

        public AssetLoadContext(
            FileSystem fileSystem,
            string language,
            GraphicsDevice graphicsDevice,
            StandardGraphicsResources standardGraphicsResources,
            ShaderResourceManager shaderResources,
            AssetStore assetStore)
        {
            FileSystem = fileSystem;
            Language = language;
            GraphicsDevice = graphicsDevice;
            StandardGraphicsResources = standardGraphicsResources;
            ShaderResources = shaderResources;
            AssetStore = assetStore;
        }
    }
}
