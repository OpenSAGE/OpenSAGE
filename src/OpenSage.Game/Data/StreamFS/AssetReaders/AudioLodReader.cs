using System;
using System.IO;

namespace OpenSage.Data.StreamFS.AssetReaders
{
    public sealed class AudioLodReader : AssetReader
    {
        public override AssetType AssetType => AssetType.AudioLod;

        public override object Parse(Asset asset, BinaryReader reader, AssetImportCollection imports, AssetParseContext context)
        {
            // TODO
            return null;
        }
    }
}
