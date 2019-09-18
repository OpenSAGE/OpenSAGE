using System.IO;
using OpenSage.Audio;

namespace OpenSage.Data.StreamFS.AssetReaders
{
    public sealed class DialogEventReader : AssetReader
    {
        public override AssetType AssetType => AssetType.DialogEvent;

        public override object Parse(Asset asset, BinaryReader reader, AssetImportCollection imports, AssetParseContext context)
        {
            var result = DialogEvent.ParseAsset(reader, asset, imports);

            context.AssetStore.DialogEvents.Add(result);

            return result;
        }
    }
}
