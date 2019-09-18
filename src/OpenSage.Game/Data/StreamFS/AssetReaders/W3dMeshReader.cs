using System.IO;
using OpenSage.Data.W3x;
using OpenSage.Graphics;

namespace OpenSage.Data.StreamFS.AssetReaders
{
    public sealed class W3dMeshReader : AssetReader
    {
        public override AssetType AssetType => AssetType.W3dMesh;

        public override object Parse(Asset asset, BinaryReader reader, AssetImportCollection imports, AssetParseContext context)
        {
            var w3xMesh = W3xMesh.Parse(reader, imports, asset.Header);

            return new ModelMesh(w3xMesh, asset, context.AssetStore.LoadContext);
        }
    }
}
