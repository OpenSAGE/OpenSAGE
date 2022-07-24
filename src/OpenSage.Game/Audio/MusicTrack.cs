using System.IO;
using OpenSage.Content;
using OpenSage.Data.Ini;

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

        public LazyAssetReference<AudioFile> File { get; private set; }
    }
}
