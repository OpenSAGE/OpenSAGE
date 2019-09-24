using System.IO;
using OpenSage.Audio;

namespace OpenSage.Data.StreamFS.AssetReaders
{
    public sealed class CrowdResponseReader : AssetReader
    {
        public override AssetType AssetType => AssetType.CrowdResponse;

        public override object Parse(Asset asset, BinaryReader reader, AssetImportCollection imports, AssetParseContext context)
        {
            return CrowdResponse.ParseAsset(reader, asset, imports);
        }
    }
}
