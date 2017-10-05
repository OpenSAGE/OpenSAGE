using System;
using System.Numerics;
using System.Threading.Tasks;
using OpenSage.Data.Map;

namespace OpenSage.Scripting
{
    public class ScriptComponent : EntityComponent
    {
        private GameTime _previousEvaluationTime;

        public Script Script { get; set; }

        public async Task Execute(ScriptExecutionContext context)
        {
            if (!Script.IsActive)
            {
                return;
            }

            if (Script.EvaluationInterval > 0)
            {
                var numSeconds = Game.UpdateTime.TotalGameTime.TotalSeconds - _previousEvaluationTime.TotalGameTime.TotalSeconds;
                if (numSeconds < Script.EvaluationInterval)
                {
                    return;
                }
            }

            _previousEvaluationTime = Game.UpdateTime;

            var conditionIsTrue = EvaluateConditions(Script.OrConditions);
            if (conditionIsTrue)
            {
                await ExecuteActions(Script.ActionsIfTrue, context);

                if (Script.DeactivateUponSuccess)
                {
                    Script.IsActive = false;
                }
            }
            else
            {
                await ExecuteActions(Script.ActionsIfFalse, context);
            }
        }

        private async Task ExecuteActions(ScriptAction[] actions, ScriptExecutionContext context)
        {
            foreach (var action in actions)
            {
                await ExecuteAction(action, context);
            }
        }

        private async Task ExecuteAction(ScriptAction action, ScriptExecutionContext context)
        {
            switch (action.ContentType)
            {
                case ScriptActionType.DebugMessageBox:
                    break;

                case ScriptActionType.SetFlag:
                    Scripting.Flags[action.Arguments[0].StringValue] = action.Arguments[1].UintValueAsBool;
                    break;

                case ScriptActionType.SetCounter:
                    break;
                case ScriptActionType.Victory:
                    break;
                case ScriptActionType.Defeat:
                    break;
                case ScriptActionType.NoOp:
                    break;

                case ScriptActionType.SetTimer:
                    break;

                case ScriptActionType.PlaySoundEffect:
                    break;
                case ScriptActionType.EnableScript:
                    break;
                case ScriptActionType.DisableScript:
                    break;
                case ScriptActionType.CallSubroutine:
                    break;
                case ScriptActionType.PlaySoundEffectAt:
                    break;
                case ScriptActionType.DamageMembersOfTeam:
                    break;
                case ScriptActionType.MoveTeamTo:
                    break;

                case ScriptActionType.MoveCameraTo:
                    break;

                case ScriptActionType.IncrementCounter:
                    break;
                case ScriptActionType.DecrementCounter:
                    break;

                case ScriptActionType.MoveCameraAlongWaypointPath:
                    var waypointPath = Scene.Settings.WaypointPaths[action.Arguments[0].StringValue];
                    var startWaypoint = waypointPath.Start;
                    var endWaypoint = waypointPath.End;
                    var direction = endWaypoint.Position - startWaypoint.Position;
                    var timeSeconds = action.Arguments[1].FloatValue.Value;
                    var cameraShutter = action.Arguments[2].FloatValue.Value; // ?
                    var startTime = Game.UpdateTime.TotalGameTime;
                    var endTime = startTime + TimeSpan.FromSeconds(timeSeconds);
                    while (Game.UpdateTime.TotalGameTime < endTime)
                    {
                        var currentTimeFraction = (float) ((endTime - Game.UpdateTime.TotalGameTime).TotalSeconds / (endTime - startTime).TotalSeconds);
                        var currentPosition = startWaypoint.Position + direction * currentTimeFraction;
                        Scene.MainCamera.Transform.WorldPosition = currentPosition;
                        Scene.MainCamera.Transform.Translate(new Vector3(0, 0, 50));
                        // Interpolate distance along waypoint path.
                        await context.WaitForNextFrame();
                    }
                    break;

                case ScriptActionType.RotateCamera:
                    break;
                case ScriptActionType.ResetCamera:
                    break;

                case ScriptActionType.SetMillisecondTimer:
                    Scripting.Timers[action.Arguments[0].StringValue] = new ScriptTimer(Game.UpdateTime.TotalGameTime + TimeSpan.FromSeconds(action.Arguments[1].FloatValue.Value));
                    break;

                case ScriptActionType.CameraModFreezeTime:
                    break;
                case ScriptActionType.SetVisualSpeedMultiplier:
                    break;
                case ScriptActionType.CreateObject:
                    break;
                case ScriptActionType.SuspendBackgroundSounds:
                    break;
                case ScriptActionType.ResumeBackgroundSounds:
                    break;
                case ScriptActionType.CameraModSetFinalZoom:
                    break;
                case ScriptActionType.CameraModSetFinalPitch:
                    break;
                case ScriptActionType.CameraModFreezeAngle:
                    break;
                case ScriptActionType.CameraModSetFinalSpeedMultiplier:
                    break;
                case ScriptActionType.CameraModSetRollingAverage:
                    break;
                case ScriptActionType.CameraModFinalLookToward:
                    break;
                case ScriptActionType.CameraModLookToward:
                    break;
                case ScriptActionType.TeamAttackTeam:
                    break;
                case ScriptActionType.CreateReinforcementTeam:
                    break;
                case ScriptActionType.MoveCameraToSelection:
                    break;
                case ScriptActionType.TeamFollowWaypoints:
                    break;
                case ScriptActionType.TeamSetState:
                    break;
                case ScriptActionType.MoveNamedUnitTo:
                    break;
                case ScriptActionType.NamedAttackNamed:
                    break;
                case ScriptActionType.CreateNamedOnTeamAtWaypoint:
                    break;
                case ScriptActionType.CreateUnnamedOnTeamAtWaypoint:
                    break;
                case ScriptActionType.NamedApplyAttackPrioritySet:
                    break;
                case ScriptActionType.TeamApplyAttackPrioritySet:
                    break;
                case ScriptActionType.SetBaseConstructionSpeed:
                    break;
                case ScriptActionType.NamedSetAttitude:
                    break;
                case ScriptActionType.TeamSetAttitude:
                    break;
                case ScriptActionType.NamedAttackArea:
                    break;
                case ScriptActionType.NamedAttackTeam:
                    break;
                case ScriptActionType.TeamAttackArea:
                    break;
                case ScriptActionType.TeamAttackNamed:
                    break;
                case ScriptActionType.TeamLoadTransports:
                    break;
                case ScriptActionType.NamedEnterNamed:
                    break;
                case ScriptActionType.TeamEnterNamed:
                    break;
                case ScriptActionType.NamedExitAll:
                    break;
                case ScriptActionType.TeamExitAll:
                    break;
                case ScriptActionType.NamedFollowWaypoints:
                    break;
                case ScriptActionType.NamedGuard:
                    break;
                case ScriptActionType.TeamGuard:
                    break;
                case ScriptActionType.NamedHunt:
                    break;
                case ScriptActionType.TeamHunt:
                    break;
                case ScriptActionType.PlayerSellEverything:
                    break;
                case ScriptActionType.PlayerDisableBaseConstruction:
                    break;
                case ScriptActionType.PlayerDisableFactories:
                    break;
                case ScriptActionType.PlayerDisableUnitConstruction:
                    break;
                case ScriptActionType.PlayerEnableBaseConstruction:
                    break;
                case ScriptActionType.PlayerEnableFactories:
                    break;
                case ScriptActionType.PlayerEnableUnitConstruction:
                    break;
                case ScriptActionType.CameraMoveHome:
                    break;
                case ScriptActionType.BuildTeam:
                    break;
                case ScriptActionType.NamedDamage:
                    break;
                case ScriptActionType.NamedDelete:
                    break;
                case ScriptActionType.TeamDelete:
                    break;
                case ScriptActionType.NamedKill:
                    break;
                case ScriptActionType.TeamKill:
                    break;
                case ScriptActionType.PlayerKill:
                    break;
                case ScriptActionType.DisplayText:
                    break;
                case ScriptActionType.CameoFlash:
                    break;
                case ScriptActionType.NamedFlash:
                    break;
                case ScriptActionType.TeamFlash:
                    break;
                case ScriptActionType.MoviePlayFullScreen:
                    break;
                case ScriptActionType.MoviePlayRadar:
                    break;
                case ScriptActionType.SoundPlayNamed:
                    break;
                case ScriptActionType.SpeechPlay:
                    break;
                case ScriptActionType.PlayerTransferOwnershipPlayer:
                    break;
                case ScriptActionType.NamedTransferOwnershipPlayer:
                    break;
                case ScriptActionType.PlayerRelatesPlayer:
                    break;
                case ScriptActionType.RadarCreateEvent:
                    break;
                case ScriptActionType.RadarDisable:
                    break;
                case ScriptActionType.RadarEnable:
                    break;
                case ScriptActionType.MapRevealAtWaypoint:
                    break;
                case ScriptActionType.TeamAvailableForRecruitment:
                    break;
                case ScriptActionType.TeamCollectNearbyForTeam:
                    break;
                case ScriptActionType.TeamMergeIntoTeam:
                    break;
                case ScriptActionType.DisableInput:
                    break;
                case ScriptActionType.EnableInput:
                    break;
                case ScriptActionType.PlayerHunt:
                    break;
                case ScriptActionType.SoundAmbientPause:
                    break;
                case ScriptActionType.SoundAmbientResume:
                    break;
                case ScriptActionType.MusicSetTrack:
                    break;
                case ScriptActionType.SetTreeSway:
                    break;
                case ScriptActionType.DebugString:
                    break;
                case ScriptActionType.MapRevealAll:
                    break;
                case ScriptActionType.TeamGarrisonSpecificBuilding:
                    break;
                case ScriptActionType.ExitSpecificBuilding:
                    break;
                case ScriptActionType.TeamGarrisonNearestBuilding:
                    break;
                case ScriptActionType.TeamExitAllBuildings:
                    break;
                case ScriptActionType.NamedGarrisonSpecificBuilding:
                    break;
                case ScriptActionType.NamedGarrisonNearestBuilding:
                    break;
                case ScriptActionType.NamedExitBuilding:
                    break;
                case ScriptActionType.PlayerGarrisonAllBuildings:
                    break;
                case ScriptActionType.PlayerExitAllBuildings:
                    break;
                case ScriptActionType.TeamWander:
                    break;
                case ScriptActionType.TeamPanic:
                    break;

                case ScriptActionType.SetupCamera:
                    var positionWaypoint = action.Arguments[0].StringValue;
                    var zoom = action.Arguments[1].FloatValue;
                    var pitch = action.Arguments[2].FloatValue;
                    var targetWaypoint = action.Arguments[3].StringValue;
                    Scene.MainCamera.Transform.WorldPosition = Scene.Settings.Waypoints[positionWaypoint].Position;
                    Scene.MainCamera.Transform.Translate(new Vector3(0, 0, 50));
                    Scene.MainCamera.Transform.LookAt(Scene.Settings.Waypoints[targetWaypoint].Position);
                    break;

                case ScriptActionType.CameraLetterboxBegin:
                    break;
                case ScriptActionType.CameraLetterboxEnd:
                    break;
                case ScriptActionType.ZoomCamera:
                    break;
                case ScriptActionType.PitchCamera:
                    break;
                case ScriptActionType.CameraFollowNamed:
                    break;
                case ScriptActionType.OversizeTerrain:
                    break;
                case ScriptActionType.CameraFadeAdd:
                    break;
                case ScriptActionType.CameraFadeSubtract:
                    break;
                case ScriptActionType.CameraFadeSaturate:
                    break;
                case ScriptActionType.CameraFadeMultiply:
                    break;
                case ScriptActionType.CameraBwModeBegin:
                    break;
                case ScriptActionType.CameraBwModeEnd:
                    break;
                case ScriptActionType.DrawSkyboxBegin:
                    break;
                case ScriptActionType.DrawSkyboxEnd:
                    break;
                case ScriptActionType.SetAttackPriorityThing:
                    break;
                case ScriptActionType.SetAttackPriorityKindOf:
                    break;
                case ScriptActionType.SetDefaultAttackPriority:
                    break;
                case ScriptActionType.CameraStopFollow:
                    break;
                case ScriptActionType.CameraMotionBlur:
                    break;
                case ScriptActionType.CameraMotionBlurJump:
                    break;
                case ScriptActionType.CameraMotionBlurFollow:
                    break;
                case ScriptActionType.CameraMotionBlurEndFollow:
                    break;
                case ScriptActionType.FreezeTime:
                    break;
                case ScriptActionType.UnfreezeTime:
                    break;
                case ScriptActionType.ShowMilitaryCaption:
                    break;
                case ScriptActionType.CameraSetAudibleDistance:
                    break;
                case ScriptActionType.SetStoppingDistance:
                    break;
                case ScriptActionType.NamedSetStoppingDistance:
                    break;
                case ScriptActionType.SetFpsLimit:
                    break;
                case ScriptActionType.MusicSetVolume:
                    break;
                case ScriptActionType.MapShroudAtWaypoint:
                    break;
                case ScriptActionType.MapShroudAll:
                    break;
                case ScriptActionType.SetRandomTimer:
                    break;
                case ScriptActionType.SetRandomMillisecondTimer:
                    break;
                case ScriptActionType.StopTimer:
                    break;
                case ScriptActionType.RestartTimer:
                    break;
                case ScriptActionType.AddToMSecTimer:
                    break;
                case ScriptActionType.SubFromMSecTimer:
                    break;
                case ScriptActionType.TeamTransferToPlayer:
                    break;
                case ScriptActionType.PlayerSetMoney:
                    break;
                case ScriptActionType.PlayerGiveMoney:
                    break;
                case ScriptActionType.DisableSpecialPowerDisplay:
                    break;
                case ScriptActionType.EnableSpecialPowerDisplay:
                    break;
                case ScriptActionType.NamedHideSpecialPowerDisplay:
                    break;
                case ScriptActionType.NamedShowSpecialPowerDisplay:
                    break;
                case ScriptActionType.DisplayCountownTimer:
                    break;
                case ScriptActionType.HideCountdownTimer:
                    break;
                case ScriptActionType.EnableCountdownTimerDisplay:
                    break;
                case ScriptActionType.DisableCountdownTimerDisplay:
                    break;
                case ScriptActionType.NamedStopSpecialPowerCountdown:
                    break;
                case ScriptActionType.NamedStartSpecialPowerCountdown:
                    break;
                case ScriptActionType.NamedSetSpecialPowerCountdown:
                    break;
                case ScriptActionType.NamedAddSpecialPowerCountdown:
                    break;
                case ScriptActionType.NamedFireSpecialPowerAtWaypoint:
                    break;
                case ScriptActionType.NamedFireSpecialPowerAtNamed:
                    break;
                case ScriptActionType.RefreshRadar:
                    break;
                case ScriptActionType.CameraTetherNamed:
                    break;
                case ScriptActionType.CameraStopTetherNamed:
                    break;
                case ScriptActionType.CameraSetDefault:
                    break;
                case ScriptActionType.NamedStop:
                    break;
                case ScriptActionType.TeamStop:
                    break;
                case ScriptActionType.TeamStopAndDisband:
                    break;
                case ScriptActionType.RecruitTeam:
                    break;
                case ScriptActionType.TeamSetOverrideRelationToTeam:
                    break;
                case ScriptActionType.TeamRemoveOverrideRelationToTeam:
                    break;
                case ScriptActionType.TeamRemoveAllOverrideRelations:
                    break;
                case ScriptActionType.CameraLookTowardObject:
                    break;
                case ScriptActionType.NamedFireWeaponFollowingWaypointPath:
                    break;
                case ScriptActionType.TeamSetOverrideRelationToPlayer:
                    break;
                case ScriptActionType.TeamRemoveOverrideRelationToPlayer:
                    break;
                case ScriptActionType.PlayerSetOverrideRelationToTeam:
                    break;
                case ScriptActionType.PlayerRemoveOverrideRelationToTeam:
                    break;
                case ScriptActionType.UnitExecuteSequentialScript:
                    break;
                case ScriptActionType.UnitExecuteSequentialScriptLooping:
                    break;
                case ScriptActionType.UnitStopSequentialScript:
                    break;
                case ScriptActionType.TeamExecuteSequentialScript:
                    break;
                case ScriptActionType.TeamExecuteSequentialScriptLooping:
                    break;
                case ScriptActionType.TeamStopSequentialScript:
                    break;
                case ScriptActionType.UnitGuardForFrameCount:
                    break;
                case ScriptActionType.UnitIdleForFrameCount:
                    break;
                case ScriptActionType.TeamGuardForFrameCount:
                    break;
                case ScriptActionType.TeamIdleForFrameCount:
                    break;
                case ScriptActionType.WaterChangeHeight:
                    break;
                case ScriptActionType.NamedUseCommandButtonAbilityOnNamed:
                    break;
                case ScriptActionType.NamedUseCommandButtonAbilityAtWaypoint:
                    break;
                case ScriptActionType.WaterChangeHeightOverTime:
                    break;
                case ScriptActionType.MapSwitchBorder:
                    break;
                case ScriptActionType.TeamGuardPosition:
                    break;
                case ScriptActionType.TeamGuardObject:
                    break;
                case ScriptActionType.TeamGuardArea:
                    break;
                case ScriptActionType.ObjectForceSelect:
                    break;
                case ScriptActionType.CameraLookTowardWaypoint:
                    break;
                case ScriptActionType.UnitDestroyAllContained:
                    break;
                case ScriptActionType.RadarForceEnable:
                    break;
                case ScriptActionType.RadarRevertToNormal:
                    break;
                case ScriptActionType.ScreenShake:
                    break;
                case ScriptActionType.TechTreeModifyBuildabilityObject:
                    break;
                case ScriptActionType.WarehouseSetValue:
                    break;
                case ScriptActionType.ObjectCreateRadarEvent:
                    break;
                case ScriptActionType.TeamCreateRadarEvent:
                    break;
                case ScriptActionType.DisplayCinematicText:
                    break;
                case ScriptActionType.SoundDisableType:
                    break;
                case ScriptActionType.SoundEnableType:
                    break;
                case ScriptActionType.SoundEnableAll:
                    break;
                case ScriptActionType.AudioOverrideVolumeType:
                    break;
                case ScriptActionType.DebugCrashBox:
                    break;
                case ScriptActionType.AudioRestoreVolumeType:
                    break;
                case ScriptActionType.AudioRestoreVolumeAllType:
                    break;
                case ScriptActionType.InGamePopupMessage:
                    break;
                case ScriptActionType.SetCaveIndex:
                    break;
                case ScriptActionType.NamedSetHeld:
                    break;
                case ScriptActionType.NamedSetToppleDirection:
                    break;
                case ScriptActionType.UnitMoveTowardsNearestObjectType:
                    break;
                case ScriptActionType.TeamMoveTowardsNearestObjectType:
                    break;
                case ScriptActionType.MapRevealAllPerm:
                    break;
                case ScriptActionType.MapRevealAllUndoPerm:
                    break;
                case ScriptActionType.NamedSetRepulsor:
                    break;
                case ScriptActionType.TeamSetRepulsor:
                    break;
                case ScriptActionType.TeamWanderInPlace:
                    break;
                case ScriptActionType.TeamIncreasePriority:
                    break;
                case ScriptActionType.TeamDecreasePriority:
                    break;
                case ScriptActionType.DisplayCounter:
                    break;
                case ScriptActionType.HideCounter:
                    break;
                case ScriptActionType.TeamUseCommandButtonAbilityOnNamed:
                    break;
                case ScriptActionType.TeamUseCommandButtonAbilityAtWaypoint:
                    break;
                case ScriptActionType.NamedUseCommandButtonAbility:
                    break;
                case ScriptActionType.TeamUseCommandButtonAbility:
                    break;
                case ScriptActionType.NamedFlashWhite:
                    break;
                case ScriptActionType.TeamFlashWhite:
                    break;
                case ScriptActionType.SkirmishBuildBuilding:
                    break;
                case ScriptActionType.SkirmishFollowApproachPath:
                    break;
                case ScriptActionType.IdleAllUnits:
                    break;
                case ScriptActionType.ResumeSupplyTrucking:
                    break;
                case ScriptActionType.NamedCustomColor:
                    break;
                case ScriptActionType.SkirmishMoveToApproachPath:
                    break;
                case ScriptActionType.SkirmishBuildBaseDefenseFront:
                    break;
                case ScriptActionType.SkirmishFireSpecialPowerAtMostCost:
                    break;
                case ScriptActionType.Placeholder_252:
                    break;
                case ScriptActionType.PlayerRepairNamedStructure:
                    break;
                case ScriptActionType.SkirmishBuildBaseDefenseFlank:
                    break;
                case ScriptActionType.SkirmishBuildStructureFront:
                    break;
                case ScriptActionType.SkirmishBuildStructureFlank:
                    break;
                case ScriptActionType.SkirmishAttackNearestGroupWithValue:
                    break;
                case ScriptActionType.SkirmishPerformCommandButtonOnMostValuableObject:
                    break;
                case ScriptActionType.SkirmishWaitForCommandButtonAvailableAll:
                    break;
                case ScriptActionType.SkirmishWaitForCommandButtonAvailablePartial:
                    break;
                case ScriptActionType.TeamSpinForFrameCount:
                    break;
                case ScriptActionType.TeamAllUseCommandButtonOnNamed:
                    break;
                case ScriptActionType.TeamAllUseCommandButtonOnNearestEnemyUnit:
                    break;
                case ScriptActionType.TeamAllUseCommandButtonOnNearestGarrisonedBuilding:
                    break;
                case ScriptActionType.TeamAllUseCommandButtonOnNearestKindOf:
                    break;
                case ScriptActionType.TeamAllUseCommandButtonOnNearestEnemyBuilding:
                    break;
                case ScriptActionType.TeamAllUseCommandButtonOnNearestEnemyBuildingClass:
                    break;
                case ScriptActionType.TeamAllUseCommandButtonOnNearestObjectType:
                    break;
                case ScriptActionType.TeamPartialUseCommandButton:
                    break;
                case ScriptActionType.TeamCaptureNearestUnownedFactionUnit:
                    break;
                case ScriptActionType.PlayerCreateTeamFromCapturedUnits:
                    break;
                case ScriptActionType.PlayerAddSkillPoints:
                    break;
                case ScriptActionType.PlayerAddRankLevel:
                    break;
                case ScriptActionType.PlayerSetRankLevel:
                    break;
                case ScriptActionType.PlayerSetRankLevelLimit:
                    break;
                case ScriptActionType.PlayerGrantScience:
                    break;
                case ScriptActionType.PlayerPurchaseScience:
                    break;
                case ScriptActionType.TeamHuntWithCommandButton:
                    break;
                case ScriptActionType.TeamWaitForNotContainedAll:
                    break;
                case ScriptActionType.TeamWaitForNotContainedPartial:
                    break;
                case ScriptActionType.TeamFollowWaypointsExact:
                    break;
                case ScriptActionType.NamedFollowWaypointsExact:
                    break;
                case ScriptActionType.TeamSetEmoticon:
                    break;
                case ScriptActionType.NamedSetEmoticon:
                    break;
                case ScriptActionType.AiPlayerBuildSupplyCenter:
                    break;
                case ScriptActionType.AiPlayerBuildUpgrade:
                    break;
                case ScriptActionType.ObjectListAddObjectType:
                    break;
                case ScriptActionType.ObjectListRemoveObjectTye:
                    break;
                case ScriptActionType.MapRevealPermanentlyAtWaypoint:
                    break;
                case ScriptActionType.MapUndoRevealPermanentlyAtWaypoint:
                    break;
                case ScriptActionType.NamedSetStealthEnabled:
                    break;
                case ScriptActionType.TeamSetStealthEnabled:
                    break;
                case ScriptActionType.EvaSetEnabledDisabled:
                    break;
                case ScriptActionType.OptionsSetOcclusionMode:
                    break;
                case ScriptActionType.LocalDefeat:
                    break;
                case ScriptActionType.OptionsSetDrawIconUiMode:
                    break;
                case ScriptActionType.OptionsSetPartialCapMode:
                    break;
                case ScriptActionType.PlayerScienceAvailability:
                    break;
                case ScriptActionType.UnitAffectObjectPanelFlags:
                    break;
                case ScriptActionType.TeamAffectObjectPanelFlags:
                    break;
                case ScriptActionType.PlayerSelectSkillSet:
                    break;
                case ScriptActionType.ScriptingOverrideHulkLifetime:
                    break;
                case ScriptActionType.NamedFaceNamed:
                    break;
                case ScriptActionType.NamedFaceWaypoint:
                    break;
                case ScriptActionType.TeamFaceNamed:
                    break;
                case ScriptActionType.TeamFaceWaypoint:
                    break;
                case ScriptActionType.CommandBarRemoveButtonObjectType:
                    break;
                case ScriptActionType.CommandBarAddButtonObjectTypeSlot:
                    break;
                case ScriptActionType.UnitSpawnNamedLocationOrientation:
                    break;
                case ScriptActionType.PlayerAffectReceivingExperience:
                    break;
                case ScriptActionType.PlayerExcludeFromScoreScreen:
                    break;
                case ScriptActionType.TeamGuardSupplyCenter:
                    break;
                case ScriptActionType.EnableScoring:
                    break;
                case ScriptActionType.DisableScoring:
                    break;
                case ScriptActionType.SoundSetVolume:
                    break;
                case ScriptActionType.SpeechSetVolume:
                    break;
                case ScriptActionType.DisableBorderShroud:
                    break;
                case ScriptActionType.EnableBorderShroud:
                    break;
                case ScriptActionType.ObjectAllowBonuses:
                    break;
                case ScriptActionType.SoundRemoveAllDisabled:
                    break;
                case ScriptActionType.SoundRemoveType:
                    break;
                case ScriptActionType.TeamGuardInTunnelNetwork:
                    break;
                case ScriptActionType.QuickVictory:
                    break;
                case ScriptActionType.SetInfantryLightingOverride:
                    break;
                case ScriptActionType.ResetInfantryLightingOverride:
                    break;
                case ScriptActionType.TeamDeleteLiving:
                    break;
                case ScriptActionType.ResizeViewGuardBand:
                    break;
                case ScriptActionType.DeleteAllUnmanned:
                    break;
                case ScriptActionType.ChooseVictimAlwaysUsesNormal:
                    break;
                case ScriptActionType.CameraEnableSlaveMode:
                    break;
                case ScriptActionType.CameraDisableSlaveMode:
                    break;
                case ScriptActionType.CameraAddShakerAt:
                    break;
                case ScriptActionType.SetTrainHeld:
                    break;
                case ScriptActionType.NamedSetEvacLeftOrRight:
                    break;
                case ScriptActionType.EnableObjectSound:
                    break;
                case ScriptActionType.DisableObjectSound:
                    break;
                case ScriptActionType.NamedUseCommandButtonAbilityUsingWaypointPath:
                    break;
                case ScriptActionType.NamedSetUnmannedStatus:
                    break;
                case ScriptActionType.TeamSetUnmannedStatus:
                    break;
                case ScriptActionType.NamedSetBoobytrapped:
                    break;
                case ScriptActionType.TeamSetBoobytrapped:
                    break;
                case ScriptActionType.ShowWeather:
                    break;
                case ScriptActionType.AiPlayerBuildTypeNearestTeam:
                    break;
            }
        }

        private bool EvaluateConditions(ScriptOrCondition[] orConditions)
        {
            var conditionIsTrue = false;

            foreach (var orCondition in Script.OrConditions)
            {
                foreach (var andCondition in orCondition.Conditions)
                {
                    var conditionValue = EvaluateCondition(andCondition);

                    if (conditionValue)
                    {
                        conditionIsTrue = true;
                    }
                    else
                    {
                        conditionIsTrue = false;
                        break;
                    }
                }

                if (conditionIsTrue)
                {
                    break;
                }
            }

            return conditionIsTrue;
        }

        private bool EvaluateCondition(ScriptCondition condition)
        {
            switch (condition.ContentType)
            {
                case ScriptConditionType.False:
                    return false;

                case ScriptConditionType.Counter:
                    break;

                case ScriptConditionType.Flag:
                    return Scripting.Flags.TryGetValue(condition.Arguments[0].StringValue, out var flagValue)
                        && flagValue == condition.Arguments[1].UintValueAsBool;

                case ScriptConditionType.True:
                    return true;

                case ScriptConditionType.TimerExpired:
                    return Scripting.Timers.TryGetValue(condition.Arguments[0].StringValue, out var timer)
                        && timer.Expired;

                case ScriptConditionType.PlayerAllDestroyed:
                    break;
                case ScriptConditionType.PlayerAllBuildFacilitiesDestroyed:
                    break;
                case ScriptConditionType.TeamInsideAreaPartially:
                    break;
                case ScriptConditionType.TeamDestroyed:
                    break;
                case ScriptConditionType.CameraMovementFinished:
                    break;
                case ScriptConditionType.TeamHasUnits:
                    break;
                case ScriptConditionType.TeamStateIs:
                    break;
                case ScriptConditionType.TeamStateIsNot:
                    break;
                case ScriptConditionType.NamedInsideArea:
                    break;
                case ScriptConditionType.NamedOutsideArea:
                    break;
                case ScriptConditionType.NamedDestroyed:
                    break;
                case ScriptConditionType.NamedNotDestroyed:
                    break;
                case ScriptConditionType.TeamInsideAreaEntirely:
                    break;
                case ScriptConditionType.TeamOutsideAreaEntirely:
                    break;
                case ScriptConditionType.NamedAttackedByObjectType:
                    break;
                case ScriptConditionType.TeamAttackedByObjectType:
                    break;
                case ScriptConditionType.NamedAttackedByPlayer:
                    break;
                case ScriptConditionType.TeamAttackedByPlayer:
                    break;
                case ScriptConditionType.BuiltByPlayer:
                    break;
                case ScriptConditionType.NamedCreated:
                    break;
                case ScriptConditionType.TeamCreated:
                    break;
                case ScriptConditionType.PlayerHasCredits:
                    break;
                case ScriptConditionType.NamedDiscovered:
                    break;
                case ScriptConditionType.TeamDiscovered:
                    break;
                case ScriptConditionType.MissionAttempts:
                    break;
                case ScriptConditionType.NamedOwnedByPlayer:
                    break;
                case ScriptConditionType.TeamOwnedByPlayer:
                    break;
                case ScriptConditionType.PlayerHasNOrFewerBuildings:
                    break;
                case ScriptConditionType.PlayerHasPower:
                    break;
                case ScriptConditionType.NamedReachedWaypointsEnd:
                    break;
                case ScriptConditionType.TeamReachedWaypointsEnd:
                    break;
                case ScriptConditionType.NamedSelected:
                    break;
                case ScriptConditionType.NamedEnteredArea:
                    break;
                case ScriptConditionType.NamedExitedArea:
                    break;
                case ScriptConditionType.TeamEnteredAreaEntirely:
                    break;
                case ScriptConditionType.TeamEnteredAreaPartially:
                    break;
                case ScriptConditionType.TeamExitedAreaEntirey:
                    break;
                case ScriptConditionType.TeamExitedAreaPartially:
                    break;
                case ScriptConditionType.MultiPlayerAlliedVictory:
                    break;
                case ScriptConditionType.MultiPlayerAlliedDefeat:
                    break;
                case ScriptConditionType.MultiPlayerDefeat:
                    break;
                case ScriptConditionType.PlayerHasNoPower:
                    break;
                case ScriptConditionType.HasFinishedVideo:
                    break;
                case ScriptConditionType.HasFinishedSpeech:
                    break;
                case ScriptConditionType.HasFinishedAudio:
                    break;
                case ScriptConditionType.BuildingEnteredByPlayer:
                    break;
                case ScriptConditionType.EnemySighted:
                    break;
                case ScriptConditionType.UnitHealth:
                    break;
                case ScriptConditionType.BridgeRepaired:
                    break;
                case ScriptConditionType.BridgeBroken:
                    break;
                case ScriptConditionType.NamedDying:
                    break;
                case ScriptConditionType.NamedTotallyDead:
                    break;
                case ScriptConditionType.PlayerHasObjectComparison:
                    break;
                case ScriptConditionType.Placeholder_58:
                    break;
                case ScriptConditionType.Placeholder_59:
                    break;
                case ScriptConditionType.PlayerTriggeredSpecialPower:
                    break;
                case ScriptConditionType.PlayerCompletedSpecialPower:
                    break;
                case ScriptConditionType.PlayerMidwaySpecialPower:
                    break;
                case ScriptConditionType.PlayerTriggeredSpecialPowerFromNamed:
                    break;
                case ScriptConditionType.PlayerCompletedSpecialPowerFromNamed:
                    break;
                case ScriptConditionType.PlayerMidwaySpecialPowerFromNamed:
                    break;
                case ScriptConditionType.Placeholder_66:
                    break;
                case ScriptConditionType.Placeholder_67:
                    break;
                case ScriptConditionType.PlayerBuiltUpgrade:
                    break;
                case ScriptConditionType.PlayerBuiltUpgradeFromNamed:
                    break;
                case ScriptConditionType.PlayerDestroyedNBuildingsPlayer:
                    break;
                case ScriptConditionType.Placeholder_71:
                    break;
                case ScriptConditionType.Placeholder_72:
                    break;
                case ScriptConditionType.PlayerHasComparisonUnitTypeInTriggerArea:
                    break;
                case ScriptConditionType.PlayerHasComparisonUnitKindInTriggerArea:
                    break;
                case ScriptConditionType.UnitEmptied:
                    break;
                case ScriptConditionType.TypeSighted:
                    break;
                case ScriptConditionType.NamedBuildingIsEmpty:
                    break;
                case ScriptConditionType.PlayerHasNOrFewerFactionBuildings:
                    break;
                case ScriptConditionType.UnitHasObjectStatus:
                    break;
                case ScriptConditionType.TeamAllHasObjectStatus:
                    break;
                case ScriptConditionType.TeamObjectStatusPartial:
                    break;
                case ScriptConditionType.PlayerPowerComparePercent:
                    break;
                case ScriptConditionType.PlayerExcessPowerCompareValue:
                    break;
                case ScriptConditionType.SkirmishSpecialPowerReady:
                    break;
                case ScriptConditionType.SkirmishValueInArea:
                    break;
                case ScriptConditionType.SkirmishPlayerFaction:
                    break;
                case ScriptConditionType.SkirmishSuppliesValueWithinDistance:
                    break;
                case ScriptConditionType.SkirmishTechBuildingWithinDistance:
                    break;
                case ScriptConditionType.SkirmishCommandButtonReadyAll:
                    break;
                case ScriptConditionType.SkirmishCommandButtonReadyPartial:
                    break;
                case ScriptConditionType.SkirmishUnownedFactionUnitExists:
                    break;
                case ScriptConditionType.SkirmishPlayerHasPrerequisiteToBuild:
                    break;
                case ScriptConditionType.SkirmishPlayerHasComparisonGarrisoned:
                    break;
                case ScriptConditionType.SkirmishPlayerHasComparisonCapturedUnits:
                    break;
                case ScriptConditionType.SkirmishNamedAreaExist:
                    break;
                case ScriptConditionType.SkirmishPlayerHasUnitsInArea:
                    break;
                case ScriptConditionType.SkirmishPlayerHasBeenAttackedByPlayer:
                    break;
                case ScriptConditionType.SkirmishPlayerIsOutsideArea:
                    break;
                case ScriptConditionType.SkirmishPlayerHasDiscoveredPlayer:
                    break;
                case ScriptConditionType.PlayerAcquiredScience:
                    break;
                case ScriptConditionType.PlayerHasSciencePurchasePoints:
                    break;
                case ScriptConditionType.PlayerCanPurchaseScience:
                    break;
                case ScriptConditionType.MusicTrackHasCompleted:
                    break;
                case ScriptConditionType.PlayerLostObjectType:
                    break;
                case ScriptConditionType.SupplySourceSafe:
                    break;
                case ScriptConditionType.SupplySourceAttacked:
                    break;
                case ScriptConditionType.StartPositionIs:
                    break;
                case ScriptConditionType.NamedHasFreeConstructionSlots:
                    break;
                default:
                    break;
            }

            // TODO: Remove this.
            return false;
        }
    }
}
