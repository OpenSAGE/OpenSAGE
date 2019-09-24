using OpenSage.Data.Ini;

namespace OpenSage.Audio
{
    [AddedIn(SageGame.Bfme)]
    public sealed class AmbientStream : BaseSingleSound
    {
        internal static AmbientStream Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("AmbientStream", name),
                FieldParseTable);
        }

        private static new readonly IniParseTable<AmbientStream> FieldParseTable = BaseSingleSound.FieldParseTable
            .Concat(new IniParseTable<AmbientStream>
            {
                { "Filename", (parser, x) => x.Filename = parser.ParseFileName() },
            });

        public string Filename { get; private set; }
    }
}
