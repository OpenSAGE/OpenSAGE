namespace OpenSage.Content.Loaders
{
    internal interface IOnDemandAssetLoader<TAsset>
        where TAsset : BaseAsset
    {
        TAsset Load(string key, AssetLoadContext context);
    }
}
