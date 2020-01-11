using OpenSage.Data.Ini;

namespace OpenSage.Logic.AI
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class AIDozerAssignment : BaseAsset
    {
        internal static AIDozerAssignment Parse(IniParser parser)
        {
             return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("AIDozerAssignment", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<AIDozerAssignment> FieldParseTable = new IniParseTable<AIDozerAssignment>
        {
            { "Side", (parser, x) => x.Side = parser.ParseString() },
            { "Unit", (parser, x) => x.Unit = parser.ParseQuotedString() }
        };

        public string Side { get; private set; }
        public string Unit { get; private set; }
    }
}
