using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class AIDozerAssignment
    {
        internal static AIDozerAssignment Parse(IniParser parser)
        {
             return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<AIDozerAssignment> FieldParseTable = new IniParseTable<AIDozerAssignment>
        {
            { "Side", (parser, x) => x.Side = parser.ParseString() },
            { "Unit", (parser, x) => x.Unit = parser.ParseQuotedString() }
        };

        public string Name { get; private set; }
        public string Side { get; private set; }
        public string Unit { get; private set; }
    }
}
