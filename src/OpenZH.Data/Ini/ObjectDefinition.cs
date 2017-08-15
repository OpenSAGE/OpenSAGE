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
            { "PlacementViewAngle", (parser, x) => x.PlacementViewAngle = parser.ParseInteger() },
            { "SelectPortrait", (parser, x) => x.SelectPortrait = parser.ParseAssetReference() },
            { "ButtonImage", (parser, x) => x.ButtonImage = parser.ParseAssetReference() },

            { "Buildable", (parser, x) => x.Buildable = parser.ParseBoolean() },
            { "Side", (parser, x) => x.Side = parser.ParseAssetReference() },
            { "DisplayName", (parser, x) => x.DisplayName = parser.ParseLocalizedStringKey() },
            { "EditorSorting", (parser, x) => x.EditorSorting = parser.ParseEnumFlags<ObjectEditorSortingFlags>() },
            { "TransportSlotCount", (parser, x) => x.TransportSlotCount = parser.ParseInteger() },
            { "VisionRange", (parser, x) => x.VisionRange = parser.ParseFloat() },
            { "ShroudClearingRange", (parser, x) => x.ShroudClearingRange = parser.ParseFloat() },
            { "CrusherLevel", (parser, x) => x.CrusherLevel = parser.ParseInteger() },
            { "CrushableLevel", (parser, x) => x.CrushableLevel = parser.ParseInteger() },
            { "BuildCost", (parser, x) => x.BuildCost = parser.ParseInteger() },
            { "BuildTime", (parser, x) => x.BuildTime = parser.ParseInteger() },
            { "RefundValue", (parser, x) => x.RefundValue = parser.ParseInteger() },
            { "EnergyProduction", (parser, x) => x.EnergyProduction = parser.ParseInteger() },
            { "IsForbidden", (parser, x) => x.IsForbidden = parser.ParseBoolean() },
            { "IsBridge", (parser, x) => x.IsBridge = parser.ParseBoolean() },
            { "IsPrerequisite", (parser, x) => x.IsPrerequisite = parser.ParseBoolean() },
            { "WeaponSet", (parser, x) => x.WeaponSets.Add(WeaponSet.Parse(parser)) },
            { "ArmorSet", (parser, x) => x.ArmorSets.Add(ArmorSet.Parse(parser)) },
            { "CommandSet", (parser, x) => x.CommandSet = parser.ParseLocalizedStringKey() },
            { "Prequisites", (parser, x) => x.Prequisites = ObjectPrequisites.Parse(parser) },
            { "IsTrainable", (parser, x) => x.IsTrainable = parser.ParseBoolean() },

            { "VoiceSelect", (parser, x) => x.VoiceSelect = parser.ParseAssetReference() },
            { "VoiceMove", (parser, x) => x.VoiceMove = parser.ParseAssetReference() },
            { "VoiceGuard", (parser, x) => x.VoiceGuard = parser.ParseAssetReference() },
            { "VoiceAttack", (parser, x) => x.VoiceAttack = parser.ParseAssetReference() },
            { "VoiceEnter", (parser, x) => x.VoiceEnter = parser.ParseAssetReference() },
            { "VoiceFear", (parser, x) => x.VoiceFear = parser.ParseAssetReference() },
            { "VoiceSelectElite", (parser, x) => x.VoiceSelectElite = parser.ParseAssetReference() },
            { "VoiceCreated", (parser, x) => x.VoiceCreated = parser.ParseAssetReference() },
            { "VoiceTaskUnable", (parser, x) => x.VoiceTaskUnable = parser.ParseAssetReference() },
            { "VoiceTaskComplete", (parser, x) => x.VoiceTaskComplete = parser.ParseAssetReference() },
            { "VoiceMeetEnemy", (parser, x) => x.VoiceMeetEnemy = parser.ParseAssetReference() },
            { "SoundMoveStart", (parser, x) => x.SoundMoveStart = parser.ParseAssetReference() },
            { "SoundMoveLoop", (parser, x) => x.SoundMoveLoop = parser.ParseAssetReference() },
            { "SoundDie", (parser, x) => x.SoundDie = parser.ParseAssetReference() },
            { "SoundDieFire", (parser, x) => x.SoundDieFire = parser.ParseAssetReference() },
            { "SoundDieToxin", (parser, x) => x.SoundDieToxin = parser.ParseAssetReference() },
            { "SoundStealthOn", (parser, x) => x.SoundStealthOn = parser.ParseAssetReference() },
            { "SoundStealthOff", (parser, x) => x.SoundStealthOff = parser.ParseAssetReference() },
            { "SoundCrush", (parser, x) => x.SoundCrush = parser.ParseAssetReference() },
            { "SoundAmbient", (parser, x) => x.SoundAmbient = parser.ParseAssetReference() },
            { "SoundCreated", (parser, x) => x.SoundCreated = parser.ParseAssetReference() },
            { "UnitSpecificSounds", (parser, x) => x.UnitSpecificSounds = UnitSpecificSounds.Parse(parser) },

            { "Behavior", (parser, x) => x.Behaviors.Add(ObjectBehavior.ParseBehavior(parser)) },
            { "Draw", (parser, x) => x.Draws.Add(ObjectDrawModule.ParseDrawModule(parser)) },
            { "Body", (parser, x) => x.Bodies.Add(ObjectBody.ParseBody(parser)) },
            { "InheritableModule", (parser, x) => x.InheritableModule = InheritableModule.Parse(parser) },
            { "KindOf", (parser, x) => x.KindOf = parser.ParseEnumBitArray<ObjectKinds>() },
            { "RadarPriority", (parser, x) => x.RadarPriority = parser.ParseEnum<RadarPriority>() },
            { "DisplayColor", (parser, x) => x.DisplayColor = IniColorRgb.Parse(parser) },
            { "Scale", (parser, x) => x.Scale = parser.ParseFloat() },
            { "Geometry", (parser, x) => x.Geometry = parser.ParseEnum<ObjectGeometry>() },
            { "GeometryMajorRadius", (parser, x) => x.GeometryMajorRadius = parser.ParseFloat() },
            { "GeometryMinorRadius", (parser, x) => x.GeometryMinorRadius = parser.ParseFloat() },
            { "GeometryHeight", (parser, x) => x.GeometryHeight = parser.ParseFloat() },
            { "GeometryIsSmall", (parser, x) => x.GeometryIsSmall = parser.ParseBoolean() },
            { "Shadow", (parser, x) => x.Shadow = parser.ParseEnum<ObjectShadowType>() },
            { "ShadowTexture", (parser, x) => x.ShadowTexture = parser.ParseAssetReference() },
            { "ShadowSizeX", (parser, x) => x.ShadowSizeX = parser.ParseInteger() },
            { "ShadowSizeY", (parser, x) => x.ShadowSizeY = parser.ParseInteger() },
            { "BuildCompletion", (parser, x) => x.BuildCompletion = parser.ParseAssetReference() },
        };

        public string Name { get; private set; }

        // Art
        public int PlacementViewAngle { get; private set; }
        public string SelectPortrait { get; private set; }
        public string ButtonImage { get; private set; }

        // Design
        public bool Buildable { get; private set; }
        public string Side { get; private set; }
        public string DisplayName { get; private set; }
        public ObjectEditorSortingFlags EditorSorting { get; private set; }
        public int TransportSlotCount { get; private set; }
        public float VisionRange { get; private set; }
        public float ShroudClearingRange { get; private set; }
        public int CrusherLevel { get; private set; }
        public int CrushableLevel { get; private set; }
        public int BuildCost { get; private set; }
        public int BuildTime { get; private set; }
        public int RefundValue { get; private set; }
        public int EnergyProduction { get; private set; }
        public bool IsForbidden { get; private set; }
        public bool IsBridge { get; private set; }
        public bool IsPrerequisite { get; private set; }
        public List<WeaponSet> WeaponSets { get; } = new List<WeaponSet>();
        public List<ArmorSet> ArmorSets { get; } = new List<ArmorSet>();
        public string CommandSet { get; private set; }
        public ObjectPrequisites Prequisites { get; private set; }
        public bool IsTrainable { get; private set; }

        // Audio
        public string VoiceSelect { get; private set; }
        public string VoiceMove { get; private set; }
        public string VoiceGuard { get; private set; }
        public string VoiceAttack { get; private set; }
        public string VoiceEnter { get; private set; }
        public string VoiceFear { get; private set; }
        public string VoiceSelectElite { get; private set; }
        public string VoiceCreated { get; private set; }
        public string VoiceTaskUnable { get; private set; }
        public string VoiceTaskComplete { get; private set; }
        public string VoiceMeetEnemy { get; private set; }
        public string SoundMoveStart { get; private set; }
        public string SoundMoveLoop { get; private set; }
        public string SoundDie { get; private set; }
        public string SoundDieFire { get; private set; }
        public string SoundDieToxin { get; private set; }
        public string SoundStealthOn { get; private set; }
        public string SoundStealthOff { get; private set; }
        public string SoundCrush { get; private set; }
        public string SoundAmbient { get; private set; }
        public string SoundCreated { get; private set; }
        public UnitSpecificSounds UnitSpecificSounds { get; private set; }

        // Engineering
        public List<ObjectBehavior> Behaviors { get; } = new List<ObjectBehavior>();
        public List<ObjectDrawModule> Draws { get; } = new List<ObjectDrawModule>();
        public List<ObjectBody> Bodies { get; } = new List<ObjectBody>();
        public InheritableModule InheritableModule { get; private set; }
        public BitArray<ObjectKinds> KindOf { get; private set; }
        public RadarPriority RadarPriority { get; private set; }
        public IniColorRgb DisplayColor { get; private set; }
        public float Scale { get; private set; }
        public ObjectGeometry Geometry { get; private set; }
        public float GeometryMajorRadius { get; private set; }
        public float GeometryMinorRadius { get; private set; }
        public float GeometryHeight { get; private set; }
        public bool GeometryIsSmall { get; private set; }
        public ObjectShadowType Shadow { get; private set; }
        public string ShadowTexture { get; private set; }
        public int ShadowSizeX { get; private set; }
        public int ShadowSizeY { get; private set; }
        public string BuildCompletion { get; private set; }
    }

    /// <summary>
    /// Modules in here are not removed by default when copied from the default object.
    /// </summary>
    public sealed class InheritableModule
    {
        internal static InheritableModule Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<InheritableModule> FieldParseTable = new IniParseTable<InheritableModule>
        {
            { "Behavior", (parser, x) => x.Behaviors.Add(ObjectBehavior.ParseBehavior(parser)) },
        };

        public List<ObjectBehavior> Behaviors { get; } = new List<ObjectBehavior>();
    }

    public sealed class ObjectPrequisites
    {
        internal static ObjectPrequisites Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<ObjectPrequisites> FieldParseTable = new IniParseTable<ObjectPrequisites>
        {
            { "Object", (parser, x) => x.Objects.Add(parser.ParseAssetReference()) },
            { "Science", (parser, x) => x.Sciences.Add(parser.ParseAssetReference()) }
        };

        public List<string> Objects { get; } = new List<string>();
        public List<string> Sciences { get; } = new List<string>();
    }

    public sealed class UnitSpecificSounds
    {
        internal static UnitSpecificSounds Parse(IniParser parser)
        {
            parser.NextToken(IniTokenType.EndOfLine);

            var result = new UnitSpecificSounds();

            while (parser.Current.TokenType == IniTokenType.Identifier)
            {
                if (parser.Current.TokenType == IniTokenType.Identifier && parser.Current.StringValue.ToUpper() == "END")
                {
                    parser.NextToken();
                    break;
                }
                else
                {
                    var fieldName = parser.Current.StringValue;

                    parser.NextToken();
                    parser.NextToken(IniTokenType.Equals);

                    result.Sounds[fieldName] = parser.ParseAssetReference();

                    parser.NextToken(IniTokenType.EndOfLine);
                }
            }

            return result;
        }

        // These keys will eventually mean something to some code, as noted in FactionUnit.ini:32029.
        public Dictionary<string, string> Sounds { get; } = new Dictionary<string, string>();
    }

    public enum RadarPriority
    {
        [IniEnum("INVALID")]
        Invalid = 0,

        [IniEnum("LOCAL_UNIT_ONLY")]
        LocalUnitOnly,

        [IniEnum("STRUCTURE")]
        Structure
    }

    public sealed class ArmorSet
    {
        internal static ArmorSet Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<ArmorSet> FieldParseTable = new IniParseTable<ArmorSet>
        {
            { "Conditions", (parser, x) => x.Conditions = parser.ParseEnumFlags<ArmorSetConditions>() },
            { "Armor", (parser, x) => x.Armor = parser.ParseAssetReference() },
            { "DamageFX", (parser, x) => x.DamageFX = parser.ParseAssetReference() },
        };

        public ArmorSetConditions Conditions { get; private set; }
        public string Armor { get; private set; }
        public string DamageFX { get; private set; }
    }

    [Flags]
    public enum ArmorSetConditions
    {
        [IniEnum("None")]
        None = 0,


    }

    public sealed class WeaponSet
    {
        internal static WeaponSet Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<WeaponSet> FieldParseTable = new IniParseTable<WeaponSet>
        {
            { "Conditions", (parser, x) => x.Conditions = parser.ParseEnumFlags<WeaponSetConditions>() },
            { "Weapon", (parser, x) => x.ParseWeaponSlotProperty(parser, s => s.Weapon = parser.ParseAssetReference()) },
            { "PreferredAgainst", (parser, x) => x.ParseWeaponSlotProperty(parser, s => s.PreferredAgainst = parser.ParseEnumBitArray<ObjectKinds>()) },
            { "AutoChooseSources", (parser, x) => x.ParseWeaponSlotProperty(parser, s => s.AutoChooseSources = parser.ParseEnumFlags<CommandSourceTypes>()) },
            { "ShareWeaponReloadTime", (parser, x) => x.ShareWeaponReloadTime = parser.ParseBoolean() },
        };

        public WeaponSetConditions Conditions { get; private set; }
        public Dictionary<WeaponSlot, WeaponSetSlot> Slots { get; } = new Dictionary<WeaponSlot, WeaponSetSlot>();
        public bool ShareWeaponReloadTime { get; private set; }

        private void ParseWeaponSlotProperty(IniParser parser, Action<WeaponSetSlot> parseValue)
        {
            var weaponSlot = parser.ParseEnum<WeaponSlot>();

            if (!Slots.TryGetValue(weaponSlot, out var weaponSetSlot))
            {
                Slots.Add(weaponSlot, weaponSetSlot = new WeaponSetSlot());
            }

            parseValue(weaponSetSlot);
        }
    }

    public sealed class WeaponSetSlot
    {
        public string Weapon { get; internal set; }
        public CommandSourceTypes AutoChooseSources { get; internal set; }
        public BitArray<ObjectKinds> PreferredAgainst { get; internal set; }
    }

    [Flags]
    public enum WeaponSetConditions
    {
        [IniEnum("None")]
        None = 0,

        [IniEnum("PLAYER_UPGRADE")]
        PlayerUpgrade = 1 << 0
    }

    [Flags]
    public enum CommandSourceTypes
    {
        None = 0,

        [IniEnum("FROM_PLAYER")]
        FromPlayer = 1 << 0,

        [IniEnum("FROM_AI")]
        FromAI = 1 << 1,

        [IniEnum("FROM_SCRIPT")]
        FromScript = 1 << 2
    }

    public abstract class ObjectDrawModule : ObjectModule
    {
        internal static ObjectDrawModule ParseDrawModule(IniParser parser) => ParseModule(parser, DrawModuleParseTable);

        internal static readonly Dictionary<string, Func<IniParser, ObjectDrawModule>> DrawModuleParseTable = new Dictionary<string, Func<IniParser, ObjectDrawModule>>
        {
            { "W3DDefaultDraw", W3dDefaultDraw.Parse},
            { "W3DModelDraw", W3dModelDraw.ParseModel },
            { "W3DScienceModelDraw", W3dScienceModelDraw.Parse },
            { "W3DTankDraw", W3dTankDraw.Parse },
        };
    }

    /// <summary>
    /// All world objects should use a draw module. This module is used where an object should 
    /// never actually be drawn due either to the nature or type of the object or because its 
    /// drawing is handled by other logic, e.g. bridges.
    /// </summary>
    public sealed class W3dDefaultDraw : ObjectDrawModule
    {
        internal static W3dDefaultDraw Parse(IniParser parser)
        {
            return parser.ParseBlock(DefaultFieldParseTable);
        }

        internal static readonly IniParseTable<W3dDefaultDraw> DefaultFieldParseTable = new IniParseTable<W3dDefaultDraw>();
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
            { "TrackMarks", (parser, x) => x.TrackMarks = parser.ParseFileName() },
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
            { "RequiredScience", (parser, x) => x.RequiredScience = parser.ParseAssetReference() }
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
            { "Model", (parser, x) => x.Model = parser.ParseFileName() },
            { "Animation", (parser, x) => x.Animation = parser.ParseAnimationName() },
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
                Bone = parser.ParseBoneName(),
                ParticleSystem = parser.ParseAssetReference()
            };
        }

        public string Bone { get; private set; }
        public string ParticleSystem { get; private set; }
    }

    public abstract class ObjectBody : ObjectModule
    {
        internal static ObjectBody ParseBody(IniParser parser) => ParseModule(parser, BodyParseTable);

        private static readonly Dictionary<string, Func<IniParser, ObjectBody>> BodyParseTable = new Dictionary<string, Func<IniParser, ObjectBody>>
        {
            { "HighlanderBody", HighlanderBody.Parse },
            { "ImmortalBody", ImmortalBody.Parse },
            { "InactiveBody", InactiveBody.Parse },
        };
    }

    /// <summary>
    /// Allows the object to take damage but not die. The object will only die from irresistable damage.
    /// </summary>
    public sealed class HighlanderBody : ObjectBody
    {
        internal static HighlanderBody Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<HighlanderBody> FieldParseTable = new IniParseTable<HighlanderBody>
        {
            { "MaxHealth", (parser, x) => x.MaxHealth = parser.ParseFloat() },
            { "InitialHealth", (parser, x) => x.InitialHealth = parser.ParseFloat() }
        };

        public float MaxHealth { get; private set; }
        public float InitialHealth { get; private set; }
    }

    /// <summary>
    /// Prevents the object from dying or taking damage.
    /// </summary>
    public sealed class ImmortalBody : ObjectBody
    {
        internal static ImmortalBody Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<ImmortalBody> FieldParseTable = new IniParseTable<ImmortalBody>
        {
            { "MaxHealth", (parser, x) => x.MaxHealth = parser.ParseFloat() },
            { "InitialHealth", (parser, x) => x.InitialHealth = parser.ParseFloat() }
        };

        public float MaxHealth { get; private set; }
        public float InitialHealth { get; private set; }
    }

    /// <summary>
    /// Prevents normal interaction with other objects.
    /// </summary>
    public sealed class InactiveBody : ObjectBody
    {
        internal static InactiveBody Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<InactiveBody> FieldParseTable = new IniParseTable<InactiveBody>();
    }

    public abstract class ObjectBehavior : ObjectModule
    {
        internal static ObjectBehavior ParseBehavior(IniParser parser) => ParseModule(parser, BehaviorParseTable);

        private static readonly Dictionary<string, Func<IniParser, ObjectBehavior>> BehaviorParseTable = new Dictionary<string, Func<IniParser, ObjectBehavior>>
        {
            { "AutoHealBehavior", AutoHealBehavior.Parse },
            { "DeletionUpdate", DeletionUpdateBehavior.Parse },
            { "DestroyDie", DestroyDieBehavior.Parse },
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
            { "ExecuteAnimation", (parser, x) => x.ExecuteAnimation = parser.ParseAssetReference() },
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

    public sealed class DestroyDieBehavior : ObjectBehavior
    {
        internal static DestroyDieBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<DestroyDieBehavior> FieldParseTable = new IniParseTable<DestroyDieBehavior>
        {
            { "DeathTypes", (parser, x) => x.DeathTypes = parser.ParseEnumBitArray<DeathType>() }
        };

        public BitArray<DeathType> DeathTypes { get; private set; }
    }

    public sealed class AutoHealBehavior : ObjectBehavior
    {
        internal static AutoHealBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AutoHealBehavior> FieldParseTable = new IniParseTable<AutoHealBehavior>
        {
            { "HealingAmount", (parser, x) => x.HealingAmount = parser.ParseInteger() },
            { "HealingDelay", (parser, x) => x.HealingDelay = parser.ParseInteger() },
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReferenceArray() },
            { "StartHealingDelay", (parser, x) => x.StartHealingDelay = parser.ParseInteger() },
        };

        public int HealingAmount { get; private set; }
        public int HealingDelay { get; private set; }
        public string[] TriggeredBy { get; private set; }
        public int StartHealingDelay { get; private set; }
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
            { "UnitName", (parser, x) => x.UnitName = parser.ParseAssetReference() }
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
            { "PickupScience", (parser, x) => x.PickupScience = parser.ParseAssetReference() },
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
