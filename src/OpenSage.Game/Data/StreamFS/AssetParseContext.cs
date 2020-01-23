using OpenSage.Content;
using Veldrid;

namespace OpenSage.Data.StreamFS
{
    public sealed class AssetParseContext
    {
        public GraphicsDevice GraphicsDevice { get; }
        internal AssetStore AssetStore { get; }

        public AssetParseContext(Game game)
        {
            GraphicsDevice = game.GraphicsDevice;
            AssetStore = game.AssetStore;
        }
    }
}
