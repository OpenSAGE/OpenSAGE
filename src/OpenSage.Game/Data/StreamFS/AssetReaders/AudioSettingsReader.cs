using System.IO;
using OpenSage.Audio;

namespace OpenSage.Data.StreamFS.AssetReaders
{
    public sealed class AudioSettingsReader : AssetReader
    {
        public override AssetType AssetType => AssetType.AudioSettings;

        public override object Parse(Asset asset, BinaryReader reader, AssetImportCollection imports, AssetParseContext context)
        {
            var result = AudioSettings.ParseAsset(reader);

            context.AssetStore.AudioSettings = result;

            return result;
        }
    }
}
