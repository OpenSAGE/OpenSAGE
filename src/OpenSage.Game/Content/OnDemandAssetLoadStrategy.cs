using OpenSage.Content.Loaders;
using OpenSage.Core.Graphics;

namespace OpenSage.Content
{
    public sealed class OnDemandAssetLoadStrategy
    {
        private readonly bool _allowOnDemandLoading;
        private readonly IPathResolver _w3dPathResolver;
        private readonly ITexturePathResolver _texturePathResolver;
        private readonly IPathResolver _guiTexturePathResolver;

        public static readonly OnDemandAssetLoadStrategy None = new OnDemandAssetLoadStrategy(false);

        public OnDemandAssetLoadStrategy(IPathResolver w3dPathResolver, ITexturePathResolver texturePathResolver, IPathResolver guiTexturePathResolver)
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

        internal OnDemandAnimationLoader CreateAnimationLoader()
        {
            return _allowOnDemandLoading
                ? new OnDemandAnimationLoader(_w3dPathResolver)
                : null;
        }

        internal OnDemandModelLoader CreateModelLoader()
        {
            return _allowOnDemandLoading
                ? new OnDemandModelLoader(_w3dPathResolver)
                : null;
        }

        internal OnDemandModelBoneHierarchyLoader CreateModelBoneHierarchyLoader()
        {
            return _allowOnDemandLoading
                ? new OnDemandModelBoneHierarchyLoader(_w3dPathResolver)
                : null;
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
