using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.BattleForMiddleEarth)]
    public sealed class AmbientStream : BaseSingleSound
    {
        internal static AmbientStream Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
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

    [AddedIn(SageGame.BattleForMiddleEarth)]
    public enum AudioVolumeSlider
    {
        [IniEnum("MUSIC")]
        Music,

        [IniEnum("AMBIENT")]
        Ambient
    }
}
