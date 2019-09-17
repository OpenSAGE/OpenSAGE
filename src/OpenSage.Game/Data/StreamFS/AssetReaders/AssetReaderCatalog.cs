using System.Collections.Generic;
using System.Linq;

namespace OpenSage.Data.StreamFS.AssetReaders
{
    public static class AssetReaderCatalog
    {
        private static readonly Dictionary<AssetType, AssetReader> AssetReaders;

        static AssetReaderCatalog()
        {
            var assetReaders = new AssetReader[]
            {
                new AmbientStreamReader(),
                new AudioEventReader(),
                new AudioFileReader(),
                new AudioLodReader(),
                new AudioSettingsReader(),
                new TextureReader(),
                new W3dCollisionBoxReader(),
                new W3dContainerReader(),
                new W3dHierarchyReader(),
                new W3dMeshReader(),
            };

            AssetReaders = assetReaders.ToDictionary(x => x.AssetType);
        }

        public static bool TryGetAssetReader(uint typeId, out AssetReader assetReader)
        {
            return AssetReaders.TryGetValue((AssetType) typeId, out assetReader);
        }
    }
}
