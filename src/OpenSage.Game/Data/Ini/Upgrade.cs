using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class Upgrade
    {
        internal static Upgrade Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<Upgrade> FieldParseTable = new IniParseTable<Upgrade>
        {
            { "Type", (parser, x) => x.Type = parser.ParseEnum<UpgradeType>() },
            { "DisplayName", (parser, x) => x.DisplayName = parser.ParseLocalizedStringKey() },
            { "Tooltip", (parser, x) => x.Tooltip = parser.ParseLocalizedStringKey() },
            { "BuildTime", (parser, x) => x.BuildTime = parser.ParseFloat() },
            { "BuildCost", (parser, x) => x.BuildCost = parser.ParseFloat() },
            { "ButtonImage", (parser, x) => x.ButtonImage = parser.ParseAssetReference() },
            { "ResearchSound", (parser, x) => x.ResearchSound = parser.ParseAssetReference() },
            { "UnitSpecificSound", (parser, x) => x.UnitSpecificSound = parser.ParseAssetReference() },
            { "AcademyClassify", (parser, x) => x.AcademyClassify = parser.ParseEnum<AcademyType>() },
            { "Cursor", (parser, x) => x.Cursor = parser.ParseAssetReference() },
            { "PersistsInCampaign", (parser, x) => x.PersistsInCampaign = parser.ParseBoolean() },
            { "NoUpgradeDiscount", (parser, x) => x.NoUpgradeDiscount = parser.ParseBoolean() },
            { "UpgradeFX", (parser, x) => x.UpgradeFX = parser.ParseAssetReference() },
        };

        public string Name { get; private set; }

        public UpgradeType Type { get; private set; } = UpgradeType.Player;
        public string DisplayName { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string Tooltip { get; private set; }

        public float BuildTime { get; private set; }
        public float BuildCost { get; private set; }
        public string ButtonImage { get; private set; }
        public string ResearchSound { get; private set; }
        public string UnitSpecificSound { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public AcademyType AcademyClassify { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string Cursor { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool PersistsInCampaign { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool NoUpgradeDiscount { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string UpgradeFX { get; private set; }
    }

    public enum UpgradeType
    {
        [IniEnum("PLAYER")]
        Player,

        [IniEnum("OBJECT")]
        Object
    }
}
