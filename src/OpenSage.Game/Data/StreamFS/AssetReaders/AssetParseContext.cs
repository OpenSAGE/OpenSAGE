using Veldrid;

namespace OpenSage.Data.StreamFS.AssetReaders
{
    public sealed class AssetParseContext
    {
        public GraphicsDevice GraphicsDevice { get; }

        public AssetParseContext(Game game)
        {
            GraphicsDevice = game.GraphicsDevice;
        }
    }
}
