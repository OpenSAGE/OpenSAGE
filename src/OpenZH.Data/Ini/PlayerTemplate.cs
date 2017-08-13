using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
{
    public sealed class PlayerTemplate
    {
        internal static PlayerTemplate Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<PlayerTemplate> FieldParseTable = new IniParseTable<PlayerTemplate>
        {
            { "Side", (parser, x) => x.Side = parser.ParseAsciiString() },
            { "PlayableSide", (parser, x) => x.PlayableSide = parser.ParseBoolean() },
            { "IsObserver", (parser, x) => x.IsObserver = parser.ParseBoolean() },
            { "StartMoney", (parser, x) => x.StartMoney = parser.ParseInteger() },
            { "PreferredColor", (parser, x) => x.PreferredColor = IniColorRgb.Parse(parser) },
            { "IntrinsicSciences", (parser, x) => x.IntrinsicSciences = parser.ParseAsciiString() },
            { "PurchaseScienceCommandSetRank1", (parser, x) => x.PurchaseScienceCommandSetRank1 = parser.ParseAsciiString() },
            { "PurchaseScienceCommandSetRank3", (parser, x) => x.PurchaseScienceCommandSetRank3 = parser.ParseAsciiString() },
            { "PurchaseScienceCommandSetRank8", (parser, x) => x.PurchaseScienceCommandSetRank8 = parser.ParseAsciiString() },
            { "SpecialPowerShortcutCommandSet", (parser, x) => x.SpecialPowerShortcutCommandSet = parser.ParseAsciiString() },
            { "SpecialPowerShortcutWinName", (parser, x) => x.SpecialPowerShortcutWinName = parser.ParseAsciiString() },
            { "SpecialPowerShortcutButtonCount", (parser, x) => x.SpecialPowerShortcutButtonCount = parser.ParseInteger() },
            { "DisplayName", (parser, x) => x.DisplayName = parser.ParseAsciiString() },
            { "StartingBuilding", (parser, x) => x.StartingBuilding = parser.ParseAsciiString() },
            { "StartingUnit0", (parser, x) => x.StartingUnit0 = parser.ParseAsciiString() },
            { "ScoreScreenImage", (parser, x) => x.ScoreScreenImage = parser.ParseAsciiString() },
            { "LoadScreenImage", (parser, x) => x.LoadScreenImage = parser.ParseAsciiString() },
            { "LoadScreenMusic", (parser, x) => x.LoadScreenMusic = parser.ParseAsciiString() },
            { "FlagWaterMark", (parser, x) => x.FlagWatermark = parser.ParseAsciiString() },
            { "EnabledImage", (parser, x) => x.EnabledImage = parser.ParseAsciiString() },
            { "BeaconName", (parser, x) => x.BeaconName = parser.ParseAsciiString() },
            { "SideIconImage", (parser, x) => x.SideIconImage = parser.ParseAsciiString() },
        };

        public string Name { get; private set; }

        public string Side { get; private set; }
        public bool PlayableSide { get; private set; }
        public bool IsObserver { get; private set; }
        public int StartMoney { get; private set; }
        public IniColorRgb PreferredColor { get; private set; }
        public string IntrinsicSciences { get; private set; }
        public string PurchaseScienceCommandSetRank1 { get; private set; }
        public string PurchaseScienceCommandSetRank3 { get; private set; }
        public string PurchaseScienceCommandSetRank8 { get; private set; }
        public string SpecialPowerShortcutCommandSet { get; private set; }
        public string SpecialPowerShortcutWinName { get; private set; }
        public int SpecialPowerShortcutButtonCount { get; private set; }
        public string DisplayName { get; private set; }
        public string StartingBuilding { get; private set; }
        public string StartingUnit0 { get; private set; }
        public string ScoreScreenImage { get; private set; }
        public string LoadScreenImage { get; private set; }
        public string LoadScreenMusic { get; private set; }
        public string FlagWatermark { get; private set; }
        public string EnabledImage { get; private set; }
        public string BeaconName { get; private set; }
        public string SideIconImage { get; private set; }
    }
}
