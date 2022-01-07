using System;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Gui;

namespace OpenSage.Logic.Object
{
    public sealed class UpgradeTemplate : BaseAsset
    {
        internal static UpgradeTemplate Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("Upgrade", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<UpgradeTemplate> FieldParseTable = new IniParseTable<UpgradeTemplate>
        {
            { "Type", (parser, x) => x.Type = parser.ParseEnum<UpgradeType>() },
            { "DisplayName", (parser, x) => x.DisplayName = parser.ParseLocalizedStringKey() },
            { "Tooltip", (parser, x) => x.Tooltip = parser.ParseLocalizedStringKey() },
            { "BuildTime", (parser, x) => x.BuildTime = parser.ParseFloat() },
            { "BuildCost", (parser, x) => x.BuildCost = parser.ParseFloat() },
            { "ButtonImage", (parser, x) => x.ButtonImage = parser.ParseMappedImageReference() },
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

        internal static UpgradeTemplate CreateVeterancyUpgradeTemplate(VeterancyLevel veterancyLevel)
        {
            var result = new UpgradeTemplate
            {
                Type = UpgradeType.Object
            };

            result.SetNameAndInstanceId("Upgrade", $"Upgrade_Veterancy_{veterancyLevel.ToString().ToUpperInvariant()}");

            return result;
        }

        public UpgradeType Type { get; private set; } = UpgradeType.Player;
        public string DisplayName { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string Tooltip { get; private set; }

        public float BuildTime { get; private set; }
        public float BuildCost { get; private set; }
        public LazyAssetReference<MappedImage> ButtonImage { get; private set; }
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

        internal void GrantUpgrade(GameObject gameObject)
        {
            switch (Type)
            {
                case UpgradeType.Player:
                    gameObject.Owner.AddUpgrade(this, UpgradeStatus.Completed);
                    break;

                case UpgradeType.Object:
                    gameObject.Upgrade(this);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        internal void RemoveUpgrade(GameObject gameObject)
        {
            switch (Type)
            {
                case UpgradeType.Player:
                    gameObject.Owner.RemoveUpgrade(this);
                    break;

                case UpgradeType.Object:
                    gameObject.RemoveUpgrade(this);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
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

        [IniEnum("AI_UPGRADEHEURISTIC_ANTISPECIAL"), AddedIn(SageGame.Bfme2Rotwk)]
        Antispecial,

        [IniEnum("AI_UPGRADEHEURISTIC_ANTIARCHER"), AddedIn(SageGame.Bfme2Rotwk)]
        Antiarcher,

        [IniEnum("AI_UPGRADEHEURISTIC_ANTICAVALRY"), AddedIn(SageGame.Bfme2Rotwk)]
        Anticavalry,

        [IniEnum("AI_UPGRADEHEURISTIC_ANTIINFANTRY"), AddedIn(SageGame.Bfme2Rotwk)]
        Antiinfantry,
    }

    public sealed class UpgradeManager
    {
        internal static void Initialize(AssetStore assetStore)
        {
            CreateVeterancyUpgrade(assetStore, VeterancyLevel.Veteran);
            CreateVeterancyUpgrade(assetStore, VeterancyLevel.Elite);
            CreateVeterancyUpgrade(assetStore, VeterancyLevel.Heroic);
        }

        private static void CreateVeterancyUpgrade(AssetStore assetStore, VeterancyLevel veterancyLevel)
        {
            assetStore.Upgrades.Add(UpgradeTemplate.CreateVeterancyUpgradeTemplate(veterancyLevel));
        }
    }
}
