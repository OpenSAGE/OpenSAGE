using OpenSage.Content.Loaders;
using OpenSage.Data;
using Veldrid;

namespace OpenSage.Content
{
    internal sealed class ScopedTextureCollection : ScopedAssetCollection<string, Texture>
    {
        public ScopedTextureCollection(AssetStore assetStore, OnDemandTextureLoader loader)
            : base(assetStore, loader)
        {
        }

        public Texture GetByName(string name) => GetByKey(name);

        protected override void OnRemovingAsset(Texture asset)
        {
            base.OnRemovingAsset(asset);
            asset.Dispose();
        }

        protected override string NormalizeKey(string key) => FileSystem.NormalizeFilePath(key);

        protected override string GetKey(Texture asset) => asset.Name;
    }
}
