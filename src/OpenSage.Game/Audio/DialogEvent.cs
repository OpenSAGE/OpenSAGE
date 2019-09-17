using OpenSage.Data.Ini.Parser;

namespace OpenSage.Audio
{
    public sealed class DialogEvent : BaseSingleSound
    {
        internal static DialogEvent Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static new readonly IniParseTable<DialogEvent> FieldParseTable = BaseSingleSound.FieldParseTable
            .Concat(new IniParseTable<DialogEvent>
            {
                { "Filename", (parser, x) => x.Filename = parser.ParseAssetReference() }
            });

        public string Filename { get; private set; }
    }
}
