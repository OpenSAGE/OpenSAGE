using System.Collections.Generic;

namespace OpenSage.Content
{
    internal interface IScopedAssetStorage
    {
        void PushScope();
        void PopScope();

        IEnumerable<BaseAsset> GetAssets();
    }

    internal interface IScopedSingleAssetStorage : IScopedAssetStorage
    {
        BaseAsset Current { get; set; }
    }

    internal interface IScopedAssetCollection : IScopedAssetStorage
    {
        void Add(object asset);
    }
}
