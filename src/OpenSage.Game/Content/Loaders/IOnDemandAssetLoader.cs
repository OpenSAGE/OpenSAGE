namespace OpenSage.Content.Loaders
{
    internal interface IOnDemandAssetLoader<TKey, TValue>
        where TValue : class
    {
        TValue Load(TKey key, AssetLoadContext context);
    }
}
