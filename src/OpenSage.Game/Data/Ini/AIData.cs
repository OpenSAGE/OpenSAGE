using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;
using System.Numerics;

namespace OpenSage.Data.Ini
{
    public sealed class AIData
    {
        internal static AIData Parse(IniParser parser) => parser.ParseTopLevelBlock(FieldParseTable);

        private static readonly IniParseTable<AIData> FieldParseTable = new IniParseTable<AIData>
        {
            { "UseLowLODTrees", (parser, x) => x.UseLowLodTrees = parser.ParseBoolean() },
            { "LowLodTreeScale", (parser, x) => x.LowLodTreeScale = parser.ParseFloat() },
            { "LowLodTreeName", (parser, x) => x.LowLodTreeName = parser.ParseAssetReference() },
            { "LowLodTreeNameNoGrab", (parser, x) => x.LowLodTreeNameNoGrab = parser.ParseAssetReference() },
            { "LowLodTreeNameNoHarvest", (parser, x) => x.LowLodTreeNameNoHarvest = parser.ParseAssetReference() },

            { "StructureSeconds", (parser, x) => x.StructureSeconds = parser.ParseFloat() },
            { "TeamSeconds", (parser, x) => x.TeamSeconds = parser.ParseFloat() },
            { "Wealthy", (parser, x) => x.Wealthy = parser.ParseInteger() },
            { "Poor", (parser, x) => x.Poor = parser.ParseInteger() },
            { "StructuresWealthyRate", (parser, x) => x.StructuresWealthyRate = parser.ParseFloat() },
            { "StructuresPoorRate", (parser, x) => x.StructuresPoorRate = parser.ParseFloat() },
            { "TeamsWealthyRate", (parser, x) => x.TeamsWealthyRate = parser.ParseFloat() },
            { "TeamsPoorRate", (parser, x) => x.TeamsPoorRate = parser.ParseFloat() },
            { "TeamResourcesToStart", (parser, x) => x.TeamResourcesToStart = parser.ParseFloat() },
            { "GuardInnerModifierAI", (parser, x) => x.GuardInnerModifierAI = parser.ParseFloat() },
            { "GuardOuterModifierAI", (parser, x) => x.GuardOuterModifierAI = parser.ParseFloat() },
            { "GuardInnerModifierHuman", (parser, x) => x.GuardInnerModifierHuman = parser.ParseFloat() },
            { "GuardOuterModifierHuman", (parser, x) => x.GuardOuterModifierHuman = parser.ParseFloat() },
            { "GuardChaseUnitsDuration", (parser, x) => x.GuardChaseUnitsDuration = parser.ParseInteger() },
            { "GuardEnemyScanRate", (parser, x) => x.GuardEnemyScanRate = parser.ParseInteger() },
            { "GuardEnemyReturnScanRate", (parser, x) => x.GuardEnemyReturnScanRate = parser.ParseInteger() },
            { "AlertRangeModifier", (parser, x) => x.AlertRangeModifier = parser.ParseFloat() },
            { "AggressiveRangeModifier", (parser, x) => x.AggressiveRangeModifier = parser.ParseFloat() },
            { "AttackPriorityDistanceModifier", (parser, x) => x.AttackPriorityDistanceModifier = parser.ParseFloat() },
            { "MaxRecruitRadius", (parser, x) => x.MaxRecruitRadius = parser.ParseFloat() },
            { "SkirmishBaseDefenseExtraDistance", (parser, x) => x.SkirmishBaseDefenseExtraDistance = parser.ParseFloat() },
            { "ForceIdleMSEC", (parser, x) => x.ForceIdleMsec = parser.ParseInteger() },
            { "ForceSkirmishAI", (parser, x) => x.ForceSkirmishAI = parser.ParseBoolean() },
            { "RotateSkirmishBases", (parser, x) => x.RotateSkirmishBases = parser.ParseBoolean() },
            { "AttackUsesLineOfSight", (parser, x) => x.AttackUsesLineOfSight = parser.ParseBoolean() },

            { "EnableRepulsors", (parser, x) => x.EnableRepulsors = parser.ParseBoolean() },
            { "RepulsedDistance", (parser, x) => x.RepulsedDistance = parser.ParseFloat() },

            { "WallHeight", (parser, x) => x.WallHeight = parser.ParseInteger() },

            { "AttackIgnoreInsignificantBuildings", (parser, x) => x.AttackIgnoreInsignificantBuildings = parser.ParseBoolean() },

            { "SkirmishGroupFudgeDistance", (parser, x) => x.SkirmishGroupFudgeDistance = parser.ParseFloat() },

            { "MinInfantryForGroup", (parser, x) => x.MinInfantryForGroup = parser.ParseInteger() },
            { "MinVehiclesForGroup", (parser, x) => x.MinVehiclesForGroup = parser.ParseInteger() },
            { "MinDistanceForGroup", (parser, x) => x.MinDistanceForGroup = parser.ParseFloat() },
            { "DistanceRequiresGroup", (parser, x) => x.DistanceRequiresGroup = parser.ParseFloat() },

            { "FormationEnemyDistance", (parser, x) => x.FormationEnemyDistance = parser.ParseFloat() },
            { "FormationColumnWidth", (parser, x) => x.FormationColumnWidth = parser.ParseFloat() },
            { "FormationRowDepth", (parser, x) => x.FormationRowDepth = parser.ParseFloat() },
            { "FormationSquadSpacing", (parser, x) => x.FormationSquadSpacing = parser.ParseFloat() },
            { "FormationColumns", (parser, x) => x.FormationColumns = parser.ParseInteger() },
            { "UseFormations", (parser, x) => x.UseFormations = parser.ParseBoolean() },
            { "WaitForOthers", (parser, x) => x.WaitForOthers = parser.ParseBoolean() },

            { "NarrowPassageScale", (parser, x) => x.NarrowPassageScale = parser.ParseFloat() },

            { "HordesWaitForHordes", (parser, x) => x.HordesWaitForHordes = parser.ParseBoolean() },
            { "AttackMoveUsesFormations", (parser, x) => x.AttackMoveUsesFormations = parser.ParseBoolean() },

            { "InfantryPathfindDiameter", (parser, x) => x.InfantryPathfindDiameter = parser.ParseInteger() },
            { "VehiclePathfindDiameter", (parser, x) => x.VehiclePathfindDiameter = parser.ParseInteger() },

            { "SupplyCenterSafeRadius", (parser, x) => x.SupplyCenterSafeRadius = parser.ParseFloat() },
            { "RebuildDelayTimeSeconds", (parser, x) => x.RebuildDelayTimeSeconds = parser.ParseInteger() },

            { "AIDozerBoredRadiusModifier", (parser, x) => x.AIDozerBoredRadiusModifier = parser.ParseFloat() },
            { "AICrushesInfantry", (parser, x) => x.AICrushesInfantry = parser.ParseBoolean() },

            { "MeleeApproachDist", (parser, x) => x.MeleeApproachDist = parser.ParseFloat() },
            { "MeleeApproachTolerance", (parser, x) => x.MeleeApproachTolerance = parser.ParseFloat() },
            { "MeleeAcquireLimitDist", (parser, x) => x.MeleeAcquireLimitDist = parser.ParseFloat() },
            { "WadeWaterDepth", (parser, x) => x.WadeWaterDepth = parser.ParseFloat() },

            { "MaxRetaliationDistance", (parser, x) => x.MaxRetaliationDistance = parser.ParseFloat() },
            { "MaxRetaliateDistance", (parser, x) => x.MaxRetaliationDistance = parser.ParseFloat() }, // Same thing, but BFME uses different name from ZH

            { "RetaliationFriendsRadius", (parser, x) => x.RetaliationFriendsRadius = parser.ParseFloat() },
            { "RetaliateFriendsRadius", (parser, x) => x.RetaliationFriendsRadius = parser.ParseFloat() }, // Same thing, but BFME uses different name from ZH

            { "ChaseFromBehindLimit", (parser, x) => x.ChaseFromBehindLimit = parser.ParseFloat() },
            { "ForceHordesToLowLOD", (parser, x) => x.ForceHordesToLowLod = parser.ParseBoolean() },
            { "AllowForestFires", (parser, x) => x.AllowForestFires = parser.ParseBoolean() },
            { "CastleSiegeStandBackDistance", (parser, x) => x.CastleSiegeStandBackDistance = parser.ParseFloat() },

            { "SideInfo", (parser, x) => x.SideInfos.Add(AISideInfo.Parse(parser)) },

            { "SkirmishBuildList", (parser, x) => x.SkirmishBuildLists.Add(AISkirmishBuildList.Parse(parser)) },

            { "AttackPriority", (parser, x) => x.AttackPriorities.Add(AttackPriority.Parse(parser)) },

            { "AltCameraZoomOverride", (parser, x) => x.AltCameraZoomOverride = parser.ParseFloat() },
            { "AltCameraPitchOverride", (parser, x) => x.AltCameraPitchOverride = parser.ParseFloat() },
        };

        [AddedIn(SageGame.Bfme)]
        public bool UseLowLodTrees { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float LowLodTreeScale { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string LowLodTreeName { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string LowLodTreeNameNoGrab { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string LowLodTreeNameNoHarvest { get; private set; }

        public float StructureSeconds { get; private set; }
        public float TeamSeconds { get; private set; }
        public int Wealthy { get; private set; }
        public int Poor { get; private set; }
        public float StructuresWealthyRate { get; private set; }
        public float StructuresPoorRate { get; private set; }
        public float TeamsWealthyRate { get; private set; }
        public float TeamsPoorRate { get; private set; }
        public float TeamResourcesToStart { get; private set; }
        public float GuardInnerModifierAI { get; private set; }
        public float GuardOuterModifierAI { get; private set; }
        public float GuardInnerModifierHuman { get; private set; }
        public float GuardOuterModifierHuman { get; private set; }
        public int GuardChaseUnitsDuration { get; private set; }
        public int GuardEnemyScanRate { get; private set; }
        public int GuardEnemyReturnScanRate { get; private set; }
        public float AlertRangeModifier { get; private set; }
        public float AggressiveRangeModifier { get; private set; }
        public float AttackPriorityDistanceModifier { get; private set; }
        public float MaxRecruitRadius { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public float SkirmishBaseDefenseExtraDistance { get; private set; }

        public int ForceIdleMsec { get; private set; }
        public bool ForceSkirmishAI { get; private set; }
        public bool RotateSkirmishBases { get; private set; }
        public bool AttackUsesLineOfSight { get; private set; }

        public bool EnableRepulsors { get; private set; }
        public float RepulsedDistance { get; private set; }

        public int WallHeight { get; private set; }

        public bool AttackIgnoreInsignificantBuildings { get; private set; }

        public float SkirmishGroupFudgeDistance { get; private set; }

        public int MinInfantryForGroup { get; private set; }
        public int MinVehiclesForGroup { get; private set; }
        public float MinDistanceForGroup { get; private set; }
        public float DistanceRequiresGroup { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float FormationEnemyDistance { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float FormationColumnWidth { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float FormationRowDepth { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float FormationSquadSpacing { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int FormationColumns { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool UseFormations { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool WaitForOthers { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float NarrowPassageScale { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool HordesWaitForHordes { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool AttackMoveUsesFormations { get; private set; }

        public int InfantryPathfindDiameter { get; private set; }
        public int VehiclePathfindDiameter { get; private set; }

        public float SupplyCenterSafeRadius { get; private set; }
        public int RebuildDelayTimeSeconds { get; private set; }

        public float AIDozerBoredRadiusModifier { get; private set; }
        public bool AICrushesInfantry { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float MeleeApproachDist { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float MeleeApproachTolerance { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float MeleeAcquireLimitDist { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float WadeWaterDepth { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public float MaxRetaliationDistance { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public float RetaliationFriendsRadius { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float ChaseFromBehindLimit { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool ForceHordesToLowLod { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool AllowForestFires { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float CastleSiegeStandBackDistance { get; private set; }

        public List<AISideInfo> SideInfos { get; } = new List<AISideInfo>();

        public List<AISkirmishBuildList> SkirmishBuildLists { get; } = new List<AISkirmishBuildList>();

        [AddedIn(SageGame.Bfme)]
        public List<AttackPriority> AttackPriorities { get; } = new List<AttackPriority>();

        [AddedIn(SageGame.Bfme)]
        public float AltCameraZoomOverride { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float AltCameraPitchOverride { get; private set; }
    }

    public sealed class AISideInfo
    {
        internal static AISideInfo Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<AISideInfo> FieldParseTable = new IniParseTable<AISideInfo>
        {
            { "ResourceGatherersEasy", (parser, x) => x.ResourceGatherersEasy = parser.ParseInteger() },
            { "ResourceGatherersNormal", (parser, x) => x.ResourceGatherersNormal = parser.ParseInteger() },
            { "ResourceGatherersHard", (parser, x) => x.ResourceGatherersHard = parser.ParseInteger() },
            { "BaseDefenseStructure1", (parser, x) => x.BaseDefenseStructure1 = parser.ParseAssetReference() },

            { "SkillSet1", (parser, x) => x.SkillSet1 = AISkillSet.Parse(parser) },
            { "SkillSet2", (parser, x) => x.SkillSet1 = AISkillSet.Parse(parser) },
            { "SkillSet3", (parser, x) => x.SkillSet3 = AISkillSet.Parse(parser) }
        };

        public string Name { get; private set; }

        public int ResourceGatherersEasy { get; private set; }
        public int ResourceGatherersNormal { get; private set; }
        public int ResourceGatherersHard { get; private set; }
        public string BaseDefenseStructure1 { get; private set; }

        public AISkillSet SkillSet1 { get; private set; }
        public AISkillSet SkillSet2 { get; private set; }
        public AISkillSet SkillSet3 { get; private set; }
    }

    public sealed class AISkillSet
    {
        internal static AISkillSet Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<AISkillSet> FieldParseTable = new IniParseTable<AISkillSet>
        {
            { "Science", (parser, x) => x.Sciences.Add(parser.ParseAssetReference()) }
        };

        public List<string> Sciences { get; } = new List<string>();
    }

    public sealed class AISkirmishBuildList
    {
        internal static AISkirmishBuildList Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<AISkirmishBuildList> FieldParseTable = new IniParseTable<AISkirmishBuildList>
        {
            { "Structure", (parser, x) => x.Structures.Add(AIStructure.Parse(parser)) }
        };

        public string Name { get; private set; }

        public List<AIStructure> Structures { get; } = new List<AIStructure>();
    }

    public sealed class AIStructure
    {
        internal static AIStructure Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Key = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<AIStructure> FieldParseTable = new IniParseTable<AIStructure>
        {
            { "Name", (parser, x) => x.Name = parser.ParseString() },
            { "Location", (parser, x) => x.Location = parser.ParseVector2() },
            { "Rebuilds", (parser, x) => x.Rebuilds = parser.ParseInteger() },
            { "Angle", (parser, x) => x.Angle = parser.ParseFloat() },
            { "InitiallyBuilt", (parser, x) => x.InitiallyBuilt = parser.ParseBoolean() },
            { "AutomaticallyBuild", (parser, x) => x.AutomaticallyBuild = parser.ParseBoolean() },
        };

        public string Key { get; private set; }

        public string Name { get; private set; }
        public Vector2 Location { get; private set; }
        public int Rebuilds { get; private set; }
        public float Angle { get; private set; }
        public bool InitiallyBuilt { get; private set; }
        public bool AutomaticallyBuild { get; private set; }
    }
}
