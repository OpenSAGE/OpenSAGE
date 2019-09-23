using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Logic.AI
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class SkirmishAIData
    {
        internal static SkirmishAIData Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<SkirmishAIData> FieldParseTable = new IniParseTable<SkirmishAIData>
        {
            { "CombatChainDefinition", (parser, x) => x.CombatChainDefinitions.Add(CombatChainDefinition.Parse(parser)) },
            { "AnyTypeTemplateDisabledSlots", (parser, x) => x.AnyTypeTemplateDisabledSlots = parser.ParseInteger() },
            { "DefaultTargetThreatRadius", (parser, x) => x.DefaultTargetThreatRadius = parser.ParseFloat() },
            { "BrutalDifficultyCheats", (parser, x) => x.BrutalDifficultyCheats = BrutalDifficultyCheats.Parse(parser) },
            { "DifficultyTuning", (parser, x) => x.DifficultyTunings.Add(DifficultyTuning.Parse(parser)) },
            { "DisableBaseBuilding", (parser, x) => x.DisableBaseBuilding = parser.ParseBoolean() },
            { "DisableEconomyBuilding", (parser, x) => x.DisableEconomyBuilding = parser.ParseBoolean() },
            { "DisableUnitBuilding", (parser, x) => x.DisableUnitBuilding = parser.ParseBoolean() },
            { "DisableScienceUpgrading", (parser, x) => x.DisableScienceUpgrading = parser.ParseBoolean() },
            { "DisableUnitUpgrading", (parser, x) => x.DisableUnitUpgrading = parser.ParseBoolean() },
            { "DisableTacticalAI", (parser, x) => x.DisableTacticalAI = parser.ParseBoolean() },
            { "DisableTeamBuilding", (parser, x) => x.DisableTeamBuilding = parser.ParseBoolean() },
            { "DisableWallBuilding", (parser, x) => x.DisableWallBuilding = parser.ParseBoolean() },
            { "MakeAllSkirmishSidesAIControlled", (parser, x) => x.MakeAllSkirmishSidesAIControlled = parser.ParseBoolean() },
            { "TeamIdleCheckRadius", (parser, x) => x.TeamIdleCheckRadius = parser.ParseFloat() },
            { "TeamTimeUntilConsideredIdle", (parser, x) => x.TeamTimeUntilConsideredIdle = parser.ParseFloat() },
            { "DefenseTreeNodeRadius", (parser, x) => x.DefenseTreeNodeRadius = parser.ParseFloat() },
            { "FarmingThreshold", (parser, x) => x.FarmingThreshold = parser.ParseInteger() },
            { "ArmyQualityBias", (parser, x) => x.ArmyQualityBias = parser.ParseInteger() },
            { "ArmyQuantityBias", (parser, x) => x.ArmyQuantityBias = parser.ParseInteger() },
            { "HeroQualityBias", (parser, x) => x.HeroQualityBias = parser.ParseInteger() },
            { "MapControlBias", (parser, x) => x.MapControlBias = parser.ParseInteger() },
            { "BaseStrengthBias", (parser, x) => x.BaseStrengthBias = parser.ParseInteger() },
            { "RingOwnershipBias", (parser, x) => x.RingOwnershipBias = parser.ParseInteger() },
            { "LogicFramesTillRetreatChecksStart", (parser, x) => x.LogicFramesTillRetreatChecksStart = parser.ParseInteger() },
            { "LogicFrameBetweenRetreatChecks", (parser, x) => x.LogicFrameBetweenRetreatChecks = parser.ParseInteger() },
            { "LogicFramesTillAISelfDestructs", (parser, x) => x.LogicFramesTillAISelfDestructs = parser.ParseInteger() },
        };

        public string Name { get; private set; }
        public List<CombatChainDefinition> CombatChainDefinitions { get; } = new List<CombatChainDefinition>();
        public int AnyTypeTemplateDisabledSlots { get; private set; }
        public float DefaultTargetThreatRadius { get; private set; }
        public BrutalDifficultyCheats BrutalDifficultyCheats { get; private set; }
        public List<DifficultyTuning> DifficultyTunings { get; } = new List<DifficultyTuning>();

        public bool DisableBaseBuilding { get; private set; }
        public bool DisableEconomyBuilding { get; private set; }
        public bool DisableUnitBuilding { get; private set; }
        public bool DisableScienceUpgrading { get; private set; }
        public bool DisableUnitUpgrading { get; private set; }
        public bool DisableTacticalAI { get; private set; }
        public bool DisableTeamBuilding { get; private set; }
        public bool DisableWallBuilding { get; private set; }
        public bool MakeAllSkirmishSidesAIControlled { get; private set; }

        public float TeamIdleCheckRadius { get; private set; }
        public float TeamTimeUntilConsideredIdle { get; private set; }
        public float DefenseTreeNodeRadius { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int FarmingThreshold { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int ArmyQualityBias { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int ArmyQuantityBias { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int HeroQualityBias { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int MapControlBias { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int BaseStrengthBias { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int RingOwnershipBias { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int LogicFramesTillRetreatChecksStart { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int LogicFrameBetweenRetreatChecks { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int LogicFramesTillAISelfDestructs { get; private set; }
    }

    public sealed class CombatChainDefinition
    {
        internal static CombatChainDefinition Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<CombatChainDefinition> FieldParseTable = new IniParseTable<CombatChainDefinition>
        {
            { "Unit", (parser, x) => x.Unit = parser.ParseEnum<ObjectKinds>() },
            { "TargetTypes", (parser, x) => x.TargetTypes = parser.ParseEnumBitArray<ObjectKinds>() },
            { "TargetPriorityModifiers", (parser, x) => x.TargetPriorityModifiers = parser.ParseFloatArray() }
            
        };

        public string Name { get; private set; }
        public ObjectKinds Unit { get; private set; }
        public BitArray<ObjectKinds> TargetTypes { get; private set; }
        public float[] TargetPriorityModifiers { get; private set; }
    }

    public class BrutalDifficultyCheats
    {
        internal static BrutalDifficultyCheats Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<BrutalDifficultyCheats> FieldParseTable = new IniParseTable<BrutalDifficultyCheats>
        {
            { "BuildCostReduction", (parser, x) => x.BuildCostReduction = parser.ParsePercentage() },
            { "BuildTimeReduction", (parser, x) => x.BuildTimeReduction = parser.ParsePercentage() }
        };

        public string Name { get; private set; }
        public Percentage BuildCostReduction { get; private set; }
        public Percentage BuildTimeReduction { get; private set;}
    }

    public class DifficultyTuning
    {
        internal static DifficultyTuning Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<DifficultyTuning> FieldParseTable = new IniParseTable<DifficultyTuning>
        {
            { "Difficulty", (parser, x) => x.Difficulty = parser.ParseEnum<Difficulty>() },
            { "EconomyMaxFarms", (parser, x) => x.EconomyMaxFarms = parser.ParseInteger() },
            { "EconomyUpgradeProbability", (parser, x) => x.EconomyUpgradeProbability = Probability.Parse(parser) },
            { "SpecialPowerActivationProbability", (parser, x) => x.SpecialPowerActivationProbability = Probability.Parse(parser) },
            { "OffensiveTacticActivationProbability", (parser, x) => x.OffensiveTacticActivationProbability = Probability.Parse(parser) },
        };

        public string Name { get; private set; }

        public Difficulty Difficulty { get; private set; }
        public int EconomyMaxFarms { get; private set; }
        public Probability EconomyUpgradeProbability { get; private set;}
        public Probability SpecialPowerActivationProbability { get; private set;}
        public Probability OffensiveTacticActivationProbability { get; private set;}
    }

    public class Probability
    {
        //represents values of type 1 : 1024 etc.
        internal static Probability Parse(IniParser parser)
        {
            var first = parser.ParseInteger();
            var colon = parser.GetNextToken();
            var second = parser.ParseInteger();

            return new Probability
            {
                First = first,
                Second = second
            };
        }

        public int First { get; private set; }
        public int Second { get; private set; }
    }

    public enum Difficulty
    {
        [IniEnum("EASY")]
        Easy,

        [IniEnum("NORMAL")]
        Normal,

        [IniEnum("HARD")]
        Hard,

        [IniEnum("BRUTAL")]
        Brutal,
    }
}
