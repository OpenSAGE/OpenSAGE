using OpenSage.Core.Graphics;
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
        public GraphicsDeviceManager GraphicsDeviceManager { get; }
        public GraphicsDevice GraphicsDevice { get; }
        public ShaderResourceManager ShaderResources { get; }
        public ShaderSetStore ShaderSetStore { get; }
        public AssetStore AssetStore { get; }

        public AssetLoadContext(
            FileSystem fileSystem,
            string language,
            GraphicsDeviceManager graphicsDeviceManager,
            ShaderResourceManager shaderResources,
            ShaderSetStore shaderSetStore,
            AssetStore assetStore)
        {
            FileSystem = fileSystem;
            Language = language;
            GraphicsDeviceManager = graphicsDeviceManager;
            GraphicsDevice = graphicsDeviceManager.GraphicsDevice;
            ShaderResources = shaderResources;
            ShaderSetStore = shaderSetStore;
            AssetStore = assetStore;
        }
    }
}
