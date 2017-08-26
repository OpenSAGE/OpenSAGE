using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
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
            { "UpgradeCameo1", (parser, x) => x.UpgradeCameo1 = parser.ParseAssetReference() },
            { "UpgradeCameo2", (parser, x) => x.UpgradeCameo2 = parser.ParseAssetReference() },
            { "UpgradeCameo3", (parser, x) => x.UpgradeCameo3 = parser.ParseAssetReference() },
            { "UpgradeCameo4", (parser, x) => x.UpgradeCameo4 = parser.ParseAssetReference() },
            { "UpgradeCameo5", (parser, x) => x.UpgradeCameo5 = parser.ParseAssetReference() },

            { "Buildable", (parser, x) => x.Buildable = parser.ParseEnum<ObjectBuildableType>() },
            { "Side", (parser, x) => x.Side = parser.ParseAssetReference() },
            { "DisplayName", (parser, x) => x.DisplayName = parser.ParseLocalizedStringKey() },
            { "EditorSorting", (parser, x) => x.EditorSorting = parser.ParseEnumFlags<ObjectEditorSortingFlags>() },
            { "TransportSlotCount", (parser, x) => x.TransportSlotCount = parser.ParseInteger() },
            { "VisionRange", (parser, x) => x.VisionRange = parser.ParseFloat() },
            { "ShroudRevealToAllRange", (parser, x) => x.ShroudRevealToAllRange = parser.ParseFloat() },
            { "ShroudClearingRange", (parser, x) => x.ShroudClearingRange = parser.ParseFloat() },
            { "CrusherLevel", (parser, x) => x.CrusherLevel = parser.ParseInteger() },
            { "CrushableLevel", (parser, x) => x.CrushableLevel = parser.ParseInteger() },
            { "BuildCost", (parser, x) => x.BuildCost = parser.ParseInteger() },
            { "BuildTime", (parser, x) => x.BuildTime = parser.ParseFloat() },
            { "RefundValue", (parser, x) => x.RefundValue = parser.ParseInteger() },
            { "EnergyProduction", (parser, x) => x.EnergyProduction = parser.ParseInteger() },
            { "EnergyBonus", (parser, x) => x.EnergyBonus = parser.ParseInteger() },
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
            { "MaxSimultaneousOfType", (parser, x) => x.MaxSimultaneousOfType = MaxSimultaneousObjectCount.Parse(parser) },
            { "MaxSimultaneousLinkKey", (parser, x) => x.MaxSimultaneousLinkKey = parser.ParseIdentifier() },

            { "VoiceSelect", (parser, x) => x.VoiceSelect = parser.ParseAssetReference() },
            { "VoiceMove", (parser, x) => x.VoiceMove = parser.ParseAssetReference() },
            { "VoiceGuard", (parser, x) => x.VoiceGuard = parser.ParseAssetReference() },
            { "VoiceAttack", (parser, x) => x.VoiceAttack = parser.ParseAssetReference() },
            { "VoiceAttackAir", (parser, x) => x.VoiceAttackAir = parser.ParseAssetReference() },
            { "VoiceGroupSelect", (parser, x) => x.VoiceGroupSelect = parser.ParseAssetReference() },
            { "VoiceEnter", (parser, x) => x.VoiceEnter = parser.ParseAssetReference() },
            { "VoiceGarrison", (parser, x) => x.VoiceGarrison = parser.ParseAssetReference() },
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
            { "SoundEnter", (parser, x) => x.SoundEnter = parser.ParseAssetReference() },
            { "SoundExit", (parser, x) => x.SoundExit = parser.ParseAssetReference() },
            { "SoundFallingFromPlane", (parser, x) => x.SoundFallingFromPlane = parser.ParseAssetReference() },
            { "UnitSpecificSounds", (parser, x) => x.UnitSpecificSounds = UnitSpecificAssets.Parse(parser) },
            { "UnitSpecificFX", (parser, x) => x.UnitSpecificFX = UnitSpecificAssets.Parse(parser) },

            { "Behavior", (parser, x) => x.Behaviors.Add(ObjectBehavior.ParseBehavior(parser)) },
            { "Draw", (parser, x) => x.Draws.Add(ObjectDrawModule.ParseDrawModule(parser)) },
            { "Body", (parser, x) => x.Body = ObjectBody.ParseBody(parser) },
            { "ClientUpdate", (parser, x) => x.ClientUpdates.Add(ClientUpdate.ParseClientUpdate(parser)) },

            { "Locomotor", (parser, x) => x.Locomotors[parser.ParseEnum<LocomotorSet>()] = parser.ParseAssetReferenceArray() },
            { "KindOf", (parser, x) => x.KindOf = parser.ParseEnumBitArray<ObjectKinds>() },
            { "RadarPriority", (parser, x) => x.RadarPriority = parser.ParseEnum<RadarPriority>() },
            { "DisplayColor", (parser, x) => x.DisplayColor = IniColorRgb.Parse(parser) },
            { "Scale", (parser, x) => x.Scale = parser.ParseFloat() },
            { "Geometry", (parser, x) => x.Geometry = parser.ParseEnum<ObjectGeometry>() },
            { "GeometryMajorRadius", (parser, x) => x.GeometryMajorRadius = parser.ParseFloat() },
            { "GeometryMinorRadius", (parser, x) => x.GeometryMinorRadius = parser.ParseFloat() },
            { "GeometryHeight", (parser, x) => x.GeometryHeight = parser.ParseFloat() },
            { "GeometryIsSmall", (parser, x) => x.GeometryIsSmall = parser.ParseBoolean() },
            { "FactoryExitWidth", (parser, x) => x.FactoryExitWidth = parser.ParseInteger() },
            { "Shadow", (parser, x) => x.Shadow = parser.ParseEnum<ObjectShadowType>() },
            { "ShadowTexture", (parser, x) => x.ShadowTexture = parser.ParseAssetReference() },
            { "ShadowSizeX", (parser, x) => x.ShadowSizeX = parser.ParseInteger() },
            { "ShadowSizeY", (parser, x) => x.ShadowSizeY = parser.ParseInteger() },
            { "InstanceScaleFuzziness", (parser, x) => x.InstanceScaleFuzziness = parser.ParseFloat() },
            { "BuildCompletion", (parser, x) => x.BuildCompletion = parser.ParseAssetReference() },
            { "BuildVariations", (parser, x) => x.BuildVariations = parser.ParseAssetReferenceArray() },

            { "InheritableModule", (parser, x) => x.InheritableModules.Add(InheritableModule.Parse(parser)) },
            { "OverrideableByLikeKind", (parser, x) => x.OverrideableByLikeKinds.Add(OverrideableByLikeKind.Parse(parser)) },

            { "RemoveModule", (parser, x) => x.RemoveModules.Add(parser.ParseIdentifier()) },
            { "AddModule", (parser, x) => x.AddModules.Add(AddModule.Parse(parser)) },
            { "ReplaceModule", (parser, x) => x.ReplaceModules.Add(ReplaceModule.Parse(parser)) },
        };

        public string Name { get; protected set; }

        // Art
        public int PlacementViewAngle { get; private set; }
        public string SelectPortrait { get; private set; }
        public string ButtonImage { get; private set; }
        public string UpgradeCameo1 { get; private set; }
        public string UpgradeCameo2 { get; private set; }
        public string UpgradeCameo3 { get; private set; }
        public string UpgradeCameo4 { get; private set; }
        public string UpgradeCameo5 { get; private set; }

        // Design
        public ObjectBuildableType Buildable { get; private set; }
        public string Side { get; private set; }
        public string DisplayName { get; private set; }
        public ObjectEditorSortingFlags EditorSorting { get; private set; }
        public int TransportSlotCount { get; private set; }
        public float VisionRange { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public float ShroudRevealToAllRange { get; private set; }

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
        public int EnergyBonus { get; private set; }
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

        public MaxSimultaneousObjectCount MaxSimultaneousOfType { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string MaxSimultaneousLinkKey { get; private set; }

        // Audio
        public string VoiceSelect { get; private set; }
        public string VoiceMove { get; private set; }
        public string VoiceGuard { get; private set; }
        public string VoiceAttack { get; private set; }
        public string VoiceAttackAir { get; private set; }
        public string VoiceGroupSelect { get; private set; }
        public string VoiceEnter { get; private set; }
        public string VoiceGarrison { get; private set; }
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
        public string SoundEnter { get; private set; }
        public string SoundExit { get; private set; }
        public string SoundFallingFromPlane { get; private set; }
        public UnitSpecificAssets UnitSpecificSounds { get; private set; }
        public UnitSpecificAssets UnitSpecificFX { get; private set; }

        // Engineering
        public List<ObjectBehavior> Behaviors { get; } = new List<ObjectBehavior>();
        public List<ObjectDrawModule> Draws { get; } = new List<ObjectDrawModule>();
        public ObjectBody Body { get; private set; }
        public List<ClientUpdate> ClientUpdates { get; } = new List<ClientUpdate>();
        public Dictionary<LocomotorSet, string[]> Locomotors { get; } = new Dictionary<LocomotorSet, string[]>();
        public BitArray<ObjectKinds> KindOf { get; private set; }
        public RadarPriority RadarPriority { get; private set; }
        public IniColorRgb DisplayColor { get; private set; }
        public float Scale { get; private set; }
        public ObjectGeometry Geometry { get; private set; }
        public float GeometryMajorRadius { get; private set; }
        public float GeometryMinorRadius { get; private set; }
        public float GeometryHeight { get; private set; }
        public bool GeometryIsSmall { get; private set; }

        /// <summary>
        /// Amount of space to leave for exiting units.
        /// </summary>
        public int FactoryExitWidth { get; private set; }

        public ObjectShadowType Shadow { get; private set; }
        public string ShadowTexture { get; private set; }
        public int ShadowSizeX { get; private set; }
        public int ShadowSizeY { get; private set; }
        public float InstanceScaleFuzziness { get; private set; }
        public string BuildCompletion { get; private set; }
        public string[] BuildVariations { get; private set; }

        public List<InheritableModule> InheritableModules { get; } = new List<InheritableModule>();
        public List<OverrideableByLikeKind> OverrideableByLikeKinds { get; } = new List<OverrideableByLikeKind>();

        // Map.ini module modifications
        public List<string> RemoveModules { get; } = new List<string>();
        public List<AddModule> AddModules { get; } = new List<AddModule>();
        public List<ReplaceModule> ReplaceModules { get; } = new List<ReplaceModule>();
    }

    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public struct MaxSimultaneousObjectCount
    {
        internal static MaxSimultaneousObjectCount Parse(IniParser parser)
        {
            if (parser.CurrentTokenType == IniTokenType.IntegerLiteral)
            {
                return new MaxSimultaneousObjectCount
                {
                    CountType = MaxSimultaneousObjectCountType.Explicit,
                    ExplicitCount = parser.ParseInteger()
                };
            }

            var currentPos = parser.CurrentPosition;
            var countType = parser.ParseIdentifier();
            if (countType != "DeterminedBySuperweaponRestriction")
            {
                throw new IniParseException("Unknown MaxSimultaneousOfType value: " + countType, currentPos);
            }

            return new MaxSimultaneousObjectCount
            {
                CountType = MaxSimultaneousObjectCountType.DeterminedBySuperweaponRestriction
            };
        }

        public MaxSimultaneousObjectCountType CountType;
        public int ExplicitCount;
    }

    public enum MaxSimultaneousObjectCountType
    {
        Explicit,
        DeterminedBySuperweaponRestriction
    }

    public enum ObjectBuildableType
    {
        [IniEnum("No")]
        No,

        [IniEnum("Yes")]
        Yes,

        [IniEnum("Only_By_AI")]
        OnlyByAI
    }

    public sealed class AddModule
    {
        internal static AddModule Parse(IniParser parser)
        {
            var name = parser.NextTokenIf(IniTokenType.Identifier);

            var result = parser.ParseBlock(FieldParseTable);

            result.Name = name?.StringValue;

            return result;
        }

        private static readonly IniParseTable<AddModule> FieldParseTable = new IniParseTable<AddModule>
        {
            { "Behavior", (parser, x) => x.Module = ObjectBehavior.ParseBehavior(parser) },
            { "Draw", (parser, x) => x.Module = ObjectDrawModule.ParseDrawModule(parser) },
            { "Body", (parser, x) => x.Module = ObjectBody.ParseBody(parser) },
        };

        public string Name { get; private set; }

        public ObjectModule Module { get; private set; }
    }

    public sealed class ReplaceModule
    {
        internal static ReplaceModule Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<ReplaceModule> FieldParseTable = new IniParseTable<ReplaceModule>
        {
            { "Behavior", (parser, x) => x.Module = ObjectBehavior.ParseBehavior(parser) },
            { "Draw", (parser, x) => x.Module = ObjectDrawModule.ParseDrawModule(parser) },
            { "Body", (parser, x) => x.Module = ObjectBody.ParseBody(parser) },
        };

        public string Name { get; private set; }

        public ObjectModule Module { get; private set; }
    }
}
