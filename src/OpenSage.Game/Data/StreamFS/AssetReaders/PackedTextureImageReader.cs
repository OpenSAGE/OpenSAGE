using System.IO;
using OpenSage.Gui;

namespace OpenSage.Data.StreamFS.AssetReaders
{
    public sealed class PackedTextureImageReader : AssetReader
    {
        public override AssetType AssetType => AssetType.PackedTextureImage;

        public override object Parse(Asset asset, BinaryReader reader, AssetImportCollection imports, AssetParseContext context)
        {
            return MappedImage.ParseAsset(reader, asset, imports);
        }
    }
}
