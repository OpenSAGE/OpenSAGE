using System.IO;
using OpenSage.Data.W3x;

namespace OpenSage.Data.StreamFS.AssetReaders
{
    public sealed class W3dContainerReader : AssetReader
    {
        public override AssetType AssetType => AssetType.W3dContainer;

        public override object Parse(Asset asset, BinaryReader reader, AssetImportCollection imports, AssetParseContext context)
        {
            return W3xContainer.Parse(reader, imports);
        }
    }
}
