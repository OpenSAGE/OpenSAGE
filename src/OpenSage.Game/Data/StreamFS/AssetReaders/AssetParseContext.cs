using OpenSage.Graphics.Effects;
using Veldrid;

namespace OpenSage.Data.StreamFS.AssetReaders
{
    public sealed class AssetParseContext
    {
        public GraphicsDevice GraphicsDevice { get; }
        public EffectLibrary EffectLibrary { get; }

        public AssetParseContext(Game game)
        {
            GraphicsDevice = game.GraphicsDevice;
            EffectLibrary = game.ContentManager.EffectLibrary;
        }
    }
}
