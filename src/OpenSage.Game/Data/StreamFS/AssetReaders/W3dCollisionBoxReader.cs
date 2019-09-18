using System.IO;
using OpenSage.Data.W3x;

namespace OpenSage.Data.StreamFS.AssetReaders
{
    public sealed class W3dCollisionBoxReader : AssetReader
    {
        public override AssetType AssetType => AssetType.W3dCollisionBox;

        public override object Parse(Asset asset, BinaryReader reader, AssetImportCollection imports, AssetParseContext context)
        {
            var box = W3xBox.Parse(reader);

            return null; // TODO
        }
    }
}
