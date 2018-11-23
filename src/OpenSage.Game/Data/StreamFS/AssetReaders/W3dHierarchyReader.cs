using System.IO;
using OpenSage.Data.W3x;

namespace OpenSage.Data.StreamFS.AssetReaders
{
    public sealed class W3dHierarchyReader : AssetReader
    {
        public override AssetType AssetType => AssetType.W3dHierarchy;

        public override object Parse(Asset asset, BinaryReader reader, AssetImportCollection imports, AssetParseContext context)
        {
            return W3xHierarchy.Parse(reader, context.Game);
        }
    }
}
