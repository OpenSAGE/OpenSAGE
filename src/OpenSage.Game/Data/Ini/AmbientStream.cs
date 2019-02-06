using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme)]
    public sealed class AmbientStream : BaseSingleSound
    {
        internal static AmbientStream Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static new readonly IniParseTable<AmbientStream> FieldParseTable = BaseSingleSound.FieldParseTable
            .Concat(new IniParseTable<AmbientStream>
            {
                { "Filename", (parser, x) => x.Filename = parser.ParseFileName() },
            });

        public string Filename { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public enum AudioVolumeSlider
    {
        [IniEnum("MUSIC")]
        Music,

        [IniEnum("MOVIE")]
        Movie,

        [IniEnum("SOUNDFX")]
        SoundFX,

        [IniEnum("VOICE")]
        Voice,

        [IniEnum("AMBIENT")]
        Ambient,

        [IniEnum("None"), AddedIn(SageGame.Bfme)]
        None,
    }
}
