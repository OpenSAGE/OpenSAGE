using System.IO;

namespace OpenSage.Data.StreamFS.AssetReaders
{
    public abstract class AssetReader
    {
        public abstract AssetType AssetType { get; }

        public abstract object Parse(Asset asset, BinaryReader reader, AssetImportCollection imports, AssetParseContext context);
    }
}
