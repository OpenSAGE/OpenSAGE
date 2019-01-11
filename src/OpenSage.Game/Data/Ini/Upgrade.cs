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
            { "LocalPlayerGainsUpgradeEvaEvent", (parser, x) => x.LocalPlayerGainsUpgradeEvaEvent = parser.ParseAssetReference() },
            { "AlliedPlayerGainsUpgradeEvaEvent", (parser, x) => x.AlliedPlayerGainsUpgradeEvaEvent = parser.ParseAssetReference() },
            { "EnemyPlayerGainsUpgradeEvaEvent", (parser, x) => x.EnemyPlayerGainsUpgradeEvaEvent = parser.ParseAssetReference() },
            { "LocalPlayerLosesUpgradeEvaEvent", (parser, x) => x.LocalPlayerLosesUpgradeEvaEvent = parser.ParseAssetReference() },
            { "AlliedPlayerLosesUpgradeEvaEvent", (parser, x) => x.AlliedPlayerLosesUpgradeEvaEvent = parser.ParseAssetReference() },
            { "EnemyPlayerLosesUpgradeEvaEvent", (parser, x) => x.EnemyPlayerLosesUpgradeEvaEvent = parser.ParseAssetReference() },
            { "UseObjectTemplateForCostDiscount", (parser, x) => x.UseObjectTemplateForCostDiscount = parser.ParseAssetReference() },
            { "ResearchCompleteEvaEvent", (parser, x) => x.ResearchCompleteEvaEvent = parser.ParseAssetReference() },
            { "RequiredObjectFilter", (parser, x) => x.RequiredObjectFilter = ObjectFilter.Parse(parser) },
            { "StrategicIcon", (parser, x) => x.StrategicIcon = parser.ParseAssetReference() },
            { "SkirmishAIHeuristic", (parser, x) => x.SkirmishAIHeuristic = parser.ParseEnum<AIHeuristic>() },
            { "SubUpgradeTemplateNames", (parser, x) => x.SubUpgradeTemplateNames = parser.ParseAssetReferenceArray() },
            { "GroupName", (parser, x) => x.GroupName = parser.ParseString() },
            { "GroupOrder", (parser, x) => x.GroupOrder = parser.ParseInteger() }
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

        [AddedIn(SageGame.Bfme)]
        public string Cursor { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool PersistsInCampaign { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool NoUpgradeDiscount { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string UpgradeFX { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string LocalPlayerGainsUpgradeEvaEvent { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string AlliedPlayerGainsUpgradeEvaEvent { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string EnemyPlayerGainsUpgradeEvaEvent { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string AlliedPlayerLosesUpgradeEvaEvent { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string LocalPlayerLosesUpgradeEvaEvent { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string EnemyPlayerLosesUpgradeEvaEvent { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string UseObjectTemplateForCostDiscount { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string ResearchCompleteEvaEvent { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public ObjectFilter RequiredObjectFilter { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string StrategicIcon { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public AIHeuristic SkirmishAIHeuristic { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string[] SubUpgradeTemplateNames { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string GroupName { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int GroupOrder { get; private set; }
    }

    public enum UpgradeType
    {
        [IniEnum("PLAYER")]
        Player,

        [IniEnum("OBJECT")]
        Object
    }

    [AddedIn(SageGame.Bfme2)]
    public enum AIHeuristic
    {
        [IniEnum("AI_UPGRADEHEURISTIC_IMPORTANT")]
        Important,

        [IniEnum("AI_UPGRADEHEURISTIC_FORTRESS")]
        Fortress,

        [IniEnum("AI_UPGRADEHEURISTIC_FACTORY_UNITUNLOCK")]
        FactoryUnitunlock,
    }
}
