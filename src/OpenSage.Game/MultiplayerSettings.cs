using OpenSage.Data.Ini;

namespace OpenSage
{
    public sealed class MultiplayerSettings : BaseSingletonAsset
    {
        internal static void Parse(IniParser parser, MultiplayerSettings value) => parser.ParseBlockContent(value, FieldParseTable);

        private static readonly IniParseTable<MultiplayerSettings> FieldParseTable = new IniParseTable<MultiplayerSettings>
        {
            { "InitialCreditsVeryLow", (parser, x) => x.InitialCreditsVeryLow = parser.ParseInteger() },
            { "InitialCreditsLow", (parser, x) => x.InitialCreditsLow = parser.ParseInteger() },
            { "InitialCreditsMedium", (parser, x) => x.InitialCreditsMedium = parser.ParseInteger() },
            { "InitialCreditsHigh", (parser, x) => x.InitialCreditsHigh = parser.ParseInteger() },
            { "InitialCreditsVeryHigh", (parser, x) => x.InitialCreditsVeryHigh = parser.ParseInteger() },
            { "InitialCreditsMin", (parser, x) => x.InitialCreditsMin = parser.ParseInteger() },
            { "InitialCreditsMax", (parser, x) => x.InitialCreditsMax = parser.ParseInteger() },
            { "StartCountdownTimer", (parser, x) => x.StartCountdownTimer = parser.ParseInteger() },
            { "MaxBeaconsPerPlayer", (parser, x) => x.MaxBeaconsPerPlayer = parser.ParseInteger() },
            { "UseShroud", (parser, x) => x.UseShroud = parser.ParseBoolean() },
            { "ShowRandomPlayerTemplate", (parser, x) => x.ShowRandomPlayerTemplate = parser.ParseBoolean() },
            { "ShowRandomStartPos", (parser, x) => x.ShowRandomStartPos = parser.ParseBoolean() },
            { "ShowRandomColor", (parser, x) => x.ShowRandomColor = parser.ParseBoolean() }
        };

        [AddedIn(SageGame.Bfme)]
        public int InitialCreditsVeryLow { get; private set; }
        [AddedIn(SageGame.Bfme)]
        public int InitialCreditsLow { get; private set; }
        [AddedIn(SageGame.Bfme)]
        public int InitialCreditsMedium { get; private set; }
        [AddedIn(SageGame.Bfme)]
        public int InitialCreditsHigh { get; private set; }
        [AddedIn(SageGame.Bfme)]
        public int InitialCreditsVeryHigh { get; private set; }
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
