using OpenSage.Content.Loaders;
using OpenSage.Core.Graphics;
using OpenSage.Core.Graphics.W3d;

namespace OpenSage.Content
{
    public sealed class OnDemandAssetLoadStrategy
    {
        private readonly bool _allowOnDemandLoading;
        private readonly IW3dPathResolver _w3dPathResolver;
        private readonly ITexturePathResolver _texturePathResolver;
        private readonly IPathResolver _guiTexturePathResolver;

        public OnDemandAssetLoadStrategy(IW3dPathResolver w3dPathResolver, ITexturePathResolver texturePathResolver, IPathResolver guiTexturePathResolver)
            : this(true)
        {
            _w3dPathResolver = w3dPathResolver;
            _texturePathResolver = texturePathResolver;
            _guiTexturePathResolver = guiTexturePathResolver;
        }

        private OnDemandAssetLoadStrategy(bool allowOnDemandLoading)
        {
            _allowOnDemandLoading = allowOnDemandLoading;
        }

        internal IW3dPathResolver GetW3dPathResolver()
        {
            return _w3dPathResolver;
        }

        internal OnDemandGuiTextureLoader CreateGuiTextureLoader()
        {
            return _allowOnDemandLoading
                ? new OnDemandGuiTextureLoader(false, _guiTexturePathResolver)
                : null;
        }

        internal ITexturePathResolver GetTexturePathResolver()
        {
            return _texturePathResolver;
        }

        internal OnDemandAudioFileLoader CreateAudioFileLoader()
        {
            return _allowOnDemandLoading
                ? new OnDemandAudioFileLoader()
                : null;
        }
    }
}
