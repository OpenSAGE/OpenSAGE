using System.IO;
using OpenSage.Audio;

namespace OpenSage.Data.StreamFS.AssetReaders
{
    public sealed class MusicTrackReader : AssetReader
    {
        public override AssetType AssetType => AssetType.MusicTrack;

        public override object Parse(Asset asset, BinaryReader reader, AssetImportCollection imports, AssetParseContext context)
        {
            var result = MusicTrack.ParseAsset(reader, asset, imports);

            context.AssetStore.MusicTracks.Add(result);

            return result;
        }
    }
}
