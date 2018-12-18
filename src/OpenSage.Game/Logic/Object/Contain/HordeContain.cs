using System.Collections.Generic;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class HordeContainModuleData : BehaviorModuleData
    {
        internal static HordeContainModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<HordeContainModuleData> FieldParseTable = new IniParseTable<HordeContainModuleData>
        {
            { "ObjectStatusOfContained", (parser, x) => x.ObjectStatusOfContained = parser.ParseEnumBitArray<ObjectStatus>() },
            { "InitialPayload", (parser, x) => x.InitialPayloads.Add(Payload.Parse(parser)) },
            { "Slots", (parser, x) => x.Slots = parser.ParseInteger() },
            { "PassengerFilter", (parser, x) => x.PassengerFilter = ObjectFilter.Parse(parser) },
            { "ShowPips", (parser, x) => x.ShowPips = parser.ParseBoolean() }, 
            { "ThisFormationIsTheMainFormation", (parser, x) => x.ThisFormationIsTheMainFormation = parser.ParseBoolean() },
            { "RandomOffset", (parser, x ) => x.RandomOffset = parser.ParsePoint() },

            { "BannerCarriersAllowed", (parser, x) => x.BannerCarriersAllowed.AddRange(parser.ParseAssetReferenceArray()) },
            { "BannerCarrierPosition", (parser, x) => x.BannerCarrierPositions.Add(BannerCarrierPosition.Parse(parser)) },

            { "RankInfo", (parser, x) => x.RankInfos.Add(RankInfo.Parse(parser)) },

            { "RanksToReleaseWhenAttacking", (parser, x) => x.RanksToReleaseWhenAttacking = parser.ParseIntegerArray() },

            { "ComboHordes", (parser, x) => x.ComboHordes.Add(ComboHorde.Parse(parser)) },
            { "ComboHorde", (parser, x) => x.ComboHordes.Add(ComboHorde.Parse(parser)) },

            { "UseSlowHordeMovement", (parser, x) => x.UseSlowHordeMovement = parser.ParseBoolean() },

            { "MeleeAttackLeashDistance", (parser, x) =>  x.MeleeAttackLeashDistance = parser.ParseInteger() },
            { "MachineAllowed", (parser, x) => x.MachineAllowed = parser.ParseBoolean() },
            { "MachineType", (parser, x) => x.MachineType = parser.ParseAssetReference() },

            { "AlternateFormation", (parser, x) => x.AlternateFormation = parser.ParseAssetReference() },

            { "BackUpMinDelayTime", (parser, x) => x.BackUpMinDelayTime = parser.ParseInteger() },
            { "BackUpMaxDelayTime", (parser, x) => x.BackUpMaxDelayTime = parser.ParseInteger() },
            { "BackUpMinDistance", (parser, x) => x.BackUpMinDistance = parser.ParseInteger() },
            { "BackUpMaxDistance", (parser, x) => x.BackUpMaxDistance = parser.ParseInteger() },
            { "BackupPercentage", (parser, x) => x.BackupPercentage = parser.ParseFloat() },
            { "AttributeModifiers", (parser, x) => x.AttributeModifiers = parser.ParseAssetReferenceArray() },
            { "RanksThatStopAdvance", (parser, x) => x.RanksThatStopAdvance = parser.ParseInteger() },
            { "RanksToJustFreeWhenAttacking", (parser, x) => x.RanksToJustFreeWhenAttacking = parser.ParseInteger() },
            { "NotComboFormation", (parser, x) => x.NotComboFormation = parser.ParseBoolean() },
            { "UsePorcupineBody", (parser, x) => x.UsePorcupineBody = parser.ParseBoolean() },
            { "SplitHorde", (parser, x) => x.SplitHordes.Add(SplitHorde.Parse(parser)) },
            { "UseMarchingAnims", (parser, x) => x.UseMarchingAnims = parser.ParseBoolean() },
            { "ForcedLocomotorSet", (parser, x) => x.ForcedLocomotorSet = parser.ParseEnum<LocomotorSetCondition>() },
            { "UpdateWeaponSetFlagsOnHordeToo", (parser, x) => x.UpdateWeaponSetFlagsOnHordeToo = parser.ParseBoolean() },
            { "RankSplit", (parser, x) => x.RankSplit = parser.ParseBoolean() },
            { "SplitHordeNumber", (parser, x) => x.SplitHordeNumber = parser.ParseInteger() },
            { "FrontAngle", (parser, x) => x.FrontAngle = parser.ParseFloat() },
            { "FlankedDelay", (parser, x) => x.FlankedDelay = parser.ParseInteger() },
            { "MeleeBehavior", (parser, x) => x.MeleeBehavior = MeleeBehavior.Parse(parser) },
            { "IsPorcupineFormation", (parser, x) => x.IsPorcupineFormation = parser.ParseBoolean() },
            { "MinimumHordeSize", (parser, x) => x.MinimumHordeSize = parser.ParseInteger() },
            { "VisionRearOverride", (parser, x) => x.VisionRearOverride = parser.ParsePercentage() },
            { "VisionSideOverride", (parser, x) => x.VisionSideOverride = parser.ParsePercentage() },
        };

        public BitArray<ObjectStatus> ObjectStatusOfContained { get; private set; }
        public List<Payload> InitialPayloads { get; private set; } = new List<Payload>();
        public int Slots { get; private set; }
        public ObjectFilter PassengerFilter { get; private set; }
        public bool ShowPips { get; private set; }
        public bool ThisFormationIsTheMainFormation { get; private set; }
        public Point2D RandomOffset { get; private set; }

        // NOTE: Despite the name, in BFME1 this always contains just 1 entry.
        public List<string> BannerCarriersAllowed { get; private set; } = new List<string>();
        public List<BannerCarrierPosition> BannerCarrierPositions { get; private set; } = new List<BannerCarrierPosition>();

        public List<RankInfo> RankInfos { get; private set; } = new List<RankInfo>();

        public int[] RanksToReleaseWhenAttacking { get; private set; }

        public List<ComboHorde> ComboHordes { get; private set; } = new List<ComboHorde>();

        public bool UseSlowHordeMovement { get; private set; }

        public int MeleeAttackLeashDistance { get; private set; }
        public bool MachineAllowed { get; private set; }
        public string MachineType { get; private set; }

        public string AlternateFormation { get; private set; }

        public int BackUpMinDelayTime { get; private set; }
        public int BackUpMaxDelayTime { get; private set; }
        public int BackUpMinDistance { get; private set; }
        public int BackUpMaxDistance { get; private set; }
        public float BackupPercentage { get; private set; }
        public string[] AttributeModifiers { get; private set; }
        public int RanksThatStopAdvance { get; private set; }
        public int RanksToJustFreeWhenAttacking { get; private set; }
        public bool NotComboFormation { get; private set; }
        public bool UsePorcupineBody { get; private set; }
        public List<SplitHorde> SplitHordes { get; } = new List<SplitHorde>();
        public bool UseMarchingAnims { get; private set; }
        public LocomotorSetCondition ForcedLocomotorSet { get; private set; }
        public bool UpdateWeaponSetFlagsOnHordeToo { get; private set; }
        public bool RankSplit { get; private set; }
        public int SplitHordeNumber { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float FrontAngle { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int FlankedDelay { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public MeleeBehavior MeleeBehavior { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool IsPorcupineFormation { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int MinimumHordeSize { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float VisionRearOverride { get; private set; }

        [AddedIn(SageGame.Bfme2)]
		public float VisionSideOverride { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class Payload
    {
        internal static Payload Parse(IniParser parser)
        {
            return new Payload
            {
                UnitType = parser.ParseAssetReference(),
                Count = parser.ParseInteger()
            };
        }

        public string UnitType { get; private set; }
        public int Count { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class BannerCarrierPosition
    {
        internal static BannerCarrierPosition Parse(IniParser parser)
        {
            return new BannerCarrierPosition
            {
                UnitType = parser.ParseAttributeIdentifier("UnitType"),
                Position = parser.ParseAttributeVector2("Pos")
            };
        }

        public string UnitType { get; private set; }
        public Vector2 Position { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class RankInfo
    {
        internal static RankInfo Parse(IniParser parser) => parser.ParseAttributeList(FieldParseTable);

        internal static readonly IniParseTable<RankInfo> FieldParseTable = new IniParseTable<RankInfo>
        {
            { "RankNumber", (parser, x) => x.RankNumber = parser.ParseInteger() },
            { "UnitType", (parser, x) => x.UnitType = parser.ParseIdentifier() },
            { "Position", (parser, x) => x.Positions.Add(parser.ParseVector2()) },
            { "RevokedWeaponCondition", (parser, x) => x.RevokedWeaponCondition = parser.ParseEnum<WeaponSetConditions>() },
            { "GrantedWeaponCondition", (parser, x) => x.GrantedWeaponCondition = parser.ParseEnum<WeaponSetConditions>() },
            { "Leader", (parser, x) => x.Leaders.Add(Leader.Parse(parser)) },
        };

        public int RankNumber { get; private set; }
        public string UnitType { get; private set; }
        public List<Vector2> Positions { get; } = new List<Vector2>();
        public WeaponSetConditions RevokedWeaponCondition { get; private set; }
        public WeaponSetConditions GrantedWeaponCondition { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public List<Leader> Leaders { get; } = new List<Leader>();
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class ComboHorde
    {
        internal static ComboHorde Parse(IniParser parser)
        {
            return new ComboHorde
            {
                Target = parser.ParseAttributeIdentifier("Target"),
                Result = parser.ParseAttributeIdentifier("Result"),
                InitiateVoice = parser.ParseAttributeIdentifier("InitiateVoice")
            };
        }

        public string Target { get; private set; }
        public string Result { get; private set; }
        public string InitiateVoice { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class SplitHorde 
    {
        internal static SplitHorde Parse(IniParser parser)
        {
            return new SplitHorde
            {
                SplitResult = parser.ParseAttributeIdentifier("SplitResult"),
                UnitType = parser.ParseAttributeIdentifier("UnitType")
            };
        }

        public string SplitResult { get; private set; }
        public string UnitType { get; private set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public sealed class Leader
    {
        internal static Leader Parse(IniParser parser)
        {
            return new Leader
            {
                X = parser.ParseInteger(),
                Y = parser.ParseInteger(),
            };
        }

        public int X { get; private set; }
        public int Y { get; private set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public sealed class MeleeBehavior
    {
        internal static MeleeBehavior Parse(IniParser parser)
        {
            return parser.ParseNamedBlock((x, name) => x.Name = name, FieldParseTable);
        }

        internal static readonly IniParseTable<MeleeBehavior> FieldParseTable = new IniParseTable<MeleeBehavior>
        {
        };

        public string Name { get; private set; }
    }
}
