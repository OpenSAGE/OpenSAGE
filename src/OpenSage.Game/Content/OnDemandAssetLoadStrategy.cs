using OpenSage.Content.Loaders;

namespace OpenSage.Content
{
    public sealed class OnDemandAssetLoadStrategy
    {
        private readonly bool _allowOnDemandLoading;
        private readonly IPathResolver _w3dPathResolver;
        private readonly IPathResolver _texturePathResolver;

        public static readonly OnDemandAssetLoadStrategy None = new OnDemandAssetLoadStrategy(false);

        public OnDemandAssetLoadStrategy(IPathResolver w3dPathResolver, IPathResolver texturePathResolver)
            : this(true)
        {
            _w3dPathResolver = w3dPathResolver;
            _texturePathResolver = texturePathResolver;
        }

        private OnDemandAssetLoadStrategy(bool allowOnDemandLoading)
        {
            _allowOnDemandLoading = allowOnDemandLoading;
        }

        public OnDemandAnimationLoader CreateAnimationLoader()
        {
            return _allowOnDemandLoading
                ? new OnDemandAnimationLoader(_w3dPathResolver)
                : null;
        }

        public OnDemandModelLoader CreateModelLoader()
        {
            return _allowOnDemandLoading
                ? new OnDemandModelLoader(_w3dPathResolver)
                : null;
        }

        public OnDemandModelBoneHierarchyLoader CreateModelBoneHierarchyLoader()
        {
            return _allowOnDemandLoading
                ? new OnDemandModelBoneHierarchyLoader(_w3dPathResolver)
                : null;
        }

        public OnDemandTextureLoader CreateGuiTextureLoader()
        {
            return _allowOnDemandLoading
                ? new OnDemandTextureLoader(false, _texturePathResolver)
                : null;
        }

        public OnDemandTextureLoader CreateTextureLoader()
        {
            return _allowOnDemandLoading
                ? new OnDemandTextureLoader(true, _texturePathResolver)
                : null;
        }

        internal OnDemandAudioFileLoader CreateAudioFileLoader()
        {
            return _allowOnDemandLoading
                ? new OnDemandAudioFileLoader()
                : null;
        }
    }
}
