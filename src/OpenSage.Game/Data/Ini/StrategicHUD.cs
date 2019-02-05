using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class StrategicHud
    {
        internal static StrategicHud Parse(IniParser parser) => parser.ParseTopLevelBlock(FieldParseTable);

        private static readonly IniParseTable<StrategicHud> FieldParseTable = new IniParseTable<StrategicHud>
        {
            { "ArmyDetailsPanel", (parser, x) => x.ArmyDetailsPanel = ArmyDetailsPanel.Parse(parser) },
            { "BattleResolver", (parser, x) => x.BattleResolver = BattleResolver.Parse(parser) },
            { "BuildQueueDetailsPanel", (parser, x) => x.BuildQueueDetailsPanel = BuildQueueDetailsPanel.Parse(parser) },
            { "CancelArmyMemberMoveButton", (parser, x) => x.CancelArmyMemberMoveButton = CancelArmyMemberMoveButton.Parse(parser) },
            { "CancelArmyMoveButton", (parser, x) => x.CancelArmyMoveButton = CancelArmyMoveButton.Parse(parser) },
            { "Checklist", (parser, x) => x.Checklist = Checklist.Parse(parser) },
            { "DestroyBuildingButton", (parser, x) => x.DestroyBuildingButton = DestroyBuildingButton.Parse(parser) },
            { "CancelBuildingConstructionButton", (parser, x) => x.CancelBuildingConstructionButton = CancelBuildingConstructionButton.Parse(parser) },
            { "DisbandArmyButton", (parser, x) => x.DisbandArmyButton = DisbandArmyButton.Parse(parser) },
            { "DisbandArmyMemberButton", (parser, x) => x.DisbandArmyMemberButton = DisbandArmyMemberButton.Parse(parser) },
            { "OptionsButton", (parser, x) => x.OptionsButton = OptionsButton.Parse(parser) },
            { "ObjectivesButton", (parser, x) => x.ObjectivesButton = ObjectivesButton.Parse(parser) },
            { "ToggleSelectionDetailsButton", (parser, x) => x.ToggleSelectionDetailsButton = ToggleSelectionDetailsButton.Parse(parser) },
            { "UpgradeUnitButton", (parser, x) => x.UpgradeUnitButton = UpgradeUnitButton.Parse(parser) },
            { "DynamicAutoResolveDialog", (parser, x) => x.DynamicAutoResolveDialog = DynamicAutoResolveDialog.Parse(parser) },
            { "RegionDetailsPanelStructuresPage", (parser, x) => x.RegionDetailsPanelStructuresPage = RegionDetailsPanelStructuresPage.Parse(parser) },
            { "RegionDisplay", (parser, x) => x.RegionDisplay = RegionDisplay.Parse(parser) },
            { "StatsDisplay", (parser, x) => x.StatsDisplay = StatsDisplay.Parse(parser) },
            { "TypeImages", (parser, x) => x.TypeImages = TypeImages.Parse(parser) }
        };

        public ArmyDetailsPanel ArmyDetailsPanel { get; private set; }
        public BattleResolver BattleResolver { get; private set; }
        public BuildQueueDetailsPanel BuildQueueDetailsPanel { get; private set; }
        public CancelArmyMemberMoveButton CancelArmyMemberMoveButton { get; private set; }
        public CancelArmyMoveButton CancelArmyMoveButton { get; private set; }
        public Checklist Checklist { get; private set; }
        public DestroyBuildingButton DestroyBuildingButton { get; private set; }
        public CancelBuildingConstructionButton CancelBuildingConstructionButton { get; private set; }
        public DisbandArmyButton DisbandArmyButton { get; private set; }
        public DisbandArmyMemberButton DisbandArmyMemberButton { get; private set; }
        public OptionsButton OptionsButton { get; private set; }
        public ObjectivesButton ObjectivesButton { get; private set; }
        public ToggleSelectionDetailsButton ToggleSelectionDetailsButton { get; private set; }
        public UpgradeUnitButton UpgradeUnitButton { get; private set; }
        public DynamicAutoResolveDialog DynamicAutoResolveDialog { get; private set; }
        public RegionDetailsPanelStructuresPage RegionDetailsPanelStructuresPage { get; private set; }
        public RegionDisplay RegionDisplay { get; private set; }
        public StatsDisplay StatsDisplay { get; private set; }
        public TypeImages TypeImages { get; private set; }
    }

    public class ArmyDetailsPanel
    {
        internal static ArmyDetailsPanel Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ArmyDetailsPanel> FieldParseTable = new IniParseTable<ArmyDetailsPanel>
        {
            { "BackButtonTooltip", (parser, x) => x.BackButtonTooltip = parser.ParseLocalizedStringKey() },
        };

        public string BackButtonTooltip { get; private set; }
    }

    public class BattleResolver
    {
        internal static BattleResolver Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BattleResolver> FieldParseTable = new IniParseTable<BattleResolver>
        {
            { "StartingAutoResolveBattleMessageDuration", (parser, x) => x.StartingAutoResolveBattleMessageDuration = parser.ParseFloat() },
            { "AllEnemiesRetreatedMessageDuration", (parser, x) => x.AllEnemiesRetreatedMessageDuration = parser.ParseFloat() },
        };

        public float StartingAutoResolveBattleMessageDuration { get; private set; }
        public float AllEnemiesRetreatedMessageDuration { get; private set; }
    }

    public class BuildQueueDetailsPanel
    {
        internal static BuildQueueDetailsPanel Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BuildQueueDetailsPanel> FieldParseTable = new IniParseTable<BuildQueueDetailsPanel>
        {
            { "BackButtonTooltip", (parser, x) => x.BackButtonTooltip = parser.ParseLocalizedStringKey() },
            { "ProgressOverlayColor", (parser, x) => x.ProgressOverlayColor = parser.ParseColorRgba() }
        };

        public string BackButtonTooltip { get; private set; }
        public ColorRgba ProgressOverlayColor { get; private set; }
    }

    public abstract class HudButton
    {
        internal static readonly IniParseTable<HudButton> FieldParseTable = new IniParseTable<HudButton>
        {
            { "Image", (parser, x) => x.Image = parser.ParseLocalizedStringKey() },
            { "Title", (parser, x) => x.Title = parser.ParseLocalizedStringKey() },
            { "Help", (parser, x) => x.Help = parser.ParseLocalizedStringKey() },
        };

        public string Image { get; private set; }
        public string Title { get; private set; }
        public string Help { get; private set; }
    }

    public class CancelArmyMemberMoveButton : HudButton
    {
        internal static CancelArmyMemberMoveButton Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<CancelArmyMemberMoveButton> FieldParseTable = HudButton.FieldParseTable
            .Concat(new IniParseTable<CancelArmyMemberMoveButton>());
    }

    public class CancelArmyMoveButton : HudButton
    {
        internal static CancelArmyMoveButton Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<CancelArmyMoveButton> FieldParseTable = HudButton.FieldParseTable
            .Concat(new IniParseTable<CancelArmyMoveButton>());
    }

    public class DestroyBuildingButton : HudButton
    {
        internal static DestroyBuildingButton Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<DestroyBuildingButton> FieldParseTable = HudButton.FieldParseTable
            .Concat(new IniParseTable<DestroyBuildingButton>());
    }

    public class CancelBuildingConstructionButton : HudButton
    {
        internal static CancelBuildingConstructionButton Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<CancelBuildingConstructionButton> FieldParseTable = HudButton.FieldParseTable
            .Concat(new IniParseTable<CancelBuildingConstructionButton>());
    }

    public class DisbandArmyButton : HudButton
    {
        internal static DisbandArmyButton Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<DisbandArmyButton> FieldParseTable = HudButton.FieldParseTable
            .Concat(new IniParseTable<DisbandArmyButton>());
    }

    public class DisbandArmyMemberButton : HudButton
    {
        internal static DisbandArmyMemberButton Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<DisbandArmyMemberButton> FieldParseTable = HudButton.FieldParseTable
            .Concat(new IniParseTable<DisbandArmyMemberButton>());
    }

    public class OptionsButton : HudButton
    {
        internal static OptionsButton Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<OptionsButton> FieldParseTable = HudButton.FieldParseTable
            .Concat(new IniParseTable<OptionsButton>());
    }

    public class ObjectivesButton : HudButton
    {
        internal static ObjectivesButton Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ObjectivesButton> FieldParseTable = HudButton.FieldParseTable
            .Concat(new IniParseTable<ObjectivesButton>());
    }

    public class ToggleSelectionDetailsButton : HudButton
    {
        internal static ToggleSelectionDetailsButton Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ToggleSelectionDetailsButton> FieldParseTable = HudButton.FieldParseTable
            .Concat(new IniParseTable<ToggleSelectionDetailsButton>());
    }

    public class UpgradeUnitButton : HudButton
    {
        internal static UpgradeUnitButton Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<UpgradeUnitButton> FieldParseTable = HudButton.FieldParseTable
            .Concat(new IniParseTable<UpgradeUnitButton>());
    }

    public class Checklist
    {
        internal static Checklist Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<Checklist> FieldParseTable = new IniParseTable<Checklist>
        {
            { "ItemFont", (parser, x) => x.ItemFont = IniFont.Parse(parser) },
            { "ItemNormalColor", (parser, x) => x.ItemNormalColor = parser.ParseColorRgba() },
            { "ItemImportantColor", (parser, x) => x.ItemImportantColor = parser.ParseColorRgb() },
            { "ItemNewColor", (parser, x) => x.ItemNewColor = parser.ParseColorRgba() },
            { "ItemVerticalSpacing", (parser, x) => x.ItemVerticalSpacing = parser.ParseFloat() },
        };

        public IniFont ItemFont { get; private set; }
        public ColorRgba ItemNormalColor { get; private set; }
        public ColorRgb ItemImportantColor { get; private set; }
        public ColorRgba ItemNewColor { get; private set; }
        public float ItemVerticalSpacing { get; private set; }
    }

    public class DynamicAutoResolveDialog
    {
        internal static DynamicAutoResolveDialog Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<DynamicAutoResolveDialog> FieldParseTable = new IniParseTable<DynamicAutoResolveDialog>
        {
            { "BattleRoundsPerStep", (parser, x) => x.BattleRoundsPerStep = parser.ParseInteger() },
        };

        public int BattleRoundsPerStep { get; private set; }
    }

    public class RegionDetailsPanelStructuresPage
    {
        internal static RegionDetailsPanelStructuresPage Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<RegionDetailsPanelStructuresPage> FieldParseTable = new IniParseTable<RegionDetailsPanelStructuresPage>
        {
            { "ProgressOverlayColor", (parser, x) => x.ProgressOverlayColor = parser.ParseColorRgba() },
        };

        public ColorRgba ProgressOverlayColor { get; private set; }
    }

    public class RegionDisplay
    {
        internal static RegionDisplay Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<RegionDisplay> FieldParseTable = new IniParseTable<RegionDisplay>
        {
            { "BuildPlotsTitle", (parser, x) => x.BuildPlotsTitle = parser.ParseLocalizedStringKey() },
            { "BuildPlotsHelp", (parser, x) => x.BuildPlotsHelp = parser.ParseLocalizedStringKey() },
            { "ArmoryPointsTitle", (parser, x) => x.ArmoryPointsTitle = parser.ParseLocalizedStringKey() },
            { "ArmoryPointsHelp", (parser, x) => x.ArmoryPointsHelp = parser.ParseLocalizedStringKey() },
            { "CommandPointsTitle", (parser, x) => x.CommandPointsTitle = parser.ParseLocalizedStringKey() },
            { "CommandPointsHelp", (parser, x) => x.CommandPointsHelp = parser.ParseLocalizedStringKey() },
        };

        public string BuildPlotsTitle { get; private set; }
        public string BuildPlotsHelp { get; private set; }
        public string ArmoryPointsTitle { get; private set; }
        public string ArmoryPointsHelp { get; private set; }
        public string CommandPointsTitle { get; private set; }
        public string CommandPointsHelp { get; private set; }
    }

    public class StatsDisplay
    {
        internal static StatsDisplay Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<StatsDisplay> FieldParseTable = new IniParseTable<StatsDisplay>
        {
            { "CommandPointsRowTitle", (parser, x) => x.CommandPointsRowTitle = parser.ParseLocalizedStringKey() },
            { "CommandPointsRowHelp", (parser, x) => x.CommandPointsRowHelp = parser.ParseLocalizedStringKey() },
            { "AttackBonusRowTitle", (parser, x) => x.AttackBonusRowTitle = parser.ParseLocalizedStringKey() },
            { "AttackBonusRowHelp", (parser, x) => x.AttackBonusRowHelp = parser.ParseLocalizedStringKey() },
            { "DefenseBonusRowTitle", (parser, x) => x.DefenseBonusRowTitle = parser.ParseLocalizedStringKey() },
            { "DefenseBonusRowHelp", (parser, x) => x.DefenseBonusRowHelp = parser.ParseLocalizedStringKey() },

            { "ExperienceBonusRowTitle", (parser, x) => x.ExperienceBonusRowTitle = parser.ParseLocalizedStringKey() },
            { "ExperienceBonusRowHelp", (parser, x) => x.ExperienceBonusRowHelp = parser.ParseLocalizedStringKey() },
            { "ResourceMultiplierRowTitle", (parser, x) => x.ResourceMultiplierRowTitle = parser.ParseLocalizedStringKey() },
            { "ResourceMultiplierRowHelp", (parser, x) => x.ResourceMultiplierRowHelp = parser.ParseLocalizedStringKey() },
            { "PowerPointsRowTitle", (parser, x) => x.PowerPointsRowTitle = parser.ParseLocalizedStringKey() },
            { "PowerPointsRowHelp", (parser, x) => x.PowerPointsRowHelp = parser.ParseLocalizedStringKey() },
            { "TreasuryIncomeRowTitle", (parser, x) => x.TreasuryIncomeRowTitle = parser.ParseLocalizedStringKey() },
            { "TreasuryIncomeRowHelp", (parser, x) => x.TreasuryIncomeRowHelp = parser.ParseLocalizedStringKey() }
        };

        public string CommandPointsRowTitle { get; private set; }
        public string CommandPointsRowHelp { get; private set; }
        public string AttackBonusRowTitle { get; private set; }
        public string AttackBonusRowHelp { get; private set; }
        public string DefenseBonusRowTitle { get; private set; }
        public string DefenseBonusRowHelp { get; private set; }
        public string ExperienceBonusRowTitle { get; private set; }
        public string ExperienceBonusRowHelp { get; private set; }
        public string ResourceMultiplierRowTitle { get; private set; }
        public string ResourceMultiplierRowHelp { get; private set; }
        public string PowerPointsRowTitle { get; private set; }
        public string PowerPointsRowHelp { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public string TreasuryIncomeRowTitle { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public string TreasuryIncomeRowHelp { get; private set; }
    }

    public class TypeImages
    {
        internal static TypeImages Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<TypeImages> FieldParseTable = new IniParseTable<TypeImages>
        {
            { "BuildingFortress", (parser, x) => x.BuildingFortress = parser.ParseAssetReference() },
            { "BuildingArmory", (parser, x) => x.BuildingArmory = parser.ParseAssetReference() },
            { "BuildingResource", (parser, x) => x.BuildingResource = parser.ParseAssetReference() },
            { "BuildingBarracks", (parser, x) => x.BuildingBarracks = parser.ParseAssetReference() },
            { "UnitSoldier", (parser, x) => x.UnitSoldier = parser.ParseAssetReference() },
            { "UnitArcher", (parser, x) => x.UnitArcher = parser.ParseAssetReference() },
            { "UnitPikemen", (parser, x) => x.UnitPikemen = parser.ParseAssetReference() },
            { "UnitCavalry", (parser, x) => x.UnitCavalry = parser.ParseAssetReference() },
            { "UnitHero", (parser, x) => x.UnitHero = parser.ParseAssetReference() },
            { "UnitFortress", (parser, x) => x.UnitFortress = parser.ParseAssetReference() },
            { "UnitSiege", (parser, x) => x.UnitSiege = parser.ParseAssetReference() },
            { "UnitSupport", (parser, x) => x.UnitSupport = parser.ParseAssetReference() },
        };

        public string BuildingFortress { get; private set; }
        public string BuildingArmory { get; private set; }
        public string BuildingResource { get; private set; }
        public string BuildingBarracks { get; private set; }
        public string UnitSoldier { get; private set; }
        public string UnitArcher { get; private set; }
        public string UnitPikemen { get; private set; }
        public string UnitCavalry { get; private set; }
        public string UnitHero { get; private set; }
        public string UnitFortress { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public string UnitSiege { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public string UnitSupport { get; private set; }
    }
}
