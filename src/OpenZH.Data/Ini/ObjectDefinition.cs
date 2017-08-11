using System;
using System.Collections;
using System.Collections.Generic;
using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
{
    public sealed class ObjectDefinition
    {
        internal static ObjectDefinition Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<ObjectDefinition> FieldParseTable = new IniParseTable<ObjectDefinition>
        {
            { "Behavior", (parser, x) => x.Behaviors.Add(ObjectBehavior.ParseBehavior(parser)) },
            { "DisplayName", (parser, x) => x.DisplayName = parser.ParseAsciiString() },
            { "Draw", (parser, x) => x.Draws.Add(ObjectDrawModule.ParseDrawModule(parser)) },
            { "EditorSorting", (parser, x) => x.EditorSorting = parser.ParseEnumFlags<ObjectEditorSortingFlags>() },
            { "KindOf", (parser, x) => x.KindOf = parser.ParseEnumBitArray<ObjectKinds>() },
            { "Geometry", (parser, x) => x.Geometry = parser.ParseEnum<ObjectGeometry>() },
            { "GeometryMajorRadius", (parser, x) => x.GeometryMajorRadius = parser.ParseFloat() },
            { "GeometryMinorRadius", (parser, x) => x.GeometryMinorRadius = parser.ParseFloat() },
            { "GeometryHeight", (parser, x) => x.GeometryHeight = parser.ParseFloat() },
            { "GeometryIsSmall", (parser, x) => x.GeometryIsSmall = parser.ParseBoolean() },
            { "Shadow", (parser, x) => x.Shadow = parser.ParseEnum<ObjectShadowType>() },
            { "TransportSlotCount", (parser, x) => x.TransportSlotCount = parser.ParseInteger() },
        };

        public string Name { get; private set; }

        public List<ObjectBehavior> Behaviors { get; } = new List<ObjectBehavior>();
        public string DisplayName { get; private set; }
        public List<ObjectDrawModule> Draws { get; } = new List<ObjectDrawModule>();
        public ObjectEditorSortingFlags EditorSorting { get; private set; }
        public BitArray<ObjectKinds> KindOf { get; private set; }
        public ObjectGeometry Geometry { get; private set; }
        public float GeometryMajorRadius { get; private set; }
        public float GeometryMinorRadius { get; private set; }
        public float GeometryHeight { get; private set; }
        public bool GeometryIsSmall { get; private set; }
        public ObjectShadowType Shadow { get; private set; }
        public int TransportSlotCount { get; private set; }
    }

    public abstract class ObjectDrawModule : ObjectModule
    {
        internal static ObjectDrawModule ParseDrawModule(IniParser parser) => ParseModule(parser, DrawModuleParseTable);

        internal static readonly Dictionary<string, Func<IniParser, ObjectDrawModule>> DrawModuleParseTable = new Dictionary<string, Func<IniParser, ObjectDrawModule>>
        {
            { "W3DModelDraw", W3dModelDraw.ParseModel },
            { "W3DScienceModelDraw", W3dScienceModelDraw.Parse },
            { "W3DTankDraw", W3dTankDraw.Parse },
        };
    }

    public class W3dModelDraw : ObjectDrawModule
    {
        internal static W3dModelDraw ParseModel(IniParser parser)
        {
            return parser.ParseBlock(ModelFieldParseTable);
        }

        internal static readonly IniParseTable<W3dModelDraw> ModelFieldParseTable = new IniParseTable<W3dModelDraw>
        {
            { "AnimationsRequirePower", (parser, x) => x.AnimationsRequirePower = parser.ParseBoolean() },
            { "DefaultConditionState", (parser, x) => x.DefaultConditionState = ObjectConditionState.Parse(parser) },
            { "ConditionState", (parser, x) => x.ConditionStates.Add(ObjectConditionState.Parse(parser)) },
            { "OkToChangeModelColor", (parser, x) => x.OkToChangeModelColor = parser.ParseBoolean() },
        };

        public ObjectConditionState DefaultConditionState { get; private set; }
        public List<ObjectConditionState> ConditionStates { get; } = new List<ObjectConditionState>();

        public bool OkToChangeModelColor { get; private set; }
        public bool AnimationsRequirePower { get; private set; }
    }

    public sealed class W3dTankDraw : W3dModelDraw
    {
        internal static W3dTankDraw Parse(IniParser parser)
        {
            return parser.ParseBlock(TankFieldParseTable);
        }

        private static readonly IniParseTable<W3dTankDraw> TankFieldParseTable = new IniParseTable<W3dTankDraw>
        {
            { "TrackMarks", (parser, x) => x.TrackMarks = parser.ParseAsciiString() },
            { "TreadDriveSpeedFraction", (parser, x) => x.TreadDriveSpeedFraction = parser.ParseFloat() },
        }.Concat<W3dTankDraw, W3dModelDraw>(ModelFieldParseTable);

        public string TrackMarks { get; private set; }
        public float TreadDriveSpeedFraction { get; private set; }
    }

    public sealed class W3dScienceModelDraw : W3dModelDraw
    {
        internal static W3dScienceModelDraw Parse(IniParser parser)
        {
            return parser.ParseBlock(ScienceModelFieldParseTable);
        }

        private static readonly IniParseTable<W3dScienceModelDraw> ScienceModelFieldParseTable = new IniParseTable<W3dScienceModelDraw>
        {
            { "RequiredScience", (parser, x) => x.RequiredScience = parser.ParseAsciiString() }
        }.Concat<W3dScienceModelDraw, W3dModelDraw>(ModelFieldParseTable);

        public string RequiredScience { get; private set; }
    }

    public sealed class ObjectConditionState
    {
        internal static ObjectConditionState ParseDefault(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        internal static ObjectConditionState Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<ObjectConditionState> FieldParseTable = new IniParseTable<ObjectConditionState>
        {
            { "Model", (parser, x) => x.Model = parser.ParseAsciiString() },
            { "Animation", (parser, x) => x.Animation = parser.ParseAsciiString() },
            { "AnimationMode", (parser, x) => x.AnimationMode = parser.ParseEnum<AnimationMode>() },
            { "Flags", (parser, x) => x.Flags = parser.ParseEnum<ObjectConditionStateFlags>() },
            { "ParticleSysBone", (parser, x) => x.ParticleSysBones.Add(ParticleSysBone.Parse(parser)) },
        };

        public string Model { get; private set; }
        public string Animation { get; private set; }
        public AnimationMode AnimationMode { get; private set; }
        public ObjectConditionStateFlags Flags { get; private set; }
        public List<ParticleSysBone> ParticleSysBones { get; } = new List<ParticleSysBone>();
    }

    public enum ModelConditionFlagType
    {
        [IniEnum("REALLYDAMAGED")]
        ReallyDamaged
    }

    public abstract class ObjectModule
    {
        internal static T ParseModule<T>(IniParser parser, Dictionary<string, Func<IniParser, T>> moduleParseTable)
            where T : ObjectModule
        {
            var moduleTypePosition = parser.CurrentPosition;
            var moduleType = parser.ParseIdentifier();
            var tag = parser.ParseIdentifier();

            if (!moduleParseTable.TryGetValue(moduleType, out var moduleParser))
            {
                throw new IniParseException($"Unknown module type: {moduleType}", moduleTypePosition);
            }

            var result = moduleParser(parser);

            result.Tag = tag;

            return result;
        }

        public string Tag { get; protected set; }
    }

    [Flags]
    public enum ObjectConditionStateFlags
    {
        None = 0,

        [IniEnum("RANDOMSTART")]
        RandomStart,

        [IniEnum("START_FRAME_FIRST")]
        StartFrameFirst
    }

    public sealed class ParticleSysBone
    {
        internal static ParticleSysBone Parse(IniParser parser)
        {
            return new ParticleSysBone
            {
                Bone = parser.ParseAsciiString(),
                ParticleSystem = parser.ParseAsciiString()
            };
        }

        public string Bone { get; private set; }
        public string ParticleSystem { get; private set; }
    }

    public abstract class ObjectBehavior : ObjectModule
    {
        internal static ObjectBehavior ParseBehavior(IniParser parser) => ParseModule(parser, BehaviorParseTable);

        private static readonly Dictionary<string, Func<IniParser, ObjectBehavior>> BehaviorParseTable = new Dictionary<string, Func<IniParser, ObjectBehavior>>
        {
            { "DeletionUpdate", DeletionUpdateBehavior.Parse },
            { "MoneyCrateCollide", MoneyCrateCollideBehavior.Parse },
            { "PhysicsBehavior", PhysicsBehavior.Parse },
            { "SalvageCrateCollide", SalvageCrateCollideBehavior.Parse },
            { "SquishCollide", SquishCollideBehavior.Parse },
            { "UnitCrateCollide", UnitCrateCollideBehavior.Parse },
            { "VeterancyCrateCollide", VeterancyCrateCollideBehavior.Parse },
        };
    }

    public sealed class MoneyCrateCollideBehavior : ObjectBehavior
    {
        internal static MoneyCrateCollideBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<MoneyCrateCollideBehavior> FieldParseTable = new IniParseTable<MoneyCrateCollideBehavior>
        {
            { "BuildingPickup", (parser, x) => x.BuildingPickup = parser.ParseBoolean() },
            { "ForbiddenKindOf", (parser, x) => x.ForbiddenKindOf = parser.ParseEnum<ObjectKinds>() },
            { "MoneyProvided", (parser, x) => x.MoneyProvided = parser.ParseInteger() },
            { "ExecuteAnimation", (parser, x) => x.ExecuteAnimation = parser.ParseAsciiString() },
            { "ExecuteAnimationTime", (parser, x) => x.ExecuteAnimationTime = parser.ParseFloat() },
            { "ExecuteAnimationZRise", (parser, x) => x.ExecuteAnimationZRise = parser.ParseFloat() },
            { "ExecuteAnimationFades", (parser, x) => x.ExecuteAnimationFades = parser.ParseBoolean() }
        };

        public bool BuildingPickup { get; private set; }
        public ObjectKinds ForbiddenKindOf { get; private set; }
        public int MoneyProvided { get; private set; }
        public string ExecuteAnimation { get; private set; }
        public float ExecuteAnimationTime { get; private set; }
        public float ExecuteAnimationZRise { get; private set; }
        public bool ExecuteAnimationFades { get; private set; }
    }

    public sealed class UnitCrateCollideBehavior : ObjectBehavior
    {
        internal static UnitCrateCollideBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<UnitCrateCollideBehavior> FieldParseTable = new IniParseTable<UnitCrateCollideBehavior>
        {
            { "ForbiddenKindOf", (parser, x) => x.ForbiddenKindOf = parser.ParseEnum<ObjectKinds>() },
            { "UnitCount", (parser, x) => x.UnitCount = parser.ParseInteger() },
            { "UnitName", (parser, x) => x.UnitName = parser.ParseAsciiString() }
        };

        public ObjectKinds ForbiddenKindOf { get; private set; }
        public int UnitCount { get; private set; }
        public string UnitName { get; private set; }
    }

    public sealed class VeterancyCrateCollideBehavior : ObjectBehavior
    {
        internal static VeterancyCrateCollideBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<VeterancyCrateCollideBehavior> FieldParseTable = new IniParseTable<VeterancyCrateCollideBehavior>
        {
            { "ForbiddenKindOf", (parser, x) => x.ForbiddenKindOf = parser.ParseEnum<ObjectKinds>() },
            { "EffectRange", (parser, x) => x.EffectRange = parser.ParseInteger() }
        };

        public ObjectKinds ForbiddenKindOf { get; private set; }
        public int EffectRange { get; private set; }
    }

    public sealed class SquishCollideBehavior : ObjectBehavior
    {
        internal static SquishCollideBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<SquishCollideBehavior> FieldParseTable = new IniParseTable<SquishCollideBehavior>();
    }

    public sealed class SalvageCrateCollideBehavior : ObjectBehavior
    {
        internal static SalvageCrateCollideBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<SalvageCrateCollideBehavior> FieldParseTable = new IniParseTable<SalvageCrateCollideBehavior>
        {
            { "ForbiddenKindOf", (parser, x) => x.ForbiddenKindOf = parser.ParseEnum<ObjectKinds>() },
            { "PickupScience", (parser, x) => x.PickupScience = parser.ParseAsciiString() },
            { "WeaponChance", (parser, x) => x.WeaponChance = parser.ParsePercentage() },
            { "LevelChance", (parser, x) => x.LevelChance = parser.ParsePercentage() },
            { "MoneyChance", (parser, x) => x.MoneyChance = parser.ParsePercentage() },
            { "MinMoney", (parser, x) => x.MinMoney = parser.ParseInteger() },
            { "MaxMoney", (parser, x) => x.MaxMoney = parser.ParseInteger() },
        };

        public ObjectKinds ForbiddenKindOf { get; private set; }
        public string PickupScience { get; private set; }
        public float WeaponChance { get; private set; }
        public float LevelChance { get; private set; }
        public float MoneyChance { get; private set; }
        public int MinMoney { get; private set; }
        public int MaxMoney { get; private set; }
    }

    public sealed class PhysicsBehavior : ObjectBehavior
    {
        internal static PhysicsBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<PhysicsBehavior> FieldParseTable = new IniParseTable<PhysicsBehavior>
        {
            { "Mass", (parser, x) => x.Mass = parser.ParseFloat() },
        };

        public float Mass { get; private set; }
    }

    public sealed class DeletionUpdateBehavior : ObjectBehavior
    {
        internal static DeletionUpdateBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<DeletionUpdateBehavior> FieldParseTable = new IniParseTable<DeletionUpdateBehavior>
        {
            { "MinLifetime", (parser, x) => x.MinLifetime = parser.ParseInteger() },
            { "MaxLifetime", (parser, x) => x.MaxLifetime = parser.ParseInteger() }
        };

        public int MinLifetime { get; private set; }
        public int MaxLifetime { get; private set; }
    }

    [Flags]
    public enum ObjectEditorSortingFlags
    {
        [IniEnum("NONE")]
        None = 0,

        [IniEnum("MISC_MAN_MADE")]
        MiscManMade    = 0x001,

        [IniEnum("MISC_NATURAL")]
        MiscNatural    = 0x002,

        [IniEnum("STRUCTURE")]
        Structure      = 0x004,

        [IniEnum("SYSTEM")]
        System         = 0x008,

        [IniEnum("CLEARED_BY_BUILD")]
        ClearedByBuild = 0x010,

        [IniEnum("VEHICLE")]
        Vehicle        = 0x020,

        [IniEnum("INFANTRY")]
        Infantry       = 0x040,

        [IniEnum("AUDIO")]
        Audio          = 0x080,

        [IniEnum("DEBRIS")]
        Debris         = 0x100,

        [IniEnum("SHRUBBERY")]
        Shrubbery      = 0x200
    }

    public enum ObjectGeometry
    {
        [IniEnum("BOX")]
        Box,

        [IniEnum("SPHERE")]
        Sphere,

        [IniEnum("CYLINDER")]
        Cylinder
    }

    public enum ObjectShadowType
    {
        [IniEnum("NONE")]
        None,

        [IniEnum("SHADOW_VOLUME")]
        ShadowVolume
    }
}
