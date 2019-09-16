using OpenSage.Content.Loaders;
using OpenSage.Data;
using OpenSage.Graphics;
using OpenSage.Graphics.Animation;

namespace OpenSage.Content
{
    internal sealed class ScopedModelCollection : ScopedAssetCollection<string, Model>
    {
        public ScopedModelCollection(AssetStore assetStore, OnDemandModelLoader loader)
            : base(assetStore, loader)
        {
        }

        public Model GetByName(string name) => GetByKey(name);

        protected override void OnRemovingAsset(Model asset)
        {
            base.OnRemovingAsset(asset);
            asset.Dispose();
        }

        protected override string NormalizeKey(string key) => FileSystem.NormalizeFilePath(key);

        protected override string GetKey(Model asset) => asset.Name;
    }

    internal sealed class ScopedModelBoneHierarchyCollection : ScopedAssetCollection<string, ModelBoneHierarchy>
    {
        public ScopedModelBoneHierarchyCollection(AssetStore assetStore, OnDemandModelBoneHierarchyLoader loader)
            : base(assetStore, loader)
        {
        }

        public ModelBoneHierarchy GetByName(string name) => GetByKey(name);

        protected override string NormalizeKey(string key) => FileSystem.NormalizeFilePath(key);

        protected override string GetKey(ModelBoneHierarchy asset) => asset.Name;
    }

    internal sealed class ScopedAnimationCollection : ScopedAssetCollection<string, Animation>
    {
        public ScopedAnimationCollection(AssetStore assetStore, OnDemandAnimationLoader loader)
            : base(assetStore, loader)
        {
        }

        public Animation GetByName(string name) => GetByKey(name);

        protected override string NormalizeKey(string key) => FileSystem.NormalizeFilePath(key);

        protected override string GetKey(Animation asset) => asset.Name;
    }
}
