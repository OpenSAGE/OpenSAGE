using System.IO;
using OpenSage.Audio;

namespace OpenSage.Data.StreamFS.AssetReaders
{
    public sealed class AudioFileReader : AssetReader
    {
        public override AssetType AssetType => AssetType.AudioFile;

        public override object Parse(Asset asset, BinaryReader reader, AssetImportCollection imports, AssetParseContext context)
        {
            var result = AudioFile.ParseAsset(reader, asset);

            context.AssetStore.AudioFiles.Add(result);

            return result;
        }
    }
}
