using System.IO;
using OpenSage.Audio;

namespace OpenSage.Data.StreamFS.AssetReaders
{
    public sealed class AudioEventReader : AssetReader
    {
        public override AssetType AssetType => AssetType.AudioEvent;

        public override object Parse(Asset asset, BinaryReader reader, AssetImportCollection imports, AssetParseContext context)
        {
            var result = AudioEvent.ParseAsset(reader, asset, imports);

            context.AssetStore.AudioEvents.Add(result);

            return result;
        }
    }
}
