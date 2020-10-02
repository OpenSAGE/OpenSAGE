using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using OpenSage.Audio;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Gui;
using OpenSage.Gui.ControlBar;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [DebuggerDisplay("[ObjectDefinition:{Name}]")]
    public class ObjectDefinition : BaseAsset
    {
        internal static ObjectDefinition Parse(IniParser parser)
        {
            // This will be null if the thing we're parsing *is* the DefaultThingTemplate
            var defaultThingTemplate = parser.GetDefaultThingTemplate();
            var resultObject = defaultThingTemplate?.CloneForImplicitInheritance();

            resultObject = parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("GameObject", name),
                FieldParseTable,
                resultObject: resultObject);

            return resultObject;
        }

        internal static ObjectDefinition ParseReskin(IniParser parser)
        {
            var name = parser.ParseIdentifier();

            var reskinClone = parser.ParseObjectReference().Value.CloneForExplicitInheritance();

            var result = parser.ParseBlock(FieldParseTable, resultObject: reskinClone);

            result.SetNameAndInstanceId("GameObject", name);

            return result;
        }

        internal static ObjectDefinition ParseChildObject(IniParser parser)
        {
            var name = parser.ParseIdentifier();

            var parentClone = parser.ParseObjectReference().Value.CloneForExplicitInheritance();

            var result = parser.ParseBlock(FieldParseTable, resultObject: parentClone);

            result.SetNameAndInstanceId("GameObject", name);

            return result;
        }

        internal static readonly IniParseTable<ObjectDefinition> FieldParseTable = new IniParseTable<ObjectDefinition>
        {
            { "PlacementViewAngle", (parser, x) => x.PlacementViewAngle = parser.ParseInteger() },
            { "SelectPortrait", (parser, x) => x.SelectPortrait = parser.ParseMappedImageReference() },
            { "ButtonImage", (parser, x) => x.ButtonImage = parser.ParseMappedImageReference() },
            { "UpgradeCameo1", (parser, x) => x.UpgradeCameo1 = parser.ParseUpgradeReference() },
            { "UpgradeCameo2", (parser, x) => x.UpgradeCameo2 = parser.ParseUpgradeReference() },
            { "UpgradeCameo3", (parser, x) => x.UpgradeCameo3 = parser.ParseUpgradeReference() },
            { "UpgradeCameo4", (parser, x) => x.UpgradeCameo4 = parser.ParseUpgradeReference() },
            { "UpgradeCameo5", (parser, x) => x.UpgradeCameo5 = parser.ParseUpgradeReference() },

            { "Buildable", (parser, x) => x.Buildable = parser.ParseEnum<ObjectBuildableType>() },
            { "Side", (parser, x) => x.Side = parser.ParseAssetReference() },
            { "DisplayName", (parser, x) => x.DisplayName = parser.ParseLocalizedStringKey() },
            { "EditorSorting", (parser, x) => x.EditorSorting = parser.ParseEnumFlags<ObjectEditorSortingFlags>() },
            { "Browser", (parser, x) => x.Browser = parser.ParseEnumFlags<ObjectBrowserFlags>() },
            { "TransportSlotCount", (parser, x) => x.TransportSlotCount = parser.ParseInteger() },
            { "VisionRange", (parser, x) => x.VisionRange = parser.ParseFloat() },
            { "ShroudRevealToAllRange", (parser, x) => x.ShroudRevealToAllRange = parser.ParseFloat() },
            { "ShroudClearingRange", (parser, x) => x.ShroudClearingRange = parser.ParseFloat() },
            { "CrusherLevel", (parser, x) => x.CrusherLevel = parser.ParseInteger() },
            { "CrushableLevel", (parser, x) => x.CrushableLevel = parser.ParseInteger() },
            { "BuildCost", (parser, x) => x.BuildCost = parser.ParseFloat() },
            { "BuildTime", (parser, x) => x.BuildTime = parser.ParseFloat() },
            { "BuildFadeInOnCreateList", (parser, x) => x.BuildFadeInOnCreateList = parser.ParseIdentifier() },
            { "BuildFadeInOnCreateTime", (parser, x) => x.BuildFadeInOnCreateTime = parser.ParseFloat() },
            { "RefundValue", (parser, x) => x.RefundValue = parser.ParseInteger() },
            { "EnergyProduction", (parser, x) => x.EnergyProduction = parser.ParseInteger() },
            { "EnergyBonus", (parser, x) => x.EnergyBonus = parser.ParseInteger() },
            { "IsForbidden", (parser, x) => x.IsForbidden = parser.ParseBoolean() },
            { "IsBridge", (parser, x) => x.IsBridge = parser.ParseBoolean() },
            { "IsPrerequisite", (parser, x) => x.IsPrerequisite = parser.ParseBoolean() },
            { "WeaponSet", (parser, x) => { var wts = WeaponTemplateSet.Parse(parser); x.WeaponSets[wts.Conditions] = wts; } },
            { "ArmorSet", (parser, x) => { var ams = ArmorTemplateSet.Parse(parser); x.ArmorSets[ams.Conditions] = ams; } },
            { "CommandSet", (parser, x) => x.CommandSet = parser.ParseCommandSetReference() },
            { "Prerequisites", (parser, x) => x.Prerequisites = ObjectPrerequisites.Parse(parser) },
            { "IsTrainable", (parser, x) => x.IsTrainable = parser.ParseBoolean() },
            { "FenceWidth", (parser, x) => x.FenceWidth = parser.ParseFloat() },
            { "FenceXOffset", (parser, x) => x.FenceXOffset = parser.ParseFloat() },
            { "ExperienceValue", (parser, x) => x.ExperienceValue = VeterancyValues.Parse(parser) },
            { "ExperienceRequired", (parser, x) => x.ExperienceRequired = VeterancyValues.Parse(parser) },
            { "MaxSimultaneousOfType", (parser, x) => x.MaxSimultaneousOfType = MaxSimultaneousObjectCount.Parse(parser) },
            { "MaxSimultaneousLinkKey", (parser, x) => x.MaxSimultaneousLinkKey = parser.ParseIdentifier() },

            { "VoiceSelect", (parser, x) => x.VoiceSelect = parser.ParseAudioEventReference() },
            { "VoiceSelectGroup", (parser, x) => x.VoiceSelectGroup = parser.ParseAssetReference() },
            { "VoiceSelectBattle", (parser, x) => x.VoiceSelectBattle = parser.ParseAssetReference() },
            { "VoiceSelectBattleGroup", (parser, x) => x.VoiceSelectBattleGroup = parser.ParseAssetReference() },
            { "VoiceSelectUnderConstruction", (parser, x) => x.VoiceSelectUnderConstruction = parser.ParseAssetReference() },
            { "VoiceMove", (parser, x) => x.VoiceMove = parser.ParseAudioEventReference() },
            { "VoiceMoveGroup", (parser, x) => x.VoiceMoveGroup = parser.ParseAudioEventReference() },
            { "VoiceMoveOverWalls", (parser, x) => x.VoiceMoveOverWall = parser.ParseAudioEventReference() },
            { "VoiceMoveToHigherGround", (parser, x) => x.VoiceMoveToHigherGround = parser.ParseAudioEventReference() },
            { "VoiceGuard", (parser, x) => x.VoiceGuard = parser.ParseAudioEventReference() },
            { "VoiceGuardGroup", (parser, x) => x.VoiceGuardGroup = parser.ParseAudioEventReference() },
            { "VoiceAttack", (parser, x) => x.VoiceAttack = parser.ParseAudioEventReference() },
            { "VoiceAttackGroup", (parser, x) => x.VoiceAttackGroup = parser.ParseAudioEventReference() },
            { "VoiceAttackCharge", (parser, x) => x.VoiceAttackCharge = parser.ParseAudioEventReference() },
            { "VoiceAttackChargeGroup", (parser, x) => x.VoiceAttackChargeGroup = parser.ParseAudioEventReference() },
            { "VoiceAttackAir", (parser, x) => x.VoiceAttackAir = parser.ParseAudioEventReference() },
            { "VoiceAttackAirGroup", (parser, x) => x.VoiceAttackAirGroup = parser.ParseAudioEventReference() },
            { "VoiceGroupSelect", (parser, x) => x.VoiceGroupSelect = parser.ParseAudioEventReference() },
            { "VoiceEnter", (parser, x) => x.VoiceEnter = parser.ParseAudioEventReference() },
            { "VoiceGarrison", (parser, x) => x.VoiceGarrison = parser.ParseAudioEventReference() },
            { "VoiceFear", (parser, x) => x.VoiceFear = parser.ParseAudioEventReference() },
            { "VoiceSelectElite", (parser, x) => x.VoiceSelectElite = parser.ParseAudioEventReference() },
            { "VoiceCreated", (parser, x) => x.VoiceCreated = parser.ParseAudioEventReference() },
            { "VoiceTaskUnable", (parser, x) => x.VoiceTaskUnable = parser.ParseAudioEventReference() },
            { "VoiceTaskComplete", (parser, x) => x.VoiceTaskComplete = parser.ParseAudioEventReference() },
            { "VoiceDefect", (parser, x) => x.VoiceDefect = parser.ParseAudioEventReference() },
            { "VoiceMeetEnemy", (parser, x) => x.VoiceMeetEnemy = parser.ParseAudioEventReference() },
            { "VoiceAlert", (parser, x) => x.VoiceAlert = parser.ParseAudioEventReference() },
            { "VoiceFullyCreated", (parser, x) => x.VoiceFullyCreated = parser.ParseAudioEventReference() },
            { "VoiceRetreatToCastle", (parser, x) => x.VoiceRetreatToCastle = parser.ParseAudioEventReference() },
            { "VoiceRetreatToCastleGroup", (parser, x) => x.VoiceRetreatToCastleGroup = parser.ParseAudioEventReference() },
            { "VoiceMoveToCamp", (parser, x) => x.VoiceMoveToCamp = parser.ParseAudioEventReference() },
            { "VoiceMoveToCampGroup", (parser, x) => x.VoiceMoveToCampGroup = parser.ParseAudioEventReference() },
            { "VoiceAttackStructure", (parser, x) => x.VoiceAttackStructure = parser.ParseAudioEventReference() },
            { "VoiceAttackStructureGroup", (parser, x) => x.VoiceAttackStructureGroup = parser.ParseAudioEventReference() },
            { "VoiceAttackMachine", (parser, x) => x.VoiceAttackMachine = parser.ParseAudioEventReference() },
            { "VoiceAttackMachineGroup", (parser, x) => x.VoiceAttackMachineGroup = parser.ParseAudioEventReference() },
            { "VoiceMoveWhileAttacking", (parser, x) => x.VoiceMoveWhileAttacking = parser.ParseAudioEventReference() },
            { "VoiceMoveWhileAttackingGroup", (parser, x) => x.VoiceMoveWhileAttackingGroup = parser.ParseAudioEventReference() },
            { "VoiceAmbushed", (parser, x) => x.VoiceAmbushed = parser.ParseAudioEventReference() },
            { "VoiceCombineWithHorde", (parser, x) => x.VoiceCombineWithHorde = parser.ParseAudioEventReference() },
            { "VoiceEnterStateAttack", (parser, x) => x.VoiceEnterStateAttack = parser.ParseAudioEventReference() },
            { "VoiceEnterStateAttackCharge", (parser, x) => x.VoiceEnterStateAttackCharge = parser.ParseAudioEventReference() },
            { "VoiceEnterStateAttackAir", (parser, x) => x.VoiceEnterStateAttackAir = parser.ParseAudioEventReference() },
            { "VoiceEnterStateAttackStructure", (parser, x) => x.VoiceEnterStateAttackStructure = parser.ParseAudioEventReference() },
            { "VoiceEnterStateAttackMachine", (parser, x) => x.VoiceEnterStateAttackMachine = parser.ParseAudioEventReference() },
            { "VoiceEnterStateMove", (parser, x) => x.VoiceEnterStateMove = parser.ParseAudioEventReference() },
            { "VoiceEnterStateRetreatToCastle", (parser, x) => x.VoiceEnterStateRetreatToCastle = parser.ParseAudioEventReference() },
            { "VoiceEnterStateMoveToCamp", (parser, x) => x.VoiceEnterStateMoveToCamp = parser.ParseAudioEventReference() },
            { "VoiceEnterStateMoveWhileAttacking", (parser, x) => x.VoiceEnterStateMoveWhileAttacking = parser.ParseAudioEventReference() },
            { "VoiceEnterStateMoveToHigherGround", (parser, x) => x.VoiceEnterStateMoveToHigherGround = parser.ParseAudioEventReference() },
            { "VoiceEnterStateMoveOverWalls", (parser, x) => x.VoiceEnterStateMoveOverWalls = parser.ParseAudioEventReference() },

            { "VoiceSelect2", (parser, x) => x.VoiceSelect2 = parser.ParseAudioEventReference() },
            { "VoiceSelectGroup2", (parser, x) => x.VoiceSelectGroup2 = parser.ParseAudioEventReference() },
            { "VoiceSelectBattle2", (parser, x) => x.VoiceSelectBattle2 = parser.ParseAudioEventReference() },
            { "VoiceSelectBattleGroup2", (parser, x) => x.VoiceSelectBattleGroup2 = parser.ParseAudioEventReference() },
            { "VoiceMove2", (parser, x) => x.VoiceMove2 = parser.ParseAudioEventReference() },
            { "VoiceMoveGroup2", (parser, x) => x.VoiceMoveGroup2 = parser.ParseAudioEventReference() },
            { "VoiceGuard2", (parser, x) => x.VoiceGuard2 = parser.ParseAudioEventReference() },
            { "VoiceGuardGroup2", (parser, x) => x.VoiceGuardGroup2 = parser.ParseAudioEventReference() },
            { "VoiceAttack2", (parser, x) => x.VoiceAttack2 = parser.ParseAudioEventReference() },
            { "VoiceAttackGroup2", (parser, x) => x.VoiceAttackGroup2 = parser.ParseAudioEventReference() },
            { "VoiceAttackCharge2", (parser, x) => x.VoiceAttackCharge2 = parser.ParseAudioEventReference() },
            { "VoiceAttackChargeGroup2", (parser, x) => x.VoiceAttackChargeGroup2 = parser.ParseAudioEventReference() },
            { "VoiceAttackAir2", (parser, x) => x.VoiceAttackAir2 = parser.ParseAudioEventReference() },
            { "VoiceAttackAirGroup2", (parser, x) => x.VoiceAttackAirGroup2 = parser.ParseAudioEventReference() },
            { "VoiceFear2", (parser, x) => x.VoiceFear2 = parser.ParseAudioEventReference() },
            { "VoiceCreated2", (parser, x) => x.VoiceCreated2 = parser.ParseAudioEventReference() },
            { "VoiceTaskComplete2", (parser, x) => x.VoiceTaskComplete2 = parser.ParseAudioEventReference() },
            { "VoiceDefect2", (parser, x) => x.VoiceDefect2 = parser.ParseAudioEventReference() },
            { "VoiceAlert2", (parser, x) => x.VoiceAlert2 = parser.ParseAudioEventReference() },
            { "VoiceFullyCreated2", (parser, x) => x.VoiceFullyCreated2 = parser.ParseAudioEventReference() },
            { "VoiceRetreatToCastle2", (parser, x) => x.VoiceRetreatToCastle2 = parser.ParseAudioEventReference() },
            { "VoiceRetreatToCastleGroup2", (parser, x) => x.VoiceRetreatToCastleGroup2 = parser.ParseAudioEventReference() },
            { "VoiceMoveToCamp2", (parser, x) => x.VoiceMoveToCamp2 = parser.ParseAudioEventReference() },
            { "VoiceMoveToCampGroup2", (parser, x) => x.VoiceMoveToCampGroup2 = parser.ParseAudioEventReference() },
            { "VoiceAttackStructure2", (parser, x) => x.VoiceAttackStructure2 = parser.ParseAudioEventReference() },
            { "VoiceAttackStructureGroup2", (parser, x) => x.VoiceAttackStructureGroup2 = parser.ParseAudioEventReference() },
            { "VoiceAttackMachine2", (parser, x) => x.VoiceAttackMachine2 = parser.ParseAudioEventReference() },
            { "VoiceAttackMachineGroup2", (parser, x) => x.VoiceAttackMachineGroup2 = parser.ParseAudioEventReference() },
            { "VoiceMoveWhileAttacking2", (parser, x) => x.VoiceMoveWhileAttacking2 = parser.ParseAudioEventReference() },
            { "VoiceMoveWhileAttackingGroup2", (parser, x) => x.VoiceMoveWhileAttackingGroup2 = parser.ParseAudioEventReference() },
            { "VoiceAmbushed2", (parser, x) => x.VoiceAmbushed2 = parser.ParseAudioEventReference() },
            { "VoiceCombineWithHorde2", (parser, x) => x.VoiceCombineWithHorde2 = parser.ParseAudioEventReference() },
            { "VoiceEnterStateAttack2", (parser, x) => x.VoiceEnterStateAttack2 = parser.ParseAudioEventReference() },
            { "VoiceEnterStateAttackCharge2", (parser, x) => x.VoiceEnterStateAttackCharge2 = parser.ParseAudioEventReference() },
            { "VoiceEnterStateAttackAir2", (parser, x) => x.VoiceEnterStateAttackAir2 = parser.ParseAudioEventReference() },
            { "VoiceEnterStateAttackStructure2", (parser, x) => x.VoiceEnterStateAttackStructure2 = parser.ParseAudioEventReference() },
            { "VoiceEnterStateAttackMachine2", (parser, x) => x.VoiceEnterStateAttackMachine2 = parser.ParseAudioEventReference() },
            { "VoiceEnterStateMove2", (parser, x) => x.VoiceEnterStateMove2 = parser.ParseAudioEventReference() },
            { "VoiceEnterStateRetreatToCastle2", (parser, x) => x.VoiceEnterStateRetreatToCastle2 = parser.ParseAudioEventReference() },
            { "VoiceEnterStateMoveToCamp2", (parser, x) => x.VoiceEnterStateMoveToCamp2 = parser.ParseAudioEventReference() },
            { "VoiceEnterStateMoveWhileAttacking2", (parser, x) => x.VoiceEnterStateMoveWhileAttacking2 = parser.ParseAudioEventReference() },

            { "SoundMoveStart", (parser, x) => x.SoundMoveStart = parser.ParseAssetReference() },
            { "SoundMoveStartDamaged", (parser, x) => x.SoundMoveStart = parser.ParseAssetReference() },
            { "SoundMoveLoop", (parser, x) => x.SoundMoveLoop = parser.ParseAssetReference() },
            { "SoundMoveLoopDamaged", (parser, x) => x.SoundMoveLoopDamaged = parser.ParseAssetReference() },
            { "SoundOnDamaged", (parser, x) => x.SoundOnDamaged = parser.ParseAudioEventReference() },
            { "SoundOnReallyDamaged", (parser, x) => x.SoundOnReallyDamaged = parser.ParseAudioEventReference() },
            { "SoundDie", (parser, x) => x.SoundDie = parser.ParseAudioEventReference() },
            { "SoundDieFire", (parser, x) => x.SoundDieFire = parser.ParseAssetReference() },
            { "SoundDieToxin", (parser, x) => x.SoundDieToxin = parser.ParseAssetReference() },
            { "SoundStealthOn", (parser, x) => x.SoundStealthOn = parser.ParseAssetReference() },
            { "SoundStealthOff", (parser, x) => x.SoundStealthOff = parser.ParseAssetReference() },
            { "SoundCrush", (parser, x) => x.SoundCrush = parser.ParseAssetReference() },
            { "SoundAmbient", (parser, x) => x.SoundAmbient = parser.ParseAssetReference() },
            { "SoundAmbientDamaged", (parser, x) => x.SoundAmbientDamaged = parser.ParseAssetReference() },
            { "SoundAmbientReallyDamaged", (parser, x) => x.SoundAmbientReallyDamaged = parser.ParseAssetReference() },
            { "SoundAmbientRubble", (parser, x) => x.SoundAmbientRubble = parser.ParseAssetReference() },
            { "SoundAmbient2", (parser, x) => x.SoundAmbient2 = parser.ParseAssetReference() },
            { "SoundAmbientDamaged2", (parser, x) => x.SoundAmbientDamaged2 = parser.ParseAssetReference() },
            { "SoundAmbientReallyDamaged2", (parser, x) => x.SoundAmbientReallyDamaged2 = parser.ParseAssetReference() },
            { "SoundAmbientRubble2", (parser, x) => x.SoundAmbientRubble2 = parser.ParseAssetReference() },
            { "SoundAmbientBattle", (parser, x) => x.SoundAmbientBattle = parser.ParseAssetReference() },
            { "SoundCreated", (parser, x) => x.SoundCreated = parser.ParseAssetReference() },
            { "SoundEnter", (parser, x) => x.SoundEnter = parser.ParseAssetReference() },
            { "SoundExit", (parser, x) => x.SoundExit = parser.ParseAssetReference() },
            { "SoundPromotedVeteran", (parser, x) => x.SoundPromotedVeteran = parser.ParseAssetReference() },
            { "SoundPromotedElite", (parser, x) => x.SoundPromotedElite = parser.ParseAssetReference() },
            { "SoundPromotedHero", (parser, x) => x.SoundPromotedHero = parser.ParseAssetReference() },
            { "SoundFallingFromPlane", (parser, x) => x.SoundFallingFromPlane = parser.ParseAssetReference() },
            { "SoundImpact", (parser, x) => x.SoundImpact = parser.ParseAssetReference() },
            { "SoundImpactCyclonic", (parser, x) => x.SoundImpactCyclonic = parser.ParseAssetReference() },
            { "SoundCrushing", (parser, x) => x.SoundCrushing = parser.ParseAssetReference() },

            { "UnitSpecificSounds", (parser, x) => x.UnitSpecificSounds = UnitSpecificSounds.Parse(parser) },
            { "UnitSpecificFX", (parser, x) => x.UnitSpecificFX = UnitSpecificAssets.Parse(parser) },

            { "VoicePriority", (parser, x) => x.VoicePriority = parser.ParseInteger() },
            { "GroupVoiceThreshold", (parser, x) => x.GroupVoiceThreshold = parser.ParseInteger() },
            { "VoiceAmbushBlockingRadius", (parser, x) => x.VoiceAmbushBlockingRadius = parser.ParseInteger() },
            { "VoiceAmbushTimeout", (parser, x) => x.VoiceAmbushTimeout = parser.ParseInteger() },

            { "EvaEnemyUnitSightedEvent", (parser, x) => x.EvaEnemyUnitSightedEvent = parser.ParseAssetReference() },

            { "CampnessValue", (parser, x) => x.CampnessValue = parser.ParseInteger() },
            { "CampnessValueRadius", (parser, x) => x.CampnessValueRadius = parser.ParseInteger() },

            {
                "Behavior",
                (parser, x) =>
                {
                    var behavior = BehaviorModuleData.ParseBehavior(parser);
                    if (behavior is AIUpdateModuleData aiUpdate)
                    {
                        x.AIUpdate = aiUpdate;
                    }
                    else if (x.Behaviors.ContainsKey(behavior.Tag))
                    {
                        x.Behaviors[behavior.Tag] = behavior;
                    }
                    else
                    {
                        x.Behaviors.Add(behavior.Tag, behavior);
                    }
                }
            },

            { "Draw", (parser, x) => { var draw = DrawModuleData.ParseDrawModule(parser); x.Draws[draw.Tag] = draw; } },
            { "Body", (parser, x) => x.Body = BodyModuleData.ParseBody(parser) },
            { "ClientUpdate", (parser, x) => x.ClientUpdates.Add(ClientUpdateModuleData.ParseClientUpdate(parser)) },
            { "ClientBehavior", (parser, x) => x.ClientBehavior = ClientBehaviorModuleData.ParseClientBehavior(parser) },

            { "Locomotor", (parser, x) => x.LocomotorSets.Add(new LocomotorSet { Condition = parser.ParseEnum<LocomotorSetType>(), Locomotor = parser.ParseLocomotorTemplateReference(), Speed = 100 }) },
            { "LocomotorSet", (parser, x) => x.LocomotorSets.Add(LocomotorSet.Parse(parser)) },

            { "KindOf", (parser, x) => x.KindOf = parser.ParseEnumBitArray<ObjectKinds>() },
            { "RadarPriority", (parser, x) => x.RadarPriority = parser.ParseEnum<RadarPriority>() },
            { "EnterGuard", (parser, x) => x.EnterGuard = parser.ParseBoolean() },
            { "HijackGuard", (parser, x) => x.HijackGuard = parser.ParseBoolean() },
            { "DisplayColor", (parser, x) => x.DisplayColor = parser.ParseColorRgb() },
            { "Scale", (parser, x) => x.Scale = parser.ParseFloat() },

            { "Geometry", (parser, x) => x.ParseGeometry(parser) },
            { "AdditionalGeometry", (parser, x) => x.ParseAdditionalGeometry(parser) },

            { "GeometryName", (parser, x) => x.CurrentGeometry.Name = parser.ParseString() },
            { "GeometryMajorRadius", (parser, x) => x.CurrentGeometry.MajorRadius = parser.ParseFloat() },
            { "GeometryMinorRadius", (parser, x) => x.CurrentGeometry.MinorRadius = parser.ParseFloat() },
            { "GeometryHeight", (parser, x) => x.CurrentGeometry.Height = parser.ParseFloat() },
            { "GeometryIsSmall", (parser, x) => x.CurrentGeometry.IsSmall = parser.ParseBoolean() },
            { "GeometryOffset", (parser, x) => x.CurrentGeometry.Offset = parser.ParseVector3() },
            { "GeometryRotationAnchorOffset", (parser, x) => x.RotationAnchorOffset = parser.ParseVector2() },
            { "GeometryActive", (parser, x) => x.CurrentGeometry.IsActive = parser.ParseBoolean() },
            { "GeometryFrontAngle", (parser, x) => x.CurrentGeometry.FrontAngle = parser.ParseFloat() },

            { "GeometryOther", (parser, x) => x.ParseOtherGeometry(parser) },

            { "GeometryContactPoint", (parser, x) => x.GeometryContactPoints.Add(ContactPoint.Parse(parser)) },

            { "CamouflageDetectionMultiplier", (parser, x) => x.CamouflageDetectionMultiplier = parser.ParseFloat()},
            { "FactoryExitWidth", (parser, x) => x.FactoryExitWidth = parser.ParseInteger() },
            { "FactoryExtraBibWidth", (parser, x) => x.FactoryExtraBibWidth = parser.ParseFloat() },
            { "Shadow", (parser, x) => x.Shadow = parser.ParseEnum<ObjectShadowType>() },
            { "ShadowTexture", (parser, x) => x.ShadowTexture = parser.ParseAssetReference() },
            { "ShadowSizeX", (parser, x) => x.ShadowSizeX = parser.ParseFloat() },
            { "ShadowSizeY", (parser, x) => x.ShadowSizeY = parser.ParseFloat() },
            { "ShadowMaxHeight", (parser, x) => x.ShadowMaxHeight = parser.ParseInteger() },
            { "InstanceScaleFuzziness", (parser, x) => x.InstanceScaleFuzziness = parser.ParseFloat() },
            { "BuildCompletion", (parser, x) => x.BuildCompletion = parser.ParseAssetReference() },
            { "BuildVariations", (parser, x) => x.BuildVariations = parser.ParseAssetReferenceArray() },

            { "ExperienceScalarTable", (parser, x) => x.ExperienceScalarTable = parser.ParseAssetReference() },

            { "InheritableModule", (parser, x) => x.InheritableModules.Add(InheritableModule.Parse(parser)) },
            { "OverrideableByLikeKind", (parser, x) => x.OverrideableByLikeKinds.Add(OverrideableByLikeKind.Parse(parser)) },

            { "RemoveModule", (parser, x) => x.RemoveModules.Add(parser.ParseIdentifier()) },
            { "AddModule", (parser, x) => x.AddModules.Add(AddModule.Parse(parser)) },
            { "ReplaceModule", (parser, x) => x.ReplaceModules.Add(ReplaceModule.Parse(parser)) },

            { "ThreatLevel", (parser, x) => x.ThreatLevel = parser.ParseFloat() },
            { "ThingClass", (parser, x) => x.ThingClass = parser.ParseString() },
            { "MinCrushVelocityPercent", (parser, x) => x.MinCrushVelocityPercent = parser.ParsePercentage() },
            { "CrushDecelerationPercent", (parser, x) => x.CrushDecelerationPercent = parser.ParsePercentage() },
            { "RamPower", (parser, x) => x.RamPower = parser.ParseInteger() },
            { "RamZMult", (parser, x) => x.RamZMult = parser.ParseFloat() },
            { "CommandPoints", (parser, x) => x.CommandPoints = parser.ParseInteger() },
            { "EmotionRange", (parser, x) => x.EmotionRange = parser.ParseInteger() },
            { "ImmuneToShockwave", (parser, x) => x.ImmuneToShockwave = parser.ParseBoolean() },
            { "ShowHealthInSelectionDecal", (parser, x) => x.ShowHealthInSelectionDecal = parser.ParseBoolean() },
            { "DeadCollideSize", (parser, x) => x.DeadCollideSize = parser.ParseEnum<CollideSize>() },
            { "CrushKnockback", (parser, x) => x.CrushKnockback = parser.ParseInteger() },
            { "CrushZFactor", (parser, x) => x.CrushZFactor = parser.ParseFloat() },
            { "BountyValue", (parser, x) => x.BountyValue = parser.ParseInteger() },
            { "Description", (parser, x) => x.Description = parser.ParseLocalizedStringKey() },

            { "FormationWidth", (parser, x) => x.FormationWidth = parser.ParseInteger() },
            { "FormationDepth", (parser, x) => x.FormationDepth = parser.ParseInteger() },
            { "KeepSelectableWhenDead", (parser, x) => x.KeepSelectableWhenDead = parser.ParseBoolean() },
            { "LiveCameraOffset", (parser, x) => x.LiveCameraOffset = parser.ParseVector3() },
            { "LiveCameraPitch", (parser, x) => x.LiveCameraPitch = parser.ParseFloat() },
            { "EvaEventDieOwner", (parser, x) => x.EvaEventDieOwner = parser.ParseAssetReference() },
            { "AttackContactPoint", (parser, x) => x.AttackContactPoints.Add(ContactPoint.Parse(parser)) },
            { "RemoveTerrainRadius", (parser, x) => x.RemoveTerrainRadius = parser.ParseFloat() },
            { "HealthBoxScale", (parser, x) => x.HealthBoxScale = parser.ParseFloat() },
            { "HealthBoxHeightOffset", (parser, x) => x.HealthBoxHeightOffset = parser.ParseFloat() },
            { "ForceLuaRegistration", (parser, x) => x.ForceLuaRegistration = parser.ParseBoolean() },
            { "CrushRevengeWeapon", (parser, x) => x.CrushRevengeWeapon = parser.ParseAssetReference() },
            { "EvaEventDamagedOwner", (parser, x) => x.EvaEventDamagedOwner = parser.ParseAssetReference() },
            { "MountedCrusherLevel", (parser, x) => x.MountedCrusherLevel = parser.ParseInteger() },
            { "MountedCrushableLevel", (parser, x) => x.MountedCrushableLevel = parser.ParseInteger() },
            { "CrushWeapon", (parser, x) => x.CrushWeapon = parser.ParseAssetReference() },
            { "DisplayMeleeDamage", (parser, x) => x.DisplayMeleeDamage = parser.ParseFloat() },
            { "RecruitText", (parser, x) => x.RecruitText = parser.ParseLocalizedStringKey() },
            { "ReviveText", (parser, x) => x.ReviveText = parser.ParseLocalizedStringKey() },
            { "Hotkey", (parser, x) => x.Hotkey = parser.ParseLocalizedStringKey() },
            { "PathfindDiameter", (parser, x) => x.PathfindDiameter = parser.ParseFloat() },
            { "DisplayRangedDamage", (parser, x) => x.DisplayRangedDamage = parser.ParseFloat() },
            { "CanPathThroughGates", (parser, x) => x.CanPathThroughGates = parser.ParseBoolean() },
            { "ShadowSunAngle", (parser, x) => x.ShadowSunAngle = parser.ParseInteger() },
            { "ShouldClearShotsOnIdle", (parser, x) => x.ShouldClearShotsOnIdle = parser.ParseBoolean() },
            { "UseCrushAttack", (parser, x) => x.UseCrushAttack = parser.ParseBoolean() },
            { "CrushAllies", (parser, x) => x.CrushAllies = parser.ParseBoolean() },
            { "ShadowOpacityStart", (parser, x) => x.ShadowOpacityStart = parser.ParseInteger() },
            { "ShadowOpacityFadeInTime", (parser, x) => x.ShadowOpacityFadeInTime = parser.ParseInteger() },
            { "ShadowOpacityPeak", (parser, x) => x.ShadowOpacityPeak = parser.ParseInteger() },
            { "ShadowOpacityFadeOutTime", (parser, x) => x.ShadowOpacityFadeOutTime = parser.ParseInteger() },
            { "ShadowOpacityEnd", (parser, x) => x.ShadowOpacityEnd = parser.ParseInteger() },
            { "ShadowOverrideLODVisibility", (parser, x) => x.ShadowOverrideLodVisibility = parser.ParseBoolean() },
            { "EquivalentTo", (parser, x) => x.EquivalentTo = parser.ParseIdentifier() },
            { "HeroSortOrder", (parser, x) => x.HeroSortOrder = parser.ParseInteger() },
            { "IsAutoBuilt", (parser, x) => x.IsAutoBuilt = parser.ParseBoolean() },
            { "IsGrabbable", (parser, x) => x.IsGrabbable = parser.ParseBoolean() },
            { "IsHarvestable", (parser, x) => x.IsHarvestable = parser.ParseBoolean() },
            { "ShadowOffsetX", (parser, x) => x.ShadowOffsetX = parser.ParseInteger() },
            { "EvaEventDamagedFromShroudedSourceOwner", (parser, x) => x.EvaEventDamagedFromShroudedSourceOwner = parser.ParseAssetReference() },
            { "EvaEventDamagedByFireOwner", (parser, x) => x.EvaEventDamagedByFireOwner = parser.ParseAssetReference() },
            { "EvaEventAmbushed", (parser, x) => x.EvaEventAmbushed = parser.ParseAssetReference() },
            { "EvaEventSecondDamageFarFromFirstOwner", (parser, x) => x.EvaEventSecondDamageFarFromFirstOwner = parser.ParseAssetReference() },
            { "EvaEventSecondDamageFarFromFirstScanRange", (parser, x) => x.EvaEventSecondDamageFarFromFirstScanRange = parser.ParseInteger() },
            { "EvaEventSecondDamageFarFromFirstTimeoutMS", (parser, x) => x.EvaEventSecondDamageFarFromFirstTimeoutMS = parser.ParseInteger() },
            { "EvaEnemyObjectSightedEvent", (parser, x) => x.EvaEnemyObjectSightedEvent = parser.ParseAssetReference() },
            { "ShockwaveResistance", (parser, x) => x.ShockwaveResistance = parser.ParseFloat() },
            { "VisionSide", (parser, x) => x.VisionSide = parser.ParsePercentage() },
            { "VisionRear", (parser, x) => x.VisionRear = parser.ParsePercentage() },
            { "VisionBonusPercentPerFoot", (parser, x) => x.VisionBonusPercentPerFoot = parser.ParsePercentage() },
            { "MaxVisionBonusPercent", (parser, x) => x.MaxVisionBonusPercent = parser.ParsePercentage() },
            { "VisionBonusTestRadius", (parser, x) => x.VisionBonusTestRadius = parser.ParseInteger() },
            { "CrowdResponseKey", (parser, x) => x.CrowdResponseKey = parser.ParseIdentifier() },
            { "CommandPointBonus", (parser, x) => x.CommandPointBonus = parser.ParseInteger() },
            { "Flammability", (parser, x) => x.Flammability = Flammability.Parse(parser) },
            { "ThreatBreakdown", (parser, x) => x.ThreatBreakdown = ThreatBreakdown.Parse(parser) },
            { "DisplayNameInvisibleForEnemy", (parser, x) => x.DisplayNameInvisibleForEnemy = parser.ParseLocalizedStringKey() },
            { "DescriptionStrategic", (parser, x) => x.DescriptionStrategic = parser.ParseLocalizedStringKey() },
            { "SupplyOverride", (parser, x) => x.SupplyOverride = parser.ParseInteger() },
            { "AutoResolveUnitType", (parser, x) => x.AutoResolveUnitType = parser.ParseAssetReference() },
            { "AutoResolveCombatChain", (parser, x) => x.AutoResolveCombatChain = parser.ParseAssetReference() },
            { "AutoResolveBody", (parser, x) => x.AutoResolveBody = parser.ParseAssetReference() },
            { "AutoResolveArmor", (parser, x) => x.AutoResolveArmor = AutoResolveArmorReference.Parse(parser) },
            { "AutoResolveWeapon", (parser, x) => x.AutoResolveWeapon = AutoResolveWeaponReference.Parse(parser) },
            { "DisplayNameStrategic", (parser, x) => x.DisplayNameStrategic = parser.ParseLocalizedStringKey() },
            { "WorldMapArmoryUpgradesAllowed", (parser, x) => x.WorldMapArmoryUpgradesAllowed = parser.ParseAssetReferenceArray() },
            { "FormationPreviewItemDecal", (parser, x) => x.FormationPreviewItemDecal = Decal.Parse(parser) },
            { "GeometryUsedForHealthBox", (parser, x) => x.GeometryUsedForHealthBox = parser.ParseBoolean() },
            { "AutoResolveLeadership", (parser, x) => x.AutoResolveLeadership = parser.ParseAssetReference() },
            { "FormationPreviewDecal", (parser, x) => x.FormationPreviewDecal = Decal.Parse(parser) },
            { "EvaEnemyObjectSightedAfterRespawnEvent", (parser, x) => x.EvaEnemyObjectSightedAfterRespawnEvent = parser.ParseAssetReference() },
            { "SelectionPriority", (parser, x) => x.SelectionPriority = parser.ParseInteger() },
            { "CrushOnlyWhileCharging", (parser, x) => x.CrushOnlyWhileCharging = parser.ParseBoolean() },
            { "MinZIncreaseForVoiceMoveToHigherGround", (parser, x) => x.MinZIncreaseForVoiceMoveToHigherGround = parser.ParseInteger() },
            { "EvaOnFirstSightingEventEnemy", (parser, x) => x.EvaOnFirstSightingEventEnemy = parser.ParseAssetReference() },
            { "EvaOnFirstSightingEventNonEnemy", (parser, x) => x.EvaOnFirstSightingEventNonEnemy = parser.ParseAssetReference() },
            { "EvaEventDetectedEnemy", (parser, x) => x.EvaEventDetectedEnemy = parser.ParseAssetReference() },
            { "EvaEventDetectedAlly", (parser, x) => x.EvaEventDetectedAlly = parser.ParseAssetReference() },
            { "EvaEventDetectedOwner", (parser, x) => x.EvaEventDetectedOwner = parser.ParseAssetReference() },
        };

        // Art
        public int PlacementViewAngle { get; private set; }
        public LazyAssetReference<MappedImage> SelectPortrait { get; private set; }
        public LazyAssetReference<MappedImage> ButtonImage { get; private set; }
        public LazyAssetReference<UpgradeTemplate> UpgradeCameo1 { get; private set; }
        public LazyAssetReference<UpgradeTemplate> UpgradeCameo2 { get; private set; }
        public LazyAssetReference<UpgradeTemplate> UpgradeCameo3 { get; private set; }
        public LazyAssetReference<UpgradeTemplate> UpgradeCameo4 { get; private set; }
        public LazyAssetReference<UpgradeTemplate> UpgradeCameo5 { get; private set; }

        // Design
        public ObjectBuildableType Buildable { get; private set; }
        public string Side { get; private set; }
        public string DisplayName { get; private set; }
        public ObjectEditorSortingFlags EditorSorting { get; private set; }
        public ObjectBrowserFlags Browser { get; private set; }
        public int TransportSlotCount { get; private set; }
        public float VisionRange { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public float ShroudRevealToAllRange { get; private set; }

        public float ShroudClearingRange { get; private set; }
        public int CrusherLevel { get; private set; }
        public int CrushableLevel { get; private set; }
        public float BuildCost { get; private set; }

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
        public Dictionary<BitArray<WeaponSetConditions>, WeaponTemplateSet> WeaponSets { get; internal set; } = new Dictionary<BitArray<WeaponSetConditions>, WeaponTemplateSet>();
        public Dictionary<BitArray<ArmorSetCondition>, ArmorTemplateSet> ArmorSets { get; internal set; } = new Dictionary<BitArray<ArmorSetCondition>, ArmorTemplateSet>();
        public LazyAssetReference<CommandSet> CommandSet { get; private set; }
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
        public LazyAssetReference<BaseAudioEventInfo> VoiceSelect { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string VoiceSelectGroup { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string VoiceSelectBattle { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string VoiceSelectBattleGroup { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string VoiceSelectUnderConstruction { get; private set; }

        public LazyAssetReference<BaseAudioEventInfo> VoiceMove { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceMoveGroup { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceMoveOverWall { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceMoveToHigherGround { get; private set; }

        public LazyAssetReference<BaseAudioEventInfo> VoiceGuard { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceGuardGroup { get; private set; }

        public LazyAssetReference<BaseAudioEventInfo> VoiceAttack { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceAttackGroup { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceAttackCharge { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceAttackChargeGroup { get; private set; }

        public LazyAssetReference<BaseAudioEventInfo> VoiceAttackAir { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceAttackAirGroup { get; private set; }

        public LazyAssetReference<BaseAudioEventInfo> VoiceGroupSelect { get; private set; }
        public LazyAssetReference<BaseAudioEventInfo> VoiceEnter { get; private set; }
        public LazyAssetReference<BaseAudioEventInfo> VoiceGarrison { get; private set; }
        public LazyAssetReference<BaseAudioEventInfo> VoiceFear { get; private set; }
        public LazyAssetReference<BaseAudioEventInfo> VoiceSelectElite { get; private set; }
        public LazyAssetReference<BaseAudioEventInfo> VoiceCreated { get; private set; }
        public LazyAssetReference<BaseAudioEventInfo> VoiceTaskUnable { get; private set; }
        public LazyAssetReference<BaseAudioEventInfo> VoiceTaskComplete { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceDefect { get; private set; }

        public LazyAssetReference<BaseAudioEventInfo> VoiceMeetEnemy { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceAlert { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceFullyCreated { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceRetreatToCastle { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceRetreatToCastleGroup { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceMoveToCamp { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceMoveToCampGroup { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceAttackStructure { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceAttackStructureGroup { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceAttackMachine { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceAttackMachineGroup { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceMoveWhileAttacking { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceMoveWhileAttackingGroup { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceAmbushed { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceCombineWithHorde { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceEnterStateAttack { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceEnterStateAttackCharge { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceEnterStateAttackAir { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceEnterStateAttackStructure { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceEnterStateAttackMachine { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceEnterStateMove { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceEnterStateRetreatToCastle { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceEnterStateMoveToCamp { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceEnterStateMoveWhileAttacking { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceEnterStateMoveToHigherGround { get; private set; } 

        [AddedIn(SageGame.Bfme2)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceEnterStateMoveOverWalls { get; private set; } 

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceSelect2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceSelectGroup2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceSelectBattle2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceSelectBattleGroup2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceMove2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceMoveGroup2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceGuard2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceGuardGroup2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceAttack2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceAttackGroup2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceAttackCharge2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceAttackChargeGroup2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceAttackAir2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceAttackAirGroup2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceFear2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceCreated2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceTaskComplete2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceDefect2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceAlert2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceFullyCreated2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceRetreatToCastle2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceRetreatToCastleGroup2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceMoveToCamp2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceMoveToCampGroup2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceAttackStructure2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceAttackStructureGroup2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceAttackMachine2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceAttackMachineGroup2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceMoveWhileAttacking2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceMoveWhileAttackingGroup2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceAmbushed2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceCombineWithHorde2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceEnterStateAttack2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceEnterStateAttackCharge2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceEnterStateAttackAir2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceEnterStateAttackStructure2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceEnterStateAttackMachine2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceEnterStateMove2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceEnterStateRetreatToCastle2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceEnterStateMoveToCamp2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<BaseAudioEventInfo> VoiceEnterStateMoveWhileAttacking2 { get; private set; }

        public string SoundMoveStart { get; private set; }
        public string SoundMoveStartDamaged { get; private set; }
        public string SoundMoveLoop { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string SoundMoveLoopDamaged { get; private set; }

        public LazyAssetReference<BaseAudioEventInfo> SoundOnDamaged { get; private set; }
        public LazyAssetReference<BaseAudioEventInfo> SoundOnReallyDamaged { get; private set; }
        public LazyAssetReference<BaseAudioEventInfo> SoundDie { get; private set; }
        public string SoundDieFire { get; private set; }
        public string SoundDieToxin { get; private set; }
        public string SoundStealthOn { get; private set; }
        public string SoundStealthOff { get; private set; }
        public string SoundCrush { get; private set; }
        public string SoundAmbient { get; private set; }
        public string SoundAmbientDamaged { get; private set; }
        public string SoundAmbientReallyDamaged { get; private set; }
        public string SoundAmbientRubble { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string SoundAmbient2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string SoundAmbientDamaged2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string SoundAmbientReallyDamaged2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string SoundAmbientRubble2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string SoundAmbientBattle { get; private set; }

        public string SoundCreated { get; private set; }
        public string SoundEnter { get; private set; }
        public string SoundExit { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string SoundPromotedVeteran { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string SoundPromotedElite { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string SoundPromotedHero { get; private set; }

        public string SoundFallingFromPlane { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string SoundImpact { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string SoundImpactCyclonic { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string SoundCrushing { get; private set; }

        public UnitSpecificSounds UnitSpecificSounds { get; private set; } = new UnitSpecificSounds();
        public UnitSpecificAssets UnitSpecificFX { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int VoicePriority { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int GroupVoiceThreshold { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int VoiceAmbushBlockingRadius { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int VoiceAmbushTimeout { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string EvaEnemyUnitSightedEvent { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int CampnessValue { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int CampnessValueRadius { get; private set; }

        // Engineering
        public AIUpdateModuleData AIUpdate { get; private set; }
        public Dictionary<string, BehaviorModuleData> Behaviors { get; internal set; } = new Dictionary<string, BehaviorModuleData>();
        public Dictionary<string, DrawModuleData> Draws { get; internal set; } = new Dictionary<string, DrawModuleData>();
        public BodyModuleData Body { get; private set; }
        public List<ClientUpdateModuleData> ClientUpdates { get; internal set; } = new List<ClientUpdateModuleData>();

        [AddedIn(SageGame.Bfme)]
        public List<LocomotorSet> LocomotorSets { get; internal set; } = new List<LocomotorSet>();

        public BitArray<ObjectKinds> KindOf { get; private set; }
        public RadarPriority RadarPriority { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool EnterGuard { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool HijackGuard { get; private set; }

        public ColorRgb DisplayColor { get; private set; }
        public float Scale { get; private set; }

        public Geometry Geometry { get; private set; } = new Geometry();

        public Vector2 RotationAnchorOffset { get; set; }

        [AddedIn(SageGame.Bfme)]
        public List<Geometry> AdditionalGeometries {  get; private set; } = new List<Geometry>();

        [AddedIn(SageGame.Bfme)]
        public List<Geometry> OtherGeometries { get; private set; } = new List<Geometry>(); //for crushing/squishing detection

        [AddedIn(SageGame.Bfme2)]
        public float CamouflageDetectionMultiplier { get; private set; }

        /// <summary>
        /// Amount of space to leave for exiting units.
        /// </summary>
        public int FactoryExitWidth { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public float FactoryExtraBibWidth { get; private set; }

        public ObjectShadowType Shadow { get; private set; }
        public string ShadowTexture { get; private set; }
        public float ShadowSizeX { get; private set; }
        public float ShadowSizeY { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int ShadowMaxHeight { get; private set; }
        public float InstanceScaleFuzziness { get; private set; }
        public string BuildCompletion { get; private set; }
        public string[] BuildVariations { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string ExperienceScalarTable { get; private set; }

        public List<InheritableModule> InheritableModules { get; private set; } = new List<InheritableModule>();
        public List<OverrideableByLikeKind> OverrideableByLikeKinds { get; } = new List<OverrideableByLikeKind>();

        // Map.ini module modifications
        public List<string> RemoveModules { get; } = new List<string>();
        public List<AddModule> AddModules { get; } = new List<AddModule>();
        public List<ReplaceModule> ReplaceModules { get; } = new List<ReplaceModule>();

        [AddedIn(SageGame.Bfme)]
        public float ThreatLevel { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ClientBehaviorModuleData ClientBehavior { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string ThingClass { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Percentage MinCrushVelocityPercent { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Percentage CrushDecelerationPercent { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int RamPower { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float RamZMult { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int CommandPoints { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int EmotionRange { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool ImmuneToShockwave { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool ShowHealthInSelectionDecal { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string BuildFadeInOnCreateList { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float BuildFadeInOnCreateTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public CollideSize DeadCollideSize { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int CrushKnockback { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float CrushZFactor { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int BountyValue { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public List<ContactPoint> GeometryContactPoints { get; private set; } = new List<ContactPoint>();

        [AddedIn(SageGame.Bfme)]
        public string Description { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int FormationWidth { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int FormationDepth { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool KeepSelectableWhenDead { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Vector3 LiveCameraOffset { get; private set; }
        
        [AddedIn(SageGame.Bfme)]
        public float LiveCameraPitch { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string EvaEventDieOwner { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public List<ContactPoint> AttackContactPoints { get; private set; } = new List<ContactPoint>();

        [AddedIn(SageGame.Bfme)]
        public float RemoveTerrainRadius { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float HealthBoxScale { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float HealthBoxHeightOffset { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool ForceLuaRegistration { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string CrushRevengeWeapon { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string EvaEventDamagedOwner { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int MountedCrusherLevel { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int MountedCrushableLevel { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string CrushWeapon { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float DisplayMeleeDamage { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string RecruitText { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string ReviveText { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string Hotkey { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float PathfindDiameter { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float DisplayRangedDamage { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool CanPathThroughGates { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int ShadowSunAngle { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool ShouldClearShotsOnIdle { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool UseCrushAttack { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool CrushAllies { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int ShadowOpacityStart { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int ShadowOpacityFadeInTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int ShadowOpacityPeak { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int ShadowOpacityFadeOutTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int ShadowOpacityEnd { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool ShadowOverrideLodVisibility { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string EquivalentTo { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int HeroSortOrder { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool IsAutoBuilt { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool IsGrabbable { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool IsHarvestable { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int ShadowOffsetX { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int SupplyOverride { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string EvaEventDamagedFromShroudedSourceOwner { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string EvaEventDamagedByFireOwner { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string EvaEventAmbushed { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string EvaEventSecondDamageFarFromFirstOwner { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int EvaEventSecondDamageFarFromFirstScanRange { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int EvaEventSecondDamageFarFromFirstTimeoutMS { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string EvaEnemyObjectSightedEvent { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float ShockwaveResistance { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage VisionSide { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage VisionRear { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage VisionBonusPercentPerFoot { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage MaxVisionBonusPercent { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int VisionBonusTestRadius { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string CrowdResponseKey { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int CommandPointBonus { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Flammability Flammability { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public ThreatBreakdown ThreatBreakdown { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string DisplayNameInvisibleForEnemy { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string DescriptionStrategic { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string AutoResolveUnitType { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string AutoResolveCombatChain { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string AutoResolveBody { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public AutoResolveArmorReference AutoResolveArmor { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public AutoResolveWeaponReference AutoResolveWeapon { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string DisplayNameStrategic { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string[] WorldMapArmoryUpgradesAllowed { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Decal FormationPreviewItemDecal { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool GeometryUsedForHealthBox { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string AutoResolveLeadership { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Decal FormationPreviewDecal { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string EvaEnemyObjectSightedAfterRespawnEvent { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int SelectionPriority { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool CrushOnlyWhileCharging { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int MinZIncreaseForVoiceMoveToHigherGround { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string EvaOnFirstSightingEventEnemy { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string EvaOnFirstSightingEventNonEnemy { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string EvaEventDetectedEnemy { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string EvaEventDetectedAlly { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string EvaEventDetectedOwner { get; private set; }

        internal ObjectDefinition CloneForImplicitInheritance()
        {
            var result = (ObjectDefinition) MemberwiseClone();

            // TODO: Clone any other lists.

            result.KindOf = new BitArray<ObjectKinds>(result.KindOf);
            result.UnitSpecificSounds = new UnitSpecificSounds(result.UnitSpecificSounds);
            result.Geometry = result.Geometry.Clone();
            result.Behaviors = new Dictionary<string, BehaviorModuleData>();
            result.Draws = new Dictionary<string, DrawModuleData>();
            result.ClientUpdates = new List<ClientUpdateModuleData>();
            result.WeaponSets = new Dictionary<BitArray<WeaponSetConditions>, WeaponTemplateSet>(result.WeaponSets);
            result.ArmorSets = new Dictionary<BitArray<ArmorSetCondition>, ArmorTemplateSet>(result.ArmorSets);
            result.LocomotorSets = new List<LocomotorSet>(result.LocomotorSets);

            foreach (var inheritableModule in result.InheritableModules)
            {
                result.Behaviors.Add(inheritableModule.Module.Tag, inheritableModule.Module);
            }

            result.InheritableModules = null;

            return result;
        }

        internal ObjectDefinition CloneForExplicitInheritance()
        {
            var result = (ObjectDefinition) MemberwiseClone();

            // TODO: Clone any other lists.

            result.KindOf = new BitArray<ObjectKinds>(result.KindOf);
            result.UnitSpecificSounds = new UnitSpecificSounds(result.UnitSpecificSounds);
            result.Geometry = result.Geometry.Clone();
            result.Behaviors = new Dictionary<string, BehaviorModuleData>(result.Behaviors);
            result.Draws = new Dictionary<string, DrawModuleData>(result.Draws);
            result.ClientUpdates = new List<ClientUpdateModuleData>(result.ClientUpdates);
            result.WeaponSets = new Dictionary<BitArray<WeaponSetConditions>, WeaponTemplateSet>(result.WeaponSets);
            result.ArmorSets = new Dictionary<BitArray<ArmorSetCondition>, ArmorTemplateSet>(result.ArmorSets);
            result.LocomotorSets = new List<LocomotorSet>(result.LocomotorSets);

            return result;
        }

        private Geometry CurrentGeometry { get; set; }

        internal void ParseGeometry(IniParser parser)
        {
            Geometry = new Geometry(parser.ParseEnum<ObjectGeometry>());
            CurrentGeometry = Geometry;
        }

        internal void ParseAdditionalGeometry(IniParser parser)
        {
            var geometry = new Geometry(parser.ParseEnum<ObjectGeometry>());
            AdditionalGeometries.Add(geometry);
            CurrentGeometry = geometry;
        }

        internal void ParseOtherGeometry(IniParser parser)
        {
            var geometry = Geometry.Parse(parser);
            OtherGeometries.Add(geometry);
            CurrentGeometry = geometry;
        }
    }

    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public struct MaxSimultaneousObjectCount
    {
        internal static MaxSimultaneousObjectCount Parse(IniParser parser)
        {
            var token = parser.GetNextToken();
            if (parser.IsInteger(token))
            {
                return new MaxSimultaneousObjectCount
                {
                    CountType = MaxSimultaneousObjectCountType.Explicit,
                    ExplicitCount = parser.ScanInteger(token)
                };
            }

            if (token.Text != "DeterminedBySuperweaponRestriction")
            {
                throw new IniParseException("Unknown MaxSimultaneousOfType value: " + token.Text, token.Position);
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

    public sealed class AddModule : ObjectDefinition
    {
        internal static new AddModule Parse(IniParser parser)
        {
            var name = parser.GetNextTokenOptional();

            var result = parser.ParseBlock(FieldParseTable);

            result.SetNameAndInstanceId("GameObject", name?.Text);

            return result;
        }

        private static new readonly IniParseTable<AddModule> FieldParseTable = ObjectDefinition.FieldParseTable
            .Concat(new IniParseTable<AddModule>());
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
            { "Behavior", (parser, x) => x.Module = BehaviorModuleData.ParseBehavior(parser) },
            { "Draw", (parser, x) => x.Module = DrawModuleData.ParseDrawModule(parser) },
            { "Body", (parser, x) => x.Module = BodyModuleData.ParseBody(parser) },
            { "ClientBehavior", (parser, x) => x.Module = ClientBehaviorModuleData.ParseClientBehavior(parser) },
            { "ArmorSet", (parser, x) => x.ArmorSet = ArmorTemplateSet.Parse(parser) },
            { "LocomotorSet", (parser, x) => x.LocomotorSet = LocomotorSet.Parse(parser) },
            { "CrushableLevel", (parser, x) => x.CrushableLevel = parser.ParseInteger() },
            { "CrusherLevel", (parser, x) => x.CrusherLevel = parser.ParseInteger() },
        };

        public string Name { get; private set; }

        public ModuleData Module { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ArmorTemplateSet ArmorSet { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public LocomotorSet LocomotorSet { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int CrushableLevel { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int CrusherLevel { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class Geometry
    {
        public Geometry() { }

        public Geometry(ObjectGeometry type)
        {
            Type = type;
        }

        internal static Geometry Parse(IniParser parser)
        {
            return new Geometry()
            {
                Type = parser.ParseAttributeEnum<ObjectGeometry>("GeomType"),
                IsSmall = parser.ParseAttributeBoolean("IsSmall"),
                Height = parser.ParseAttributeInteger("Height"),
                MajorRadius = parser.ParseAttributeInteger("MajorRadius"),
                MinorRadius = parser.ParseAttributeInteger("MinorRadius"),
                OffsetX = parser.ParseAttributeInteger("OffsetX")
            };
        }

        public string Name { get; set; }
        public ObjectGeometry Type { get; set; }
        public bool IsSmall { get; set; }
        public float Height { get; set; }
        public float MajorRadius { get; set; }
        public float MinorRadius { get; set; }
        public int OffsetX { get; set; }
        public Vector3 Offset { get; set; }
        public bool IsActive { get; set; }
        public float FrontAngle { get; set; }

        public Geometry Clone() => (Geometry) MemberwiseClone();
    }

    [AddedIn(SageGame.Bfme)]
    public enum CollideSize
    {
        [IniEnum("LARGE")]
        Large,
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class ContactPoint
    {
        internal static ContactPoint Parse(IniParser parser)
        {
            return new ContactPoint()
            {
                Position = parser.ParseVector3(),
                Type = parser.ParseEnumFlags<ContactPointType>()
            };
        }

        public Vector3 Position { get; private set; }
        public ContactPointType Type { get; internal set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public sealed class Flammability
    {
        internal static Flammability Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<Flammability> FieldParseTable = new IniParseTable<Flammability>
        {
            { "Fuel", (parser, x) => x.Fuel = parser.ParseAssetReference() },
            { "MaxBurnRate", (parser, x) => x.MaxBurnRate = parser.ParseInteger() },
            { "Decay", (parser, x) => x.Decay = parser.ParseInteger() },
            { "Resistance", (parser, x) => x.Resistance = parser.ParseInteger() },
            { "FuelFactor", (parser, x) => x.FuelFactor = parser.ParseFloat() }
        };

        public string Fuel { get; private set; }
        public int MaxBurnRate { get; private set; }
        public int Decay { get; private set; }
        public int Resistance { get; private set; }
        public float FuelFactor { get; private set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public sealed class ThreatBreakdown
    {
        internal static ThreatBreakdown Parse(IniParser parser)
        {
            return parser.ParseNamedBlock((x, name) => x.ModuleTag = name, FieldParseTable);
        }

        internal static readonly IniParseTable<ThreatBreakdown> FieldParseTable = new IniParseTable<ThreatBreakdown>
        {
            { "AIKindOf", (parser, x) => x.AiKindOf = parser.ParseEnum<ObjectKinds>() },
        };

        public string ModuleTag { get; private set; }
        public ObjectKinds AiKindOf { get; private set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public sealed class AutoResolveArmorReference
    {
        internal static AutoResolveArmorReference Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<AutoResolveArmorReference> FieldParseTable = new IniParseTable<AutoResolveArmorReference>
        {
            { "Armor", (parser, x) => x.Armor = parser.ParseAssetReference() },
            { "RequiredUpgrades", (parser, x) => x.RequiredUpgrades = parser.ParseAssetReferenceArray() },
            { "ExcludedUpgrades", (parser, x) => x.ExcludedUpgrades = parser.ParseAssetReferenceArray() },
        };

        public string Armor { get; private set; }
        public string[] RequiredUpgrades { get; private set; }
        public string[] ExcludedUpgrades { get; private set; }
    }


    [AddedIn(SageGame.Bfme2)]
    public sealed class AutoResolveWeaponReference
    {
        internal static AutoResolveWeaponReference Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<AutoResolveWeaponReference> FieldParseTable = new IniParseTable<AutoResolveWeaponReference>
        {
            { "Weapon", (parser, x) => x.Weapon = parser.ParseAssetReference() },
            { "RequiredUpgrades", (parser, x) => x.RequiredUpgrades = parser.ParseAssetReferenceArray() },
            { "ExcludedUpgrades", (parser, x) => x.ExcludedUpgrades = parser.ParseAssetReferenceArray() },
        };

        public string Weapon { get; private set; }
        public string[] RequiredUpgrades { get; private set; }
        public string[] ExcludedUpgrades { get; private set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public class Decal
    {
        internal static Decal Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<Decal> FieldParseTable = new IniParseTable<Decal>
        {
            { "Texture", (parser, x) => x.Texture = parser.ParseAssetReference() },
            { "Width", (parser, x) => x.Width = parser.ParseInteger() },
            { "Height", (parser, x) => x.Height = parser.ParseInteger() },
        };

        public string Texture { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public enum ContactPointType
    {
        None = 0,

        [IniEnum("Repair")]
        Repair,

        [IniEnum("Swoop")]
        Swoop,

        [IniEnum("Grab")]
        Grab,

        [IniEnum("Ram")]
        Ram,

        [IniEnum("Bomb")]
        Bomb,

        [IniEnum("Menu")]
        Menu,
    }
}
