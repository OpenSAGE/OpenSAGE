using OpenSage.Graphics;
using OpenSage.Graphics.Shaders;
using OpenSage.IO;
using OpenSage.Rendering;
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
        public ShaderSetStore ShaderSetStore { get; }
        public AssetStore AssetStore { get; }

        public AssetLoadContext(
            FileSystem fileSystem,
            string language,
            GraphicsDevice graphicsDevice,
            StandardGraphicsResources standardGraphicsResources,
            ShaderResourceManager shaderResources,
            ShaderSetStore shaderSetStore,
            AssetStore assetStore)
        {
            FileSystem = fileSystem;
            Language = language;
            GraphicsDevice = graphicsDevice;
            StandardGraphicsResources = standardGraphicsResources;
            ShaderResources = shaderResources;
            ShaderSetStore = shaderSetStore;
            AssetStore = assetStore;
        }
    }
}
