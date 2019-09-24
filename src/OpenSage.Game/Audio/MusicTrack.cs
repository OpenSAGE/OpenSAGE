using System.IO;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Data.StreamFS;

namespace OpenSage.Audio
{
    public sealed class MusicTrack : BaseSingleSound
    {
        internal static MusicTrack Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("MusicTrack", name),
                FieldParseTable);
        }

        private static new readonly IniParseTable<MusicTrack> FieldParseTable = BaseSingleSound.FieldParseTable
            .Concat(new IniParseTable<MusicTrack>
            {
                { "Filename", (parser, x) => x.File = parser.ParseAudioFileReference() }
            });

        internal static MusicTrack ParseAsset(BinaryReader reader, Asset asset, AssetImportCollection imports)
        {
            var result = new MusicTrack();
            result.SetNameAndInstanceId(asset);

            ParseAsset(reader, result);

            result.File = new LazyAssetReference<AudioFile>(imports.GetImportedData<AudioFile>(reader));

            return result;
        }

        public LazyAssetReference<AudioFile> File { get; private set; }
    }
}
