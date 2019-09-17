using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class ArmyDefinition
    {
        internal static ArmyDefinition Parse(IniParser parser)
        {
             return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<ArmyDefinition> FieldParseTable = new IniParseTable<ArmyDefinition>
        {
            { "Side", (parser, x) => x.Side = parser.ParseString() },
            { "MustUseCommandPointPercentage_Phase1", (parser, x) => x.MustUseCommandPointPercentage_Phase1 = parser.ParsePercentage() },
            { "MustUseCommandPointPercentage_Phase2", (parser, x) => x.MustUseCommandPointPercentage_Phase2 = parser.ParsePercentage() },
            { "MustUseCommandPointPercentage_Phase3", (parser, x) => x.MustUseCommandPointPercentage_Phase3 = parser.ParsePercentage() },
            { "StructureRebuildPriorityModifier", (parser, x) => x.StructureRebuildPriorityModifier = parser.ParsePercentage() },
            { "DefaultUnitPriority", (parser, x) => x.DefaultUnitPriority = parser.ParseFloat() },
            { "FortressRebuildPriority", (parser, x) => x.FortressRebuildPriority = parser.ParseFloat() },
            { "LowUnitPriorityModifier_Rush", (parser, x) => x.LowUnitPriorityModifier_Rush = parser.ParseFloat() },
            { "LowUnitPriorityModifier_MidGame", (parser, x) => x.LowUnitPriorityModifier_MidGame = parser.ParseFloat() },
            { "LowUnitPriorityModifier_EndGame", (parser, x) => x.LowUnitPriorityModifier_EndGame = parser.ParseFloat() },
            { "EconomyBuilderMinFarmsOwned", (parser, x) => x.EconomyBuilderMinFarmsOwned = parser.ParseInteger() },
            { "EconomyBuilderMinMoney", (parser, x) => x.EconomyBuilderMinMoney = parser.ParseInteger() },
            { "EconomyBuilderPerFarmValue", (parser, x) => x.EconomyBuilderPerFarmValue = parser.ParseInteger() },
            { "EconomyBuilderPerSecPriorityIncreaseBase", (parser, x) => x.EconomyBuilderPerSecPriorityIncreaseBase = parser.ParseFloat() },
            { "EconomyBuilderMinTimeBetweenFarms_Rush", (parser, x) => x.EconomyBuilderMinTimeBetweenFarms_Rush = parser.ParseFloat() },
            { "PercentToSave_Rush", (parser, x) => x.PercentToSave_Rush = parser.ParsePercentage() },
            { "PercentToSave_MidGame", (parser, x) => x.PercentToSave_MidGame = parser.ParsePercentage() },
            { "PercentToSave_EndGame", (parser, x) => x.PercentToSave_EndGame = parser.ParsePercentage() },
            { "PhaseDuration_Rush", (parser, x) => x.PhaseDuration_Rush = parser.ParseFloat() },
            { "PhaseDuration_MidGame", (parser, x) => x.PhaseDuration_MidGame = parser.ParseFloat() },
            { "ChanceForUnitsToUpgrade", (parser, x) => x.ChanceForUnitsToUpgrade = parser.ParsePercentage() },
            { "UpgradeSciencePriorityNormalLow", (parser, x) => x.UpgradeSciencePriorityNormalLow = parser.ParseFloat() },
            { "UpgradeSciencePriorityNormalHigh", (parser, x) => x.UpgradeSciencePriorityNormalHigh = parser.ParseFloat() },
            { "UpgradeSciencePriorityImportantLow", (parser, x) => x.UpgradeSciencePriorityImportantLow = parser.ParseFloat() },
            { "UpgradeSciencePriorityImportantHigh", (parser, x) => x.UpgradeSciencePriorityImportantHigh = parser.ParseFloat() },
            { "UnitUpgradePriorityLow", (parser, x) => x.UnitUpgradePriorityLow = parser.ParseFloat() },
            { "UnitUpgradePriorityHigh", (parser, x) => x.UnitUpgradePriorityHigh = parser.ParseFloat() },
            { "MaxThreatForOpportunityTargets", (parser, x) => x.MaxThreatForOpportunityTargets = parser.ParseFloat() },
            { "ValueToSetForMaxOnDefenseTeam", (parser, x) => x.ValueToSetForMaxOnDefenseTeam = parser.ParseInteger() },
            { "CombatChainSearchDepthForTeamRecruits_AttackTeams", (parser, x) => x.CombatChainSearchDepthForTeamRecruits_AttackTeams = parser.ParseInteger() },
            { "CombatChainSearchDepthForTeamRecruits_DefenseTeams", (parser, x) => x.CombatChainSearchDepthForTeamRecruits_DefenseTeams = parser.ParseInteger() },
            { "CombatChainSearchDepthForTeamRecruits_ExploreTeams", (parser, x) => x.CombatChainSearchDepthForTeamRecruits_ExploreTeams = parser.ParseInteger() },
            { "SecondsTillTargetsCanExpire", (parser, x) => x.SecondsTillTargetsCanExpire = parser.ParseFloat() },
            { "ChanceForTargetToExpire", (parser, x) => x.ChanceForTargetToExpire = parser.ParsePercentage() },
            { "MaxBuildingsToBeDefensiveTarget_Small", (parser, x) => x.MaxBuildingsToBeDefensiveTarget_Small = parser.ParseInteger() },
            { "MaxBuildingsToBeDefensiveTarget_Med", (parser, x) => x.MaxBuildingsToBeDefensiveTarget_Med = parser.ParseInteger() },
            { "ChanceToUseAllUnitsForDefenseTarget_Small", (parser, x) => x.ChanceToUseAllUnitsForDefenseTarget_Small = parser.ParsePercentage() },
            { "ChanceToUseAllUnitsForDefenseTarget_Med", (parser, x) => x.ChanceToUseAllUnitsForDefenseTarget_Med = parser.ParsePercentage() },
            { "ChanceToUseAllUnitsForDefenseTarget_Large", (parser, x) => x.ChanceToUseAllUnitsForDefenseTarget_Large = parser.ParsePercentage() },
            { "TacticalAITargets", (parser, x) => x.TacticalAITargets = parser.ParseAssetReferenceArray() },
            { "MaxTeamsPerTarget", (parser, x) => x.MaxTeamsPerTarget = parser.ParseIntegerArray() },
            { "AIEconomyAssigment", (parser, x) => x.AIEconomyAssigment = AIEconomyAssigment.Parse(parser) },
            { "AIWallNodeAssignment", (parser, x) => x.AIWallNodeAssignment = AIWallNodeAssignment.Parse(parser) },
            { "ArmyMemberDefinition", (parser, x) => x.ArmyMemberDefinitions.Add(ArmyMemberDefinition.Parse(parser)) },
            { "HeroBuildOrder", (parser, x) => x.HeroBuildOrder = parser.ParseAssetReferenceArray() },
            { "OffensiveBuildings", (parser, x) => x.OffensiveBuildings = parser.ParseAssetReferenceArray() },
            { "ScavangedResourceBuildings", (parser, x) => x.ScavangedResourceBuildings = parser.ParseAssetReferenceArray() }
        };

        public string Name { get; private set; }

        public string Side { get; private set; }
        public Percentage MustUseCommandPointPercentage_Phase1 { get; private set; }
        public Percentage MustUseCommandPointPercentage_Phase2 { get; private set; }
        public Percentage MustUseCommandPointPercentage_Phase3 { get; private set; }
        public Percentage StructureRebuildPriorityModifier { get; private set; }
        public float DefaultUnitPriority { get; private set; }
        public float FortressRebuildPriority { get; private set; }
        public float LowUnitPriorityModifier_Rush { get; private set; }
        public float LowUnitPriorityModifier_MidGame { get; private set; }
        public float LowUnitPriorityModifier_EndGame { get; private set; }
        public int EconomyBuilderMinFarmsOwned { get; private set; }
        public int EconomyBuilderMinMoney { get; private set; }
        public int EconomyBuilderPerFarmValue { get; private set; }
        public float EconomyBuilderPerSecPriorityIncreaseBase { get; private set; }
        public float EconomyBuilderMinTimeBetweenFarms_Rush { get; private set; }
        public Percentage PercentToSave_Rush { get; private set; }
        public Percentage PercentToSave_MidGame { get; private set; }
        public Percentage PercentToSave_EndGame { get; private set; }
        public float PhaseDuration_Rush { get; private set; }
        public float PhaseDuration_MidGame { get; private set; }
        public Percentage ChanceForUnitsToUpgrade { get; private set; }
        public float UpgradeSciencePriorityNormalLow { get; private set; }
        public float UpgradeSciencePriorityNormalHigh { get; private set; }
        public float UpgradeSciencePriorityImportantLow { get; private set; }
        public float UpgradeSciencePriorityImportantHigh { get; private set; }
        public float UnitUpgradePriorityLow { get; private set; }
        public float UnitUpgradePriorityHigh { get; private set; }
        public float MaxThreatForOpportunityTargets { get; private set; }
        public int ValueToSetForMaxOnDefenseTeam { get; private set; }
        public int CombatChainSearchDepthForTeamRecruits_AttackTeams { get; private set; }
        public int CombatChainSearchDepthForTeamRecruits_DefenseTeams { get; private set; }
        public int CombatChainSearchDepthForTeamRecruits_ExploreTeams { get; private set; }
        public float SecondsTillTargetsCanExpire { get; private set; }
        public Percentage ChanceForTargetToExpire { get; private set; }

        public int MaxBuildingsToBeDefensiveTarget_Small { get; private set; }
        public int MaxBuildingsToBeDefensiveTarget_Med { get; private set; }
        public Percentage ChanceToUseAllUnitsForDefenseTarget_Small { get; private set; }
        public Percentage ChanceToUseAllUnitsForDefenseTarget_Med { get; private set; }
        public Percentage ChanceToUseAllUnitsForDefenseTarget_Large { get; private set; }

        public string[] TacticalAITargets { get; private set; }
        public int[] MaxTeamsPerTarget { get; private set; }

        public AIEconomyAssigment AIEconomyAssigment { get; private set; }
        public AIWallNodeAssignment AIWallNodeAssignment { get; private set; }

        public List<ArmyMemberDefinition> ArmyMemberDefinitions { get; } = new List<ArmyMemberDefinition>();

        public string[] HeroBuildOrder { get; private set; }
        public string[] OffensiveBuildings { get; private set; }
        public string[] ScavangedResourceBuildings { get; private set; }
    }

    public class AIEconomyAssigment
    {
        internal static AIEconomyAssigment Parse(IniParser parser)
        {
             return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<AIEconomyAssigment> FieldParseTable = new IniParseTable<AIEconomyAssigment>
        {
            { "TemplateName", (parser, x) => x.TemplateName = parser.ParseAssetReference() }
        };

        public string Name { get; private set; }
        public string TemplateName { get; private set; }
    }

    public class AIWallNodeAssignment
    {
        internal static AIWallNodeAssignment Parse(IniParser parser)
        {
             return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<AIWallNodeAssignment> FieldParseTable = new IniParseTable<AIWallNodeAssignment>
        {
            { "TemplateName", (parser, x) => x.TemplateName = parser.ParseAssetReference() }
        };

        public string Name { get; private set; }
        public string TemplateName { get; private set; }
    }

    public class ArmyMemberDefinition
    {
        internal static ArmyMemberDefinition Parse(IniParser parser)
        {
             return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<ArmyMemberDefinition> FieldParseTable = new IniParseTable<ArmyMemberDefinition>
        {
            { "Unit", (parser, x) => x.Unit = parser.ParseAssetReference() },
            { "PercentageOfArmyPhase1", (parser, x) => x.PercentageOfArmyPhase1 = parser.ParsePercentage() },
            { "PercentageOfArmyPhase2", (parser, x) => x.PercentageOfArmyPhase2 = parser.ParsePercentage() },
            { "PercentageOfArmyPhase3", (parser, x) => x.PercentageOfArmyPhase3 = parser.ParsePercentage() },
        };

        public string Name { get; private set; }

        public string Unit { get; private set; }
        public Percentage PercentageOfArmyPhase1 { get; private set; }
        public Percentage PercentageOfArmyPhase2 { get; private set; }
        public Percentage PercentageOfArmyPhase3 { get; private set; }
    }
}
