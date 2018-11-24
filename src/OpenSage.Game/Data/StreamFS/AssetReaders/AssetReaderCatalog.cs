using System.Collections.Generic;
using System.Linq;

namespace OpenSage.Data.StreamFS.AssetReaders
{
    public static class AssetReaderCatalog
    {
        private static readonly Dictionary<AssetType, AssetReader> _assetReaders;

        static AssetReaderCatalog()
        {
            var assetReaders = new AssetReader[]
            {
                new TextureReader(),
                new W3dCollisionBoxReader(),
                new W3dContainerReader(),
                new W3dHierarchyReader(),
                new W3dMeshReader(),
            };

            _assetReaders = assetReaders.ToDictionary(x => x.AssetType);
        }

        public static bool TryGetAssetReader(uint typeId, out AssetReader assetReader)
        {
            return _assetReaders.TryGetValue((AssetType) typeId, out assetReader);
        }
    }
}
