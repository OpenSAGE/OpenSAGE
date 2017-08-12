using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
{
    public sealed class MultiplayerSettings
    {
        internal static MultiplayerSettings Parse(IniParser parser)
        {
            return parser.ParseTopLevelBlock(FieldParseTable);
        }

        private static readonly IniParseTable<MultiplayerSettings> FieldParseTable = new IniParseTable<MultiplayerSettings>
        {
            { "InitialCreditsMin", (parser, x) => x.InitialCreditsMin = parser.ParseInteger() },
            { "InitialCreditsMax", (parser, x) => x.InitialCreditsMax = parser.ParseInteger() },
            { "StartCountdownTimer", (parser, x) => x.StartCountdownTimer = parser.ParseInteger() },
            { "MaxBeaconsPerPlayer", (parser, x) => x.MaxBeaconsPerPlayer = parser.ParseInteger() },
            { "UseShroud", (parser, x) => x.UseShroud = parser.ParseBoolean() },
            { "ShowRandomPlayerTemplate", (parser, x) => x.ShowRandomPlayerTemplate = parser.ParseBoolean() },
            { "ShowRandomStartPos", (parser, x) => x.ShowRandomStartPos = parser.ParseBoolean() },
            { "ShowRandomColor", (parser, x) => x.ShowRandomColor = parser.ParseBoolean() }
        };

        public int InitialCreditsMin { get; private set; }
        public int InitialCreditsMax { get; private set; }
        public int StartCountdownTimer { get; private set; }
        public int MaxBeaconsPerPlayer { get; private set; }
        public bool UseShroud { get; private set; }
        public bool ShowRandomPlayerTemplate { get; private set; }
        public bool ShowRandomStartPos { get; private set; }
        public bool ShowRandomColor { get; private set; }
    }
}
