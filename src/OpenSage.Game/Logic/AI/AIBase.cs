using OpenSage.Data.Ini;

namespace OpenSage.Logic.AI
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class AIBase
    {
        internal static AIBase Parse(IniParser parser)
        {
             return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<AIBase> FieldParseTable = new IniParseTable<AIBase>
        {
            { "Side", (parser, x) => x.Side = parser.ParseString() },
            { "Map", (parser, x) => x.Map = parser.ParseQuotedString() },
            { "GameMapToUseOn", (parser, x) => x.GameMapToUseOn = parser.ParseQuotedString() },
            { "PlayerPositions", (parser, x) => x.PlayerPositions = parser.ParseInteger() },
            { "AllowsArbirtaryRotation", (parser, x) => x.AllowsArbirtaryRotation = parser.ParseBoolean() }
        };

        public string Name { get; private set; }
        public string Side { get; private set; }
        public string Map { get; private set; }
        public string GameMapToUseOn { get; private set; }
        public int PlayerPositions { get; private set; }
        public bool AllowsArbirtaryRotation { get; private set; }
    }
}
