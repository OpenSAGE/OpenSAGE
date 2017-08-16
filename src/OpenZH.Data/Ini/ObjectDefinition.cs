using System;
using System.Collections.Generic;
using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
{
    public class ObjectDefinition
    {
        internal static ObjectDefinition Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        internal static readonly IniParseTable<ObjectDefinition> FieldParseTable = new IniParseTable<ObjectDefinition>
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
            { "BuildTime", (parser, x) => x.BuildTime = parser.ParseFloat() },
            { "RefundValue", (parser, x) => x.RefundValue = parser.ParseInteger() },
            { "EnergyProduction", (parser, x) => x.EnergyProduction = parser.ParseInteger() },
            { "IsForbidden", (parser, x) => x.IsForbidden = parser.ParseBoolean() },
            { "IsBridge", (parser, x) => x.IsBridge = parser.ParseBoolean() },
            { "IsPrerequisite", (parser, x) => x.IsPrerequisite = parser.ParseBoolean() },
            { "WeaponSet", (parser, x) => x.WeaponSets.Add(WeaponSet.Parse(parser)) },
            { "ArmorSet", (parser, x) => x.ArmorSets.Add(ArmorSet.Parse(parser)) },
            { "CommandSet", (parser, x) => x.CommandSet = parser.ParseLocalizedStringKey() },
            { "Prerequisites", (parser, x) => x.Prerequisites = ObjectPrerequisites.Parse(parser) },
            { "IsTrainable", (parser, x) => x.IsTrainable = parser.ParseBoolean() },
            { "FenceWidth", (parser, x) => x.FenceWidth = parser.ParseFloat() },
            { "FenceXOffset", (parser, x) => x.FenceXOffset = parser.ParseFloat() },
            { "ExperienceValue", (parser, x) => x.ExperienceValue = VeterancyValues.Parse(parser) },
            { "ExperienceRequired", (parser, x) => x.ExperienceRequired = VeterancyValues.Parse(parser) },

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
            { "SoundMoveStartDamaged", (parser, x) => x.SoundMoveStart = parser.ParseAssetReference() },
            { "SoundMoveLoop", (parser, x) => x.SoundMoveLoop = parser.ParseAssetReference() },
            { "SoundOnDamaged", (parser, x) => x.SoundOnDamaged = parser.ParseAssetReference() },
            { "SoundOnReallyDamaged", (parser, x) => x.SoundOnReallyDamaged = parser.ParseAssetReference() },
            { "SoundDie", (parser, x) => x.SoundDie = parser.ParseAssetReference() },
            { "SoundDieFire", (parser, x) => x.SoundDieFire = parser.ParseAssetReference() },
            { "SoundDieToxin", (parser, x) => x.SoundDieToxin = parser.ParseAssetReference() },
            { "SoundStealthOn", (parser, x) => x.SoundStealthOn = parser.ParseAssetReference() },
            { "SoundStealthOff", (parser, x) => x.SoundStealthOff = parser.ParseAssetReference() },
            { "SoundCrush", (parser, x) => x.SoundCrush = parser.ParseAssetReference() },
            { "SoundAmbient", (parser, x) => x.SoundAmbient = parser.ParseAssetReference() },
            { "SoundAmbientDamaged", (parser, x) => x.SoundAmbientDamaged = parser.ParseAssetReference() },
            { "SoundAmbientReallyDamaged", (parser, x) => x.SoundAmbientReallyDamaged = parser.ParseAssetReference() },
            { "SoundAmbientRubble", (parser, x) => x.SoundAmbientRubble = parser.ParseAssetReference() },
            { "SoundCreated", (parser, x) => x.SoundCreated = parser.ParseAssetReference() },
            { "UnitSpecificSounds", (parser, x) => x.UnitSpecificSounds = UnitSpecificSounds.Parse(parser) },

            { "Behavior", (parser, x) => x.Behaviors.Add(ObjectBehavior.ParseBehavior(parser)) },
            { "Draw", (parser, x) => x.Draws.Add(ObjectDrawModule.ParseDrawModule(parser)) },
            { "Body", (parser, x) => x.Body = ObjectBody.ParseBody(parser) },
            { "ClientUpdate", (parser, x) => x.ClientUpdates.Add(ClientUpdate.ParseClientUpdate(parser)) },
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

        public string Name { get; protected set; }

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

        /// <summary>
        /// Build time in seconds.
        /// </summary>
        public float BuildTime { get; private set; }
        public int RefundValue { get; private set; }
        public int EnergyProduction { get; private set; }
        public bool IsForbidden { get; private set; }
        public bool IsBridge { get; private set; }
        public bool IsPrerequisite { get; private set; }
        public List<WeaponSet> WeaponSets { get; } = new List<WeaponSet>();
        public List<ArmorSet> ArmorSets { get; } = new List<ArmorSet>();
        public string CommandSet { get; private set; }
        public ObjectPrerequisites Prerequisites { get; private set; }
        public bool IsTrainable { get; private set; }

        /// <summary>
        /// Spacing used by fence tool in WorldBuilder.
        /// </summary>
        public float FenceWidth { get; private set; }

        /// <summary>
        /// Offset used by fence tool in WorldBuilder, to ensure that corners line up.
        /// </summary>
        public float FenceXOffset { get; private set; }

        /// <summary>
        /// Experience points given off when this object is destroyed.
        /// </summary>
        public VeterancyValues ExperienceValue { get; private set; }

        /// <summary>
        /// Experience points required to be promoted to next level.
        /// </summary>
        public VeterancyValues ExperienceRequired { get; private set; }

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
        public string SoundMoveStartDamaged { get; private set; }
        public string SoundMoveLoop { get; private set; }
        public string SoundOnDamaged { get; private set; }
        public string SoundOnReallyDamaged { get; private set; }
        public string SoundDie { get; private set; }
        public string SoundDieFire { get; private set; }
        public string SoundDieToxin { get; private set; }
        public string SoundStealthOn { get; private set; }
        public string SoundStealthOff { get; private set; }
        public string SoundCrush { get; private set; }
        public string SoundAmbient { get; private set; }
        public string SoundAmbientDamaged { get; private set; }
        public string SoundAmbientReallyDamaged { get; private set; }
        public string SoundAmbientRubble { get; private set; }
        public string SoundCreated { get; private set; }
        public UnitSpecificSounds UnitSpecificSounds { get; private set; }

        // Engineering
        public List<ObjectBehavior> Behaviors { get; } = new List<ObjectBehavior>();
        public List<ObjectDrawModule> Draws { get; } = new List<ObjectDrawModule>();
        public ObjectBody Body { get; private set; }
        public List<ClientUpdate> ClientUpdates { get; } = new List<ClientUpdate>();
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

    public struct VeterancyValues
    {
        internal static VeterancyValues Parse(IniParser parser)
        {
            return new VeterancyValues
            {
                Regular = parser.ParseInteger(),
                Veteran = parser.ParseInteger(),
                Elite = parser.ParseInteger(),
                Heroic = parser.ParseInteger(),
            };
        }

        public int Regular { get; private set; }
        public int Veteran { get; private set; }
        public int Elite { get; private set; }
        public int Heroic { get; private set; }
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

    public sealed class ObjectPrerequisites
    {
        internal static ObjectPrerequisites Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<ObjectPrerequisites> FieldParseTable = new IniParseTable<ObjectPrerequisites>
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
        Structure,

        [IniEnum("UNIT")]
        Unit,

        [IniEnum("NOT_ON_RADAR")]
        NotOnRadar
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

        [IniEnum("PLAYER_UPGRADE")]
        PlayerUpgrade,
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

    public abstract class ClientUpdate : ObjectModule
    {
        internal static ClientUpdate ParseClientUpdate(IniParser parser) => ParseModule(parser, ClientUpdateParseTable);

        private static readonly Dictionary<string, Func<IniParser, ClientUpdate>> ClientUpdateParseTable = new Dictionary<string, Func<IniParser, ClientUpdate>>
        {
            { "AnimatedParticleSysBoneClientUpdate", AnimatedParticleSysBoneClientUpdate.Parse },
        };
    }

    /// <summary>
    /// Allows the object to have particle system effects dynamically attached to animated 
    /// sub objects or bones.
    /// </summary>
    public sealed class AnimatedParticleSysBoneClientUpdate : ClientUpdate
    {
        internal static AnimatedParticleSysBoneClientUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<AnimatedParticleSysBoneClientUpdate> FieldParseTable = new IniParseTable<AnimatedParticleSysBoneClientUpdate>();
    }

    public abstract class ObjectDrawModule : ObjectModule
    {
        internal static ObjectDrawModule ParseDrawModule(IniParser parser) => ParseModule(parser, DrawModuleParseTable);

        internal static readonly Dictionary<string, Func<IniParser, ObjectDrawModule>> DrawModuleParseTable = new Dictionary<string, Func<IniParser, ObjectDrawModule>>
        {
            { "W3DDefaultDraw", W3dDefaultDraw.Parse},
            { "W3DModelDraw", W3dModelDraw.ParseModel },
            { "W3DScienceModelDraw", W3dScienceModelDraw.Parse },
            { "W3DSupplyDraw", W3dSupplyDraw.Parse },
            { "W3DTankDraw", W3dTankDraw.Parse },
            { "W3DTruckDraw", W3dTruckDraw.Parse },
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
            { "DefaultConditionState", (parser, x) => x.DefaultConditionState = ObjectConditionState.ParseDefault(parser) },
            { "ConditionState", (parser, x) => x.ConditionStates.Add(ObjectConditionState.Parse(parser)) },
            { "AliasConditionState", (parser, x) => x.ParseAliasConditionState(parser) },
            { "OkToChangeModelColor", (parser, x) => x.OkToChangeModelColor = parser.ParseBoolean() },
            { "ExtraPublicBone", (parser, x) => x.ExtraPublicBones.Add(parser.ParseBoneName()) },
        };

        public ObjectConditionState DefaultConditionState { get; private set; }
        public List<ObjectConditionState> ConditionStates { get; } = new List<ObjectConditionState>();

        public bool OkToChangeModelColor { get; private set; }
        public bool AnimationsRequirePower { get; private set; }
        public List<string> ExtraPublicBones { get; } = new List<string>();

        private void ParseAliasConditionState(IniParser parser)
        {
            if (ConditionStates.Count == 0)
            {
                throw new IniParseException("Cannot use AliasConditionState if there are no preceding ConditionStates", parser.CurrentPosition);
            }

            var lastConditionState = ConditionStates[ConditionStates.Count - 1];

            var conditionFlags = parser.ParseEnumBitArray<ModelConditionFlag>();

            var aliasedConditionState = lastConditionState.Clone(conditionFlags);

            ConditionStates.Add(aliasedConditionState);
        }
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

    public sealed class W3dSupplyDraw : W3dModelDraw
    {
        internal static W3dSupplyDraw Parse(IniParser parser)
        {
            return parser.ParseBlock(SupplyFieldParseTable);
        }

        private static readonly IniParseTable<W3dSupplyDraw> SupplyFieldParseTable = new IniParseTable<W3dSupplyDraw>
        {
            { "SupplyBonePrefix", (parser, x) => x.SupplyBonePrefix = parser.ParseString() }
        }.Concat<W3dSupplyDraw, W3dModelDraw>(ModelFieldParseTable);

        public string SupplyBonePrefix { get; private set; }
    }

    /// <summary>
    /// Hardcoded to call for the TreadDebrisRight and TreadDebrisLeft (unless overriden) particle 
    /// system definitions and allows use of TruckPowerslideSound and TruckLandingSound within the 
    /// UnitSpecificSounds section of the object.
    /// 
    /// This module also includes automatic logic for showing and hiding of HEADLIGHT bones in and 
    /// out of the NIGHT ConditionState.
    /// </summary>
    public sealed class W3dTruckDraw : W3dModelDraw
    {
        internal static W3dTruckDraw Parse(IniParser parser)
        {
            return parser.ParseBlock(SupplyFieldParseTable);
        }

        private static readonly IniParseTable<W3dTruckDraw> SupplyFieldParseTable = new IniParseTable<W3dTruckDraw>
        {
            
        }.Concat<W3dTruckDraw, W3dModelDraw>(ModelFieldParseTable);
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
            var result = parser.ParseBlock(FieldParseTable);

            result.ConditionFlags = new BitArray<ModelConditionFlag>(); // "NONE"

            return result;
        }

        internal static ObjectConditionState Parse(IniParser parser)
        {
            var conditionFlags = parser.ParseEnumBitArray<ModelConditionFlag>();

            var result = parser.ParseBlock(FieldParseTable);

            result.ConditionFlags = conditionFlags;

            return result;
        }

        private static readonly IniParseTable<ObjectConditionState> FieldParseTable = new IniParseTable<ObjectConditionState>
        {
            { "Model", (parser, x) => x.Model = parser.ParseFileName() },
            { "Turret", (parser, x) => x.Turret = parser.ParseAssetReference() },
            { "Animation", (parser, x) => x.Animation = parser.ParseAnimationName() },
            { "AnimationMode", (parser, x) => x.AnimationMode = parser.ParseEnum<AnimationMode>() },
            { "AnimationSpeedFactorRange", (parser, x) => x.AnimationSpeedFactorRange = FloatRange.Parse(parser) },
            { "Flags", (parser, x) => x.Flags = parser.ParseEnumFlags<ObjectConditionStateFlags>() },
            { "WeaponFireFXBone", (parser, x) => x.WeaponFireFXBones.Add(BoneAttachPoint.Parse(parser)) },
            { "WeaponRecoilBone", (parser, x) => x.WeaponRecoilBones.Add(BoneAttachPoint.Parse(parser)) },
            { "WeaponMuzzleFlash", (parser, x) => x.WeaponMuzzleFlashes.Add(BoneAttachPoint.Parse(parser)) },
            { "WeaponLaunchBone", (parser, x) => x.WeaponLaunchBones.Add(BoneAttachPoint.Parse(parser)) },
            { "ParticleSysBone", (parser, x) => x.ParticleSysBones.Add(ParticleSysBone.Parse(parser)) },
            { "HideSubObject", (parser, x) => x.HideSubObject = parser.ParseAssetReference() },
            { "ShowSubObject", (parser, x) => x.ShowSubObject = parser.ParseAssetReference() },
        };

        public BitArray<ModelConditionFlag> ConditionFlags { get; private set; }

        public string Model { get; private set; }
        public string Turret { get; private set; }
        public string Animation { get; private set; }
        public AnimationMode AnimationMode { get; private set; }
        public FloatRange AnimationSpeedFactorRange { get; private set; }
        public ObjectConditionStateFlags Flags { get; private set; }
        public List<BoneAttachPoint> WeaponFireFXBones { get; private set; } = new List<BoneAttachPoint>();
        public List<BoneAttachPoint> WeaponRecoilBones { get; private set; } = new List<BoneAttachPoint>();
        public List<BoneAttachPoint> WeaponMuzzleFlashes { get; private set; } = new List<BoneAttachPoint>();
        public List<BoneAttachPoint> WeaponLaunchBones { get; private set; } = new List<BoneAttachPoint>();
        public List<ParticleSysBone> ParticleSysBones { get; private set; } = new List<ParticleSysBone>();
        public string HideSubObject { get; private set; }
        public string ShowSubObject { get; private set; }

        public ObjectConditionState Clone(BitArray<ModelConditionFlag> conditionFlags)
        {
            return new ObjectConditionState
            {
                ConditionFlags = conditionFlags,

                Model = Model,
                Turret = Turret,
                Animation = Animation,
                AnimationMode = AnimationMode,
                Flags = Flags,
                ParticleSysBones = ParticleSysBones,
                HideSubObject = HideSubObject,
                ShowSubObject = ShowSubObject
            };
        }
    }

    public enum ModelConditionFlag
    {
        [IniEnum("DAMAGED")]
        Damaged,

        [IniEnum("REALLYDAMAGED")]
        ReallyDamaged,

        [IniEnum("RUBBLE")]
        Rubble,

        [IniEnum("SNOW")]
        Snow,

        [IniEnum("NIGHT")]
        Night,

        [IniEnum("GARRISONED")]
        Garrisoned,

        [IniEnum("POST_COLLAPSE")]
        PostCollapse,

        [IniEnum("CAPTURED")]
        Captured,

        [IniEnum("DOOR_1_OPENING")]
        Door1Opening
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
        RandomStart = 1 << 0,

        [IniEnum("START_FRAME_FIRST")]
        StartFrameFirst = 1 << 1,

        [IniEnum("MAINTAIN_FRAME_ACROSS_STATES")]
        MaintainFrameAcrossStates = 1 << 2
    }

    public sealed class BoneAttachPoint
    {
        internal static BoneAttachPoint Parse(IniParser parser)
        {
            return new BoneAttachPoint
            {
                WeaponSlot = parser.ParseEnum<WeaponSlot>(),
                BoneName = parser.ParseBoneName()
            };
        }

        public WeaponSlot WeaponSlot { get; private set; }
        public string BoneName { get; private set; }
    }

    public sealed class ParticleSysBone
    {
        internal static ParticleSysBone Parse(IniParser parser)
        {
            return new ParticleSysBone
            {
                BoneName = parser.ParseBoneName(),
                ParticleSystem = parser.ParseAssetReference()
            };
        }

        public string BoneName { get; private set; }
        public string ParticleSystem { get; private set; }
    }

    public abstract class ObjectBody : ObjectModule
    {
        internal static ObjectBody ParseBody(IniParser parser) => ParseModule(parser, BodyParseTable);

        private static readonly Dictionary<string, Func<IniParser, ObjectBody>> BodyParseTable = new Dictionary<string, Func<IniParser, ObjectBody>>
        {
            { "ActiveBody", ActiveBody.Parse },
            { "HighlanderBody", HighlanderBody.Parse },
            { "ImmortalBody", ImmortalBody.Parse },
            { "InactiveBody", InactiveBody.Parse },
            { "StructureBody", StructureBody.Parse },
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

    public sealed class ActiveBody : ObjectBody
    {
        internal static ActiveBody Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<ActiveBody> FieldParseTable = new IniParseTable<ActiveBody>
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
            { "AIUpdateInterface", AIUpdateInterfaceBehavior.Parse },
            { "AutoDepositUpdate", AutoDepositUpdateBehavior.Parse },
            { "AutoHealBehavior", AutoHealBehavior.Parse },
            { "BaikonurLaunchPower", BaikonurLaunchPowerBehavior.Parse },
            { "BoneFXDamage", BoneFXDamageBehavior.Parse },
            { "BoneFXUpdate", BoneFXUpdateBehavior.Parse },
            { "BridgeBehavior", BridgeBehavior.Parse },
            { "BridgeTowerBehavior", BridgeTowerBehavior.Parse },
            { "CostModifierUpgrade", CostModifierUpgradeBehavior.Parse },
            { "CreateObjectDie", CreateObjectDieBehavior.Parse },
            { "DamDie", DamDieBehavior.Parse },
            { "DeletionUpdate", DeletionUpdateBehavior.Parse },
            { "DestroyDie", DestroyDieBehavior.Parse },
            { "FireWeaponWhenDeadBehavior", FireWeaponWhenDeadBehavior.Parse },
            { "FireWeaponWhenDamagedBehavior", FireWeaponWhenDamagedBehavior.Parse },
            { "FlammableUpdate", FlammableUpdateBehavior.Parse },
            { "FXListDie", FXListDieBehavior.Parse },
            { "GarrisonContain", GarrisonContainBehavior.Parse },
            { "GrantUpgradeCreate", GrantUpgradeCreateBehavior.Parse },
            { "KeepObjectDie", KeepObjectDieBehavior.Parse },
            { "LifetimeUpdate", LifetimeUpdateBehavior.Parse },
            { "MoneyCrateCollide", MoneyCrateCollideBehavior.Parse },
            { "PhysicsBehavior", PhysicsBehavior.Parse },
            { "SalvageCrateCollide", SalvageCrateCollideBehavior.Parse },
            { "SlowDeathBehavior", SlowDeathBehavior.Parse },
            { "SquishCollide", SquishCollideBehavior.Parse },
            { "StructureCollapseUpdate", StructureCollapseUpdateBehavior.Parse },
            { "StructureToppleUpdate", StructureToppleUpdateBehavior.Parse },
            { "SupplyWarehouseCreate", SupplyWarehouseCreateBehavior.Parse },
            { "SupplyWarehouseCripplingBehavior", SupplyWarehouseCripplingBehavior.Parse },
            { "SupplyWarehouseDockUpdate", SupplyWarehouseDockUpdateBehavior.Parse },
            { "TechBuildingBehavior", TechBuildingBehavior.Parse },
            { "ToppleUpdate", ToppleUpdateBehavior.Parse },
            { "TransitionDamageFX", TransitionDamageFXBehavior.Parse },
            { "UnitCrateCollide", UnitCrateCollideBehavior.Parse },
            { "VeterancyCrateCollide", VeterancyCrateCollideBehavior.Parse },
        };
    }

    public sealed class MoneyCrateCollideBehavior : ObjectBehavior
    {
        internal static MoneyCrateCollideBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

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

    /// <summary>
    /// This module is required when KindOf contains TECH_BUILDING.
    /// </summary>
    public sealed class TechBuildingBehavior : ObjectBehavior
    {
        internal static TechBuildingBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<TechBuildingBehavior> FieldParseTable = new IniParseTable<TechBuildingBehavior>();
    }

    /// <summary>
    /// Special-case logic allows for ParentObject to be specified as a bone name to allow other 
    /// objects to appear on the bridge.
    /// </summary>
    public sealed class BridgeBehavior : ObjectBehavior
    {
        internal static BridgeBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BridgeBehavior> FieldParseTable = new IniParseTable<BridgeBehavior>
        {
            { "LateralScaffoldSpeed", (parser, x) => x.LateralScaffoldSpeed = parser.ParseFloat() },
            { "VerticalScaffoldSpeed", (parser, x) => x.VerticalScaffoldSpeed = parser.ParseFloat() },

            { "BridgeDieOCL", (parser, x) => x.BridgeDieOCLs.Add(BridgeDieObjectCreationList.Parse(parser)) },
            { "BridgeDieFX", (parser, x) => x.BridgeDieFXs.Add(BridgeDieFX.Parse(parser)) }
        };

        public float LateralScaffoldSpeed { get; private set; }
        public float VerticalScaffoldSpeed { get; private set; }

        public List<BridgeDieObjectCreationList> BridgeDieOCLs { get; } = new List<BridgeDieObjectCreationList>();
        public List<BridgeDieFX> BridgeDieFXs { get; } = new List<BridgeDieFX>();
    }

    public sealed class BridgeDieFX
    {
        internal static BridgeDieFX Parse(IniParser parser)
        {
            return new BridgeDieFX
            {
                FX = parser.ParseAttribute("FX", () => parser.ParseAssetReference()),
                Delay = parser.ParseAttributeInteger("Delay"),
                Bone = parser.ParseAttribute("Bone", () => parser.ParseBoneName())
            };
        }

        public string FX { get; private set; }
        public int Delay { get; private set; }
        public string Bone { get; private set; }
    }

    public sealed class BridgeDieObjectCreationList
    {
        internal static BridgeDieObjectCreationList Parse(IniParser parser)
        {
            return new BridgeDieObjectCreationList
            {
                OCL = parser.ParseAttribute("OCL", () => parser.ParseAssetReference()),
                Delay = parser.ParseAttributeInteger("Delay"),
                Bone = parser.ParseAttribute("Bone", () => parser.ParseBoneName())
            };
        }

        public string OCL { get; private set; }
        public int Delay { get; private set; }
        public string Bone { get; private set; }
    }

    public sealed class FireWeaponWhenDeadBehavior : ObjectBehavior
    {
        internal static FireWeaponWhenDeadBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<FireWeaponWhenDeadBehavior> FieldParseTable = new IniParseTable<FireWeaponWhenDeadBehavior>
        {
            { "DeathWeapon", (parser, x) => x.DeathWeapon = parser.ParseAssetReference() },
            { "StartsActive", (parser, x) => x.StartsActive = parser.ParseBoolean() }
        };

        public string DeathWeapon { get; private set; }
        public bool StartsActive { get; private set; }
    }

    public sealed class LifetimeUpdateBehavior : ObjectBehavior
    {
        internal static LifetimeUpdateBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LifetimeUpdateBehavior> FieldParseTable = new IniParseTable<LifetimeUpdateBehavior>
        {
            { "MinLifetime", (parser, x) => x.MinLifetime = parser.ParseInteger() },
            { "MaxLifetime", (parser, x) => x.MaxLifetime = parser.ParseInteger() }
        };

        public int MinLifetime { get; private set; }
        public int MaxLifetime { get; private set; }
    }

    public sealed class GrantUpgradeCreateBehavior : ObjectBehavior
    {
        internal static GrantUpgradeCreateBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<GrantUpgradeCreateBehavior> FieldParseTable = new IniParseTable<GrantUpgradeCreateBehavior>
        {
            { "UpgradeToGrant", (parser, x) => x.UpgradeToGrant = parser.ParseAssetReference() }
        };

        public string UpgradeToGrant { get; private set; }
    }

    public sealed class CostModifierUpgradeBehavior : ObjectBehavior
    {
        internal static CostModifierUpgradeBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CostModifierUpgradeBehavior> FieldParseTable = new IniParseTable<CostModifierUpgradeBehavior>
        {
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReference() },
            { "EffectKindOf", (parser, x) => x.EffectKindOf = parser.ParseEnum<ObjectKinds>() },
            { "Percentage", (parser, x) => x.Percentage = parser.ParsePercentage() }
        };

        public string TriggeredBy { get; private set; }
        public ObjectKinds EffectKindOf { get; private set; }
        public float Percentage { get; private set; }
    }

    public sealed class AutoDepositUpdateBehavior : ObjectBehavior
    {
        internal static AutoDepositUpdateBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AutoDepositUpdateBehavior> FieldParseTable = new IniParseTable<AutoDepositUpdateBehavior>
        {
            { "DepositTiming", (parser, x) => x.DepositTiming = parser.ParseInteger() },
            { "DepositAmount", (parser, x) => x.DepositAmount = parser.ParseInteger() },
            { "InitialCaptureBonus", (parser, x) => x.InitialCaptureBonus = parser.ParseInteger() }
        };

        /// <summary>
        /// How often, in milliseconds, to give money to the owning player.
        /// </summary>
        public int DepositTiming { get; private set; }

        /// <summary>
        /// Amount of cash to deposit after every <see cref="DepositTiming"/>.
        /// </summary>
        public int DepositAmount { get; private set; }

        /// <summary>
        /// One-time capture bonus.
        /// </summary>
        public int InitialCaptureBonus { get; private set; }
    }

    /// <summary>
    /// Enables use of BoneFXUpdate module on this object where an additional dynamic FX logic can 
    /// be used.
    /// </summary>
    public sealed class BoneFXDamageBehavior : ObjectBehavior
    {
        internal static BoneFXDamageBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BoneFXDamageBehavior> FieldParseTable = new IniParseTable<BoneFXDamageBehavior>();
    }

    public sealed class BoneFXUpdateBehavior : ObjectBehavior
    {
        internal static BoneFXUpdateBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BoneFXUpdateBehavior> FieldParseTable = new IniParseTable<BoneFXUpdateBehavior>
        {
            { "DamageFXTypes", (parser, x) => x.DamageFXTypes = parser.ParseEnumBitArray<DamageType>() },
            { "RubbleFXList1", (parser, x) => x.RubbleFXList1 = BoneFXUpdateFXList.Parse(parser) },

            { "DamageParticleTypes", (parser, x) => x.DamageParticleTypes = parser.ParseEnumBitArray<DamageType>() },
            { "RubbleParticleSystem1", (parser, x) => x.RubbleParticleSystem1 = BoneFXUpdateParticleSystem.Parse(parser) },
        };

        public BitArray<DamageType> DamageFXTypes { get; private set; }
        public BoneFXUpdateFXList RubbleFXList1 { get; private set; }

        public BitArray<DamageType> DamageParticleTypes { get; private set; }
        public BoneFXUpdateParticleSystem RubbleParticleSystem1 { get; private set; }
    }

    public sealed class BoneFXUpdateFXList
    {
        internal static BoneFXUpdateFXList Parse(IniParser parser)
        {
            return new BoneFXUpdateFXList
            {
                Bone = parser.ParseAttribute("Bone", () => parser.ParseBoneName()),
                OnlyOnce = parser.ParseAttributeBoolean("OnlyOnce"),
                Min = parser.ParseInteger(),
                Max = parser.ParseInteger(),
                FXList = parser.ParseAttribute("FXList", () => parser.ParseAssetReference())
            };
        }

        public string Bone { get; private set; }
        public bool OnlyOnce { get; private set; }
        public int Min { get; private set; }
        public int Max { get; private set; }
        public string FXList { get; private set; }
    }

    public sealed class BoneFXUpdateParticleSystem
    {
        internal static BoneFXUpdateParticleSystem Parse(IniParser parser)
        {
            return new BoneFXUpdateParticleSystem
            {
                Bone = parser.ParseAttribute("Bone", () => parser.ParseBoneName()),
                OnlyOnce = parser.ParseAttributeBoolean("OnlyOnce"),
                Min = parser.ParseInteger(),
                Max = parser.ParseInteger(),
                ParticleSystem = parser.ParseAttribute("PSys", () => parser.ParseAssetReference())
            };
        }

        public string Bone { get; private set; }
        public bool OnlyOnce { get; private set; }
        public int Min { get; private set; }
        public int Max { get; private set; }
        public string ParticleSystem { get; private set; }
    }

    /// <summary>
    /// Hardcoded to use the GarrisonGun object definition for the weapons pointing from the object 
    /// when occupants are firing and these are drawn at bones named FIREPOINT. Also, it Allows use 
    /// of the GARRISONED Model ConditionState.
    /// </summary>
    public sealed class GarrisonContainBehavior : ObjectBehavior
    {
        internal static GarrisonContainBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<GarrisonContainBehavior> FieldParseTable = new IniParseTable<GarrisonContainBehavior>
        {
            { "ContainMax", (parser, x) => x.ContainMax = parser.ParseInteger() },
            { "EnterSound", (parser, x) => x.EnterSound = parser.ParseAssetReference() },
            { "ExitSound", (parser, x) => x.ExitSound = parser.ParseAssetReference() }
        };

        public int ContainMax { get; private set; }
        public string EnterSound { get; private set; }
        public string ExitSound { get; private set; }
    }

    public sealed class StructureCollapseUpdateBehavior : ObjectBehavior
    {
        internal static StructureCollapseUpdateBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<StructureCollapseUpdateBehavior> FieldParseTable = new IniParseTable<StructureCollapseUpdateBehavior>
        {
            { "MinCollapseDelay", (parser, x) => x.MinCollapseDelay = parser.ParseInteger() },
            { "MaxCollapseDelay", (parser, x) => x.MaxCollapseDelay = parser.ParseInteger() },
            { "CollapseDamping", (parser, x) => x.CollapseDamping = parser.ParseFloat() },
            { "MaxShudder", (parser, x) => x.MaxShudder = parser.ParseFloat() },
            { "MinBurstDelay", (parser, x) => x.MinBurstDelay = parser.ParseInteger() },
            { "MaxBurstDelay", (parser, x) => x.MaxBurstDelay = parser.ParseInteger() },
            { "BigBurstFrequency", (parser, x) => x.BigBurstFrequency = parser.ParseInteger() },

            { "OCL", (parser, x) => x.OCLs[parser.ParseEnum<StructureCollapseStage>()] = parser.ParseAssetReference() },
            { "FXList", (parser, x) => x.FXLists[parser.ParseEnum<StructureCollapseStage>()] = parser.ParseAssetReference() },
        };

        public int MinCollapseDelay { get; private set; }
        public int MaxCollapseDelay { get; private set; }
        public float CollapseDamping { get; private set; }
        public float MaxShudder { get; private set; }
        public int MinBurstDelay { get; private set; }
        public int MaxBurstDelay { get; private set; }
        public int BigBurstFrequency { get; private set; }

        public Dictionary<StructureCollapseStage, string> OCLs { get; } = new Dictionary<StructureCollapseStage, string>();
        public Dictionary<StructureCollapseStage, string> FXLists { get; } = new Dictionary<StructureCollapseStage, string>();
    }

    public sealed class SlowDeathBehavior : ObjectBehavior
    {
        internal static SlowDeathBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SlowDeathBehavior> FieldParseTable = new IniParseTable<SlowDeathBehavior>
        {
            { "DeathTypes", (parser, x) => x.DeathTypes = parser.ParseEnumBitArray<DeathType>() },
            { "SinkRate", (parser, x) => x.SinkRate = parser.ParseInteger() },
            { "SinkDelay", (parser, x) => x.SinkDelay = parser.ParseInteger() },
            { "DestructionDelay", (parser, x) => x.DestructionDelay = parser.ParseInteger() },
            { "DestructionDelayVariance", (parser, x) => x.DestructionDelayVariance = parser.ParseInteger() },

            { "OCL", (parser, x) => x.OCLs[parser.ParseEnum<SlowDeathStage>()] = parser.ParseAssetReference() },
            { "FX", (parser, x) => x.FXs[parser.ParseEnum<SlowDeathStage>()] = parser.ParseAssetReference() },
            { "Weapon", (parser, x) => x.Weapons[parser.ParseEnum<SlowDeathStage>()] = parser.ParseAssetReference() },
        };

        public BitArray<DeathType> DeathTypes { get; private set; }
        public int SinkRate { get; private set; }
        public int SinkDelay { get; private set; }
        public int DestructionDelay { get; private set; }
        public int DestructionDelayVariance { get; private set; }

        public Dictionary<SlowDeathStage, string> OCLs { get; } = new Dictionary<SlowDeathStage, string>();
        public Dictionary<SlowDeathStage, string> FXs { get; } = new Dictionary<SlowDeathStage, string>();
        public Dictionary<SlowDeathStage, string> Weapons { get; } = new Dictionary<SlowDeathStage, string>();
    }

    public enum SlowDeathStage
    {
        [IniEnum("INITIAL")]
        Initial,

        [IniEnum("FINAL")]
        Final
    }

    public sealed class AIUpdateInterfaceBehavior : ObjectBehavior
    {
        internal static AIUpdateInterfaceBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AIUpdateInterfaceBehavior> FieldParseTable = new IniParseTable<AIUpdateInterfaceBehavior>
        {
            { "Turret", (parser, x) => x.Turret = TurretAIData.Parse(parser) }
        };

        /// <summary>
        /// Allows the use of TurretMoveStart and TurretMoveLoop within the UnitSpecificSounds 
        /// section of the object.
        /// </summary>
        public TurretAIData Turret { get; private set; }
    }

    public sealed class TurretAIData
    {
        internal static TurretAIData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<TurretAIData> FieldParseTable = new IniParseTable<TurretAIData>
        {
            { "TurretTurnRate", (parser, x) => x.TurretTurnRate = parser.ParseInteger() },
            { "ControlledWeaponSlots", (parser, x) => x.ControlledWeaponSlots = parser.ParseEnum<WeaponSlot>() }
        };

        public int TurretTurnRate { get; private set; }
        public WeaponSlot ControlledWeaponSlots { get; private set; }
    }

    public sealed class ToppleUpdateBehavior : ObjectBehavior
    {
        internal static ToppleUpdateBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ToppleUpdateBehavior> FieldParseTable = new IniParseTable<ToppleUpdateBehavior>
        {
            { "ToppleFX", (parser, x) => x.ToppleFX = parser.ParseAssetReference() },
            { "BounceFX", (parser, x) => x.BounceFX = parser.ParseAssetReference() },
            { "KillWhenStartToppling", (parser, x) => x.KillWhenStartToppling = parser.ParseBoolean() },
            { "ToppleLeftOrRightOnly", (parser, x) => x.ToppleLeftOrRightOnly = parser.ParseBoolean() },
            { "ReorientToppledRubble", (parser, x) => x.ReorientToppledRubble = parser.ParseBoolean() },
            { "BounceVelocityPercent", (parser, x) => x.BounceVelocityPercent = parser.ParsePercentage() },
            { "InitialAccelPercent", (parser, x) => x.InitialAccelPercent = parser.ParsePercentage() },
        };

        public string ToppleFX { get; private set; }
        public string BounceFX { get; private set; }
        public bool KillWhenStartToppling { get; private set; }
        public bool ToppleLeftOrRightOnly { get; private set; }
        public bool ReorientToppledRubble { get; private set; }
        public float BounceVelocityPercent { get; private set; } = 30;
        public float InitialAccelPercent { get; private set; } = 1;
    }

    public sealed class StructureToppleUpdateBehavior : ObjectBehavior
    {
        internal static StructureToppleUpdateBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<StructureToppleUpdateBehavior> FieldParseTable = new IniParseTable<StructureToppleUpdateBehavior>
        {
            { "MinToppleDelay", (parser, x) => x.MinToppleDelay = parser.ParseInteger() },
            { "MaxToppleDelay", (parser, x) => x.MaxToppleDelay = parser.ParseInteger() },
            { "MinToppleBurstDelay", (parser, x) => x.MinToppleBurstDelay = parser.ParseInteger() },
            { "MaxToppleBurstDelay", (parser, x) => x.MaxToppleBurstDelay = parser.ParseInteger() },
            { "StructuralIntegrity", (parser, x) => x.StructuralIntegrity = parser.ParseFloat() },
            { "StructuralDecay", (parser, x) => x.StructuralDecay = parser.ParseFloat() },
            { "DamageFXTypes", (parser, x) => x.DamageFXTypes = parser.ParseEnumBitArray<DamageType>() },
            { "ToppleStartFX", (parser, x) => x.ToppleStartFX = parser.ParseAssetReference() },
            { "ToppleDelayFX", (parser, x) => x.ToppleDelayFX = parser.ParseAssetReference() },
            { "CrushingFX", (parser, x) => x.CrushingFX = parser.ParseAssetReference() },
            { "AngleFX", (parser, x) => x.AngleFX = StructureToppleAngleFX.Parse(parser) },
            { "ToppleDoneFX", (parser, x) => x.ToppleDoneFX = parser.ParseAssetReference() },
            { "CrushingWeaponName", (parser, x) => x.CrushingWeaponName = parser.ParseAssetReference() },
        };

        public int MinToppleDelay { get; private set; }
        public int MaxToppleDelay { get; private set; }
        public int MinToppleBurstDelay { get; private set; }
        public int MaxToppleBurstDelay { get; private set; }
        public float StructuralIntegrity { get; private set; }
        public float StructuralDecay { get; private set; }
        public BitArray<DamageType> DamageFXTypes { get; private set; }
        public string ToppleStartFX { get; private set; }
        public string ToppleDelayFX { get; private set; }
        public string CrushingFX { get; private set; }
        public StructureToppleAngleFX AngleFX { get; private set; }
        public string ToppleDoneFX { get; private set; }
        public string CrushingWeaponName { get; private set; }
    }

    public struct StructureToppleAngleFX
    {
        internal static StructureToppleAngleFX Parse(IniParser parser)
        {
            return new StructureToppleAngleFX
            {
                Angle = parser.ParseFloat(),
                FX = parser.ParseAssetReference()
            };
        }

        public float Angle;
        public string FX;
    }

    public enum StructureCollapseStage
    {
        [IniEnum("INITIAL")]
        Initial,

        [IniEnum("DELAY")]
        Delay,

        [IniEnum("BURST")]
        Burst,

        [IniEnum("FINAL")]
        Final
    }

    /// <summary>
    /// Forces object to dynamically restore itself.
    /// Triggered when object is REALLYDAMAGED, or at 30% of MaxHealth.
    /// </summary>
    public sealed class SupplyWarehouseCripplingBehavior : ObjectBehavior
    {
        internal static SupplyWarehouseCripplingBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<SupplyWarehouseCripplingBehavior> FieldParseTable = new IniParseTable<SupplyWarehouseCripplingBehavior>
        {
            { "SelfHealSupression", (parser, x) => x.SelfHealSuppression = parser.ParseInteger() },
            { "SelfHealDelay", (parser, x) => x.SelfHealDelay = parser.ParseInteger() },
            { "SelfHealAmount", (parser, x) => x.SelfHealAmount = parser.ParseInteger() }
        };

        /// <summary>
        /// Time since last damage until healing starts.
        /// </summary>
        public int SelfHealSuppression { get; private set; }

        /// <summary>
        /// How frequently to heal.
        /// </summary>
        public int SelfHealDelay { get; private set; }

        public int SelfHealAmount { get; private set; }
    }

    public sealed class SupplyWarehouseDockUpdateBehavior : ObjectBehavior
    {
        internal static SupplyWarehouseDockUpdateBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<SupplyWarehouseDockUpdateBehavior> FieldParseTable = new IniParseTable<SupplyWarehouseDockUpdateBehavior>
        {
            { "NumberApproachPositions", (parser, x) => x.NumberApproachPositions = parser.ParseInteger() },
            { "StartingBoxes", (parser, x) => x.StartingBoxes = parser.ParseInteger() },
            { "AllowsPassthrough", (parser, x) => x.AllowsPassthrough = parser.ParseBoolean() },
            { "DeleteWhenEmpty", (parser, x) => x.DeleteWhenEmpty = parser.ParseBoolean() }
        };

        /// <summary>
        /// Number of approach bones in the model. If this is -1, infinite harvesters can approach.
        /// </summary>
        public int NumberApproachPositions { get; private set; }

        /// <summary>
        /// Used to determine the visual representation of a full warehouse.
        /// </summary>
        public int StartingBoxes { get; private set; }

        /// <summary>
        /// Can harvesters drive through this warehouse? Should be set to false if all dock points are external.
        /// </summary>
        public bool AllowsPassthrough { get; private set; } = true;

        /// <summary>
        /// True if warehouse should be deleted when depleted.
        /// </summary>
        public bool DeleteWhenEmpty { get; private set; }
    }

    /// <summary>
    /// Ensures the object acts as a source for supply collection.
    /// </summary>
    public sealed class SupplyWarehouseCreateBehavior : ObjectBehavior
    {
        internal static SupplyWarehouseCreateBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<SupplyWarehouseCreateBehavior> FieldParseTable = new IniParseTable<SupplyWarehouseCreateBehavior>();
    }

    public sealed class BaikonurLaunchPowerBehavior : ObjectBehavior
    {
        internal static BaikonurLaunchPowerBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<BaikonurLaunchPowerBehavior> FieldParseTable = new IniParseTable<BaikonurLaunchPowerBehavior>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },
            { "DetonationObject", (parser, x) => x.DetonationObject = parser.ParseAssetReference() }
        };

        public string SpecialPowerTemplate { get; private set; }
        public string DetonationObject { get; private set; }
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

    public sealed class CreateObjectDieBehavior : ObjectBehavior
    {
        internal static CreateObjectDieBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CreateObjectDieBehavior> FieldParseTable = new IniParseTable<CreateObjectDieBehavior>
        {
            { "DeathTypes", (parser, x) => x.DeathTypes = parser.ParseEnumBitArray<DeathType>() },
            { "CreationList", (parser, x) => x.CreationList = parser.ParseAssetReference() }
        };

        public BitArray<DeathType> DeathTypes { get; private set; }
        public string CreationList { get; private set; }
    }

    /// <summary>
    /// Allows object to continue to exist as an obstacle but allowing water terrain to move 
    /// through. The module must be applied after any other death modules.
    /// </summary>
    public sealed class DamDieBehavior : ObjectBehavior
    {
        internal static DamDieBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<DamDieBehavior> FieldParseTable = new IniParseTable<DamDieBehavior>();
    }

    public sealed class TransitionDamageFXBehavior : ObjectBehavior
    {
        internal static TransitionDamageFXBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<TransitionDamageFXBehavior> FieldParseTable = new IniParseTable<TransitionDamageFXBehavior>
        {
            { "DamageFXTypes", (parser, x) => x.DamageFXTypes = parser.ParseEnumBitArray<DamageType>() },

            { "DamagedFXList1", (parser, x) => x.DamagedFXList1 = TransitionDamageFXList.Parse(parser) },

            { "ReallyDamagedFXList1", (parser, x) => x.ReallyDamagedFXList1 = TransitionDamageFXList.Parse(parser) },

            { "RubbleFXList1", (parser, x) => x.RubbleFXList1 = TransitionDamageFXList.Parse(parser) },

            { "DamageParticleTypes", (parser, x) => x.DamageParticleTypes = parser.ParseEnumBitArray<DamageType>() },

            { "DamagedParticleSystem1", (parser, x) => x.DamagedParticleSystem1 = TransitionDamageParticleSystem.Parse(parser) },
            { "DamagedParticleSystem2", (parser, x) => x.DamagedParticleSystem2 = TransitionDamageParticleSystem.Parse(parser) },
            { "DamagedParticleSystem3", (parser, x) => x.DamagedParticleSystem3 = TransitionDamageParticleSystem.Parse(parser) },
            { "DamagedParticleSystem4", (parser, x) => x.DamagedParticleSystem4 = TransitionDamageParticleSystem.Parse(parser) },
            { "DamagedParticleSystem5", (parser, x) => x.DamagedParticleSystem5 = TransitionDamageParticleSystem.Parse(parser) },
            { "DamagedParticleSystem6", (parser, x) => x.DamagedParticleSystem6 = TransitionDamageParticleSystem.Parse(parser) },

            { "ReallyDamagedParticleSystem1", (parser, x) => x.ReallyDamagedParticleSystem1 = TransitionDamageParticleSystem.Parse(parser) },
            { "ReallyDamagedParticleSystem2", (parser, x) => x.ReallyDamagedParticleSystem2 = TransitionDamageParticleSystem.Parse(parser) },
            { "ReallyDamagedParticleSystem3", (parser, x) => x.ReallyDamagedParticleSystem3 = TransitionDamageParticleSystem.Parse(parser) },
            { "ReallyDamagedParticleSystem4", (parser, x) => x.ReallyDamagedParticleSystem4 = TransitionDamageParticleSystem.Parse(parser) },
            { "ReallyDamagedParticleSystem5", (parser, x) => x.ReallyDamagedParticleSystem5 = TransitionDamageParticleSystem.Parse(parser) },
            { "ReallyDamagedParticleSystem6", (parser, x) => x.ReallyDamagedParticleSystem6 = TransitionDamageParticleSystem.Parse(parser) },
            { "ReallyDamagedParticleSystem7", (parser, x) => x.ReallyDamagedParticleSystem7 = TransitionDamageParticleSystem.Parse(parser) },
            { "ReallyDamagedParticleSystem8", (parser, x) => x.ReallyDamagedParticleSystem8 = TransitionDamageParticleSystem.Parse(parser) },

            { "RubbleParticleSystem1", (parser, x) => x.RubbleParticleSystem1 = TransitionDamageParticleSystem.Parse(parser) },
            { "RubbleParticleSystem2", (parser, x) => x.RubbleParticleSystem2 = TransitionDamageParticleSystem.Parse(parser) },
            { "RubbleParticleSystem3", (parser, x) => x.RubbleParticleSystem3 = TransitionDamageParticleSystem.Parse(parser) },
            { "RubbleParticleSystem4", (parser, x) => x.RubbleParticleSystem4 = TransitionDamageParticleSystem.Parse(parser) },
            { "RubbleParticleSystem5", (parser, x) => x.RubbleParticleSystem5 = TransitionDamageParticleSystem.Parse(parser) },
            { "RubbleParticleSystem6", (parser, x) => x.RubbleParticleSystem6 = TransitionDamageParticleSystem.Parse(parser) },
            { "RubbleParticleSystem7", (parser, x) => x.RubbleParticleSystem7 = TransitionDamageParticleSystem.Parse(parser) },
        };

        public BitArray<DamageType> DamageFXTypes { get; private set; }

        public TransitionDamageFXList DamagedFXList1 { get; private set; }

        public TransitionDamageFXList ReallyDamagedFXList1 { get; private set; }

        public TransitionDamageFXList RubbleFXList1 { get; private set; }

        public BitArray<DamageType> DamageParticleTypes { get; private set; }

        public TransitionDamageParticleSystem DamagedParticleSystem1 { get; private set; }
        public TransitionDamageParticleSystem DamagedParticleSystem2 { get; private set; }
        public TransitionDamageParticleSystem DamagedParticleSystem3 { get; private set; }
        public TransitionDamageParticleSystem DamagedParticleSystem4 { get; private set; }
        public TransitionDamageParticleSystem DamagedParticleSystem5 { get; private set; }
        public TransitionDamageParticleSystem DamagedParticleSystem6 { get; private set; }

        public TransitionDamageParticleSystem ReallyDamagedParticleSystem1 { get; private set; }
        public TransitionDamageParticleSystem ReallyDamagedParticleSystem2 { get; private set; }
        public TransitionDamageParticleSystem ReallyDamagedParticleSystem3 { get; private set; }
        public TransitionDamageParticleSystem ReallyDamagedParticleSystem4 { get; private set; }
        public TransitionDamageParticleSystem ReallyDamagedParticleSystem5 { get; private set; }
        public TransitionDamageParticleSystem ReallyDamagedParticleSystem6 { get; private set; }
        public TransitionDamageParticleSystem ReallyDamagedParticleSystem7 { get; private set; }
        public TransitionDamageParticleSystem ReallyDamagedParticleSystem8 { get; private set; }

        public TransitionDamageParticleSystem RubbleParticleSystem1 { get; private set; }
        public TransitionDamageParticleSystem RubbleParticleSystem2 { get; private set; }
        public TransitionDamageParticleSystem RubbleParticleSystem3 { get; private set; }
        public TransitionDamageParticleSystem RubbleParticleSystem4 { get; private set; }
        public TransitionDamageParticleSystem RubbleParticleSystem5 { get; private set; }
        public TransitionDamageParticleSystem RubbleParticleSystem6 { get; private set; }
        public TransitionDamageParticleSystem RubbleParticleSystem7 { get; private set; }
    }

    public sealed class TransitionDamageFXList
    {
        internal static TransitionDamageFXList Parse(IniParser parser)
        {
            return new TransitionDamageFXList
            {
                Location = parser.ParseAttribute("Loc", () => Coord3D.Parse(parser)),
                FXList = parser.ParseAttribute("FXList", () => parser.ParseAssetReference())
            };
        }

        public Coord3D Location { get; private set; }
        public string FXList { get; private set; }
    }

    public sealed class TransitionDamageParticleSystem
    {
        internal static TransitionDamageParticleSystem Parse(IniParser parser)
        {
            return new TransitionDamageParticleSystem
            {
                Bone = parser.ParseAttribute("Bone", () => parser.ParseBoneName()),
                RandomBone = parser.ParseAttributeBoolean("RandomBone"),
                ParticleSystem = parser.ParseAttribute("PSys", () => parser.ParseAssetReference())
            };
        }

        public string Bone { get; private set; }
        public bool RandomBone { get; private set; }
        public string ParticleSystem { get; private set; }
    }

    public sealed class FXListDieBehavior : ObjectBehavior
    {
        internal static FXListDieBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<FXListDieBehavior> FieldParseTable = new IniParseTable<FXListDieBehavior>
        {
            { "DeathTypes", (parser, x) => x.DeathTypes = parser.ParseEnumBitArray<DeathType>() },
            { "DeathFX", (parser, x) => x.DeathFX = parser.ParseAssetReference() },
            { "OrientToObject", (parser, x) => x.OrientToObject = parser.ParseBoolean() }
        };

        public BitArray<DeathType> DeathTypes { get; private set; }
        public string DeathFX { get; private set; }
        public bool OrientToObject { get; private set; }
    }

    public sealed class FireWeaponWhenDamagedBehavior : ObjectBehavior
    {
        internal static FireWeaponWhenDamagedBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<FireWeaponWhenDamagedBehavior> FieldParseTable = new IniParseTable<FireWeaponWhenDamagedBehavior>
        {
            { "StartsActive", (parser, x) => x.StartsActive = parser.ParseBoolean() },
            { "ContinuousWeaponDamaged", (parser, x) => x.ContinuousWeaponDamaged = parser.ParseAssetReference() },
            { "ContinuousWeaponReallyDamaged", (parser, x) => x.ContinuousWeaponReallyDamaged = parser.ParseAssetReference() },
            { "DamageTypes", (parser, x) => x.DamageTypes = parser.ParseEnumBitArray<DamageType>() },
            { "DamageAmount", (parser, x) => x.DamageAmount = parser.ParseInteger() }
        };

        public bool StartsActive { get; private set; }
        public string ContinuousWeaponDamaged { get; private set; }
        public string ContinuousWeaponReallyDamaged { get; private set; }
        public BitArray<DamageType> DamageTypes { get; private set; }

        /// <summary>
        /// If damage >= this value, the weapon will be fired.
        /// </summary>
        public int DamageAmount { get; private set; }
    }

    /// <summary>
    /// Allows the use of the AFLAME, SMOLDERING, and BURNED condition states.
    /// </summary>
    public sealed class FlammableUpdateBehavior : ObjectBehavior
    {
        internal static FlammableUpdateBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<FlammableUpdateBehavior> FieldParseTable = new IniParseTable<FlammableUpdateBehavior>
        {
            { "FlameDamageLimit", (parser, x) => x.FlameDamageLimit = parser.ParseInteger() },
            { "FlameDamageExpiration", (parser, x) => x.FlameDamageExpiration = parser.ParseInteger() },
            { "AflameDuration", (parser, x) => x.AflameDuration = parser.ParseInteger() },
            { "AflameDamageAmount", (parser, x) => x.AflameDamageAmount = parser.ParseInteger() },
            { "AflameDamageDelay", (parser, x) => x.AflameDamageDelay = parser.ParseInteger() },
        };

        /// <summary>
        /// How much flame damage to receive before catching fire.
        /// </summary>
        public int FlameDamageLimit { get; private set; }

        /// <summary>
        /// Time within which <see cref="FlameDamageLimit"/> must be received in order to catch fire.
        /// </summary>
        public int FlameDamageExpiration { get; private set; }

        /// <summary>
        /// How long to burn for after catching fire.
        /// </summary>
        public int AflameDuration { get; private set; }

        /// <summary>
        /// Amount of damage inflicted.
        /// </summary>
        public int AflameDamageAmount { get; private set; }

        /// <summary>
        /// Delay between each time that <see cref="AflameDamageAmount"/> is inflicted.
        /// </summary>
        public int AflameDamageDelay { get; private set; }
    }

    /// <summary>
    /// Transfers damage done to itself to its parent bridge too.
    /// </summary>
    public sealed class BridgeTowerBehavior : ObjectBehavior
    {
        internal static BridgeTowerBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<BridgeTowerBehavior> FieldParseTable = new IniParseTable<BridgeTowerBehavior>();
    }

    public sealed class KeepObjectDieBehavior : ObjectBehavior
    {
        internal static KeepObjectDieBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<KeepObjectDieBehavior> FieldParseTable = new IniParseTable<KeepObjectDieBehavior>();
    }

    public sealed class AutoHealBehavior : ObjectBehavior
    {
        internal static AutoHealBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AutoHealBehavior> FieldParseTable = new IniParseTable<AutoHealBehavior>
        {
            { "HealingAmount", (parser, x) => x.HealingAmount = parser.ParseInteger() },
            { "HealingDelay", (parser, x) => x.HealingDelay = parser.ParseInteger() },
            { "AffectsWholePlayer", (parser, x) => x.AffectsWholePlayer = parser.ParseBoolean() },
            { "StartsActive", (parser, x) => x.StartsActive = parser.ParseBoolean() },
            { "KindOf", (parser, x) => x.KindOf = parser.ParseEnum<ObjectKinds>() },
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReferenceArray() },
            { "StartHealingDelay", (parser, x) => x.StartHealingDelay = parser.ParseInteger() },
        };

        public int HealingAmount { get; private set; }
        public int HealingDelay { get; private set; }
        public bool AffectsWholePlayer { get; private set; }
        public bool StartsActive { get; private set; }
        public ObjectKinds KindOf { get; private set; }
        public string[] TriggeredBy { get; private set; }
        public int StartHealingDelay { get; private set; }
    }

    /// <summary>
    /// Used by objects with STRUCTURE and IMMOBILE KindOfs defined.
    /// </summary>
    public sealed class StructureBody : ObjectBody
    {
        internal static StructureBody Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<StructureBody> FieldParseTable = new IniParseTable<StructureBody>
        {
            { "MaxHealth", (parser, x) => x.MaxHealth = parser.ParseFloat() },
            { "InitialHealth", (parser, x) => x.InitialHealth = parser.ParseFloat() }
        };

        public float MaxHealth { get; private set; }
        public float InitialHealth { get; private set; }
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
