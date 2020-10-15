using System;
using System.Collections.Generic;
using OpenSage.Audio;
using OpenSage.Content;
using OpenSage.Eva;
using OpenSage.FX;
using OpenSage.Graphics;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Gui.ControlBar;
using OpenSage.Gui.InGame;
using OpenSage.Gui.Wnd.Transitions;
using OpenSage.Input;
using OpenSage.Input.Cursors;
using OpenSage.LivingWorld;
using OpenSage.LivingWorld.AutoResolve;
using OpenSage.Lod;
using OpenSage.Logic;
using OpenSage.Logic.AI;
using OpenSage.Logic.Object;
using OpenSage.Logic.Pathfinding;
using OpenSage.Terrain;
using OpenSage.Terrain.Roads;

namespace OpenSage.Data.Ini
{
    partial class IniParser
    {
        private static readonly Dictionary<string, Action<IniParser, AssetStore>> BlockParsers = new Dictionary<string, Action<IniParser, AssetStore>>
        {
            { "AIBase", (parser, assetStore) => assetStore.AIBases.Add(AIBase.Parse(parser)) },
            { "AIData", (parser, assetStore) => AIData.Parse(parser, assetStore.AIData.Current) },
            { "AIDozerAssignment", (parser, assetStore) => assetStore.AIDozerAssignments.Add(AIDozerAssignment.Parse(parser)) },
            { "AmbientStream", (parser, assetStore) => assetStore.AmbientStreams.Add(AmbientStream.Parse(parser)) },
            { "Animation", (parser, assetStore) => assetStore.Animations.Add(Animation.Parse(parser)) },
            { "AnimationSoundClientBehaviorGlobalSetting", (parser, assetStore) => AnimationSoundClientBehaviorGlobalSetting.Parse(parser, assetStore.AnimationSoundClientBehaviorGlobalSetting.Current) },
            { "AptButtonTooltipMap", (parser, assetStore) => AptButtonTooltipMap.Parse(parser, assetStore.AptButtonTooltipMap.Current) },
            { "Armor", (parser, assetStore) => assetStore.ArmorTemplates.Add(ArmorTemplate.Parse(parser)) },
            { "ArmyDefinition", (parser, assetStore) => assetStore.ArmyDefinitions.Add(ArmyDefinition.Parse(parser)) },
            { "ArmySummaryDescription", (parser, assetStore) => ArmySummaryDescription.Parse(parser, assetStore.ArmySummaryDescription.Current) },
            { "AudioEvent", (parser, assetStore) => assetStore.AudioEvents.Add(AudioEvent.Parse(parser)) },
            { "AudioLOD", (parser, assetStore) => assetStore.AudioLods.Add(AudioLod.Parse(parser)) },
            { "AudioLowMHz", (parser, assetStore) => parser.ParseInteger() },
            { "AudioSettings", (parser, assetStore) => AudioSettings.Parse(parser, assetStore.AudioSettings.Current) },
            { "AutoResolveArmor", (parser, assetStore) => assetStore.AutoResolveArmors.Add(AutoResolveArmor.Parse(parser)) },
            { "AutoResolveBody", (parser, assetStore) => assetStore.AutoResolveBodies.Add(AutoResolveBody.Parse(parser)) },
            { "AutoResolveCombatChain", (parser, assetStore) => assetStore.AutoResolveCombatChains.Add(AutoResolveCombatChain.Parse(parser)) },
            { "AutoResolveHandicapLevel", (parser, assetStore) => assetStore.AutoResolveHandicapLevels.Add(AutoResolveHandicapLevel.Parse(parser)) },
            { "AutoResolveReinforcementSchedule", (parser, assetStore) => assetStore.AutoResolveReinforcementSchedules.Add(AutoResolveReinforcementSchedule.Parse(parser)) },
            { "AutoResolveLeadership", (parser, assetStore) => assetStore.AutoResolveLeaderships.Add(AutoResolveLeadership.Parse(parser)) },
            { "AutoResolveWeapon", (parser, assetStore) => assetStore.AutoResolveWeapons.Add(AutoResolveWeapon.Parse(parser)) },
            { "AwardSystem", (parser, assetStore) => AwardSystem.Parse(parser, assetStore.AwardSystem.Current) },
            { "BannerType", (parser, assetStore) => assetStore.BannerTypes.Add(BannerType.Parse(parser)) },
            { "BannerUI", (parser, assetStore) => BannerUI.Parse(parser, assetStore.BannerUI.Current) },
            { "BenchProfile", (parser, assetStore) => assetStore.BenchProfiles.Add(BenchProfile.Parse(parser)) },
            { "Bridge", (parser, assetStore) => assetStore.BridgeTemplates.Add(BridgeTemplate.Parse(parser)) },
            { "Campaign", (parser, assetStore) => assetStore.CampaignTemplates.Add(CampaignTemplate.Parse(parser)) },
            { "ChallengeGenerals", (parser, assetStore) => ChallengeGenerals.Parse(parser, assetStore.ChallengeGenerals.Current) },
            { "ChildObject", (parser, assetStore) => assetStore.ObjectDefinitions.Add(ObjectDefinition.ParseChildObject(parser)) },
            { "CloudEffect", (parser, assetStore) => assetStore.Environment.Current.CloudEffect = CloudEffect.Parse(parser) },
            { "CommandButton", (parser, assetStore) => assetStore.CommandButtons.Add(CommandButton.Parse(parser)) },
            { "CommandMap", (parser, assetStore) => assetStore.CommandMaps.Add(CommandMap.Parse(parser)) },
            { "CommandSet", (parser, assetStore) => assetStore.CommandSets.Add(CommandSet.Parse(parser)) },
            { "ControlBarResizer", (parser, assetStore) => assetStore.ControlBarResizers.Add(ControlBarResizer.Parse(parser)) },
            { "ControlBarScheme", (parser, assetStore) => assetStore.ControlBarSchemes.Add(ControlBarScheme.Parse(parser)) },
            { "CrateData", (parser, assetStore) => assetStore.CrateDatas.Add(CrateData.Parse(parser)) },
            { "CreateAHeroSystem", (parser, assetStore) => CreateAHeroSystem.Parse(parser, assetStore.CreateAHeroSystem.Current) },
            { "Credits", (parser, assetStore) => Credits.Parse(parser, assetStore.Credits.Current) },
            { "CrowdResponse", (parser, assetStore) => assetStore.CrowdResponses.Add(CrowdResponse.Parse(parser)) },
            { "DamageFX", (parser, assetStore) => assetStore.DamageFXs.Add(DamageFX.Parse(parser)) },
            { "DialogEvent", (parser, assetStore) => assetStore.DialogEvents.Add(DialogEvent.Parse(parser)) },
            { "DrawGroupInfo", (parser, assetStore) => DrawGroupInfo.Parse(parser, assetStore.DrawGroupInfo.Current) },
            { "DynamicGameLOD", (parser, assetStore) => assetStore.DynamicGameLods.Add(DynamicGameLod.Parse(parser)) },
            { "EmotionNugget", (parser, assetStore) => assetStore.EmotionNuggets.Add(EmotionNugget.Parse(parser)) },
            { "EvaEvent", (parser, assetStore) => assetStore.EvaEvents.Add(EvaEvent.Parse(parser)) },
            { "EvaEventForwardReference", (parser, assetStore) => EvaEvent.Parse(parser) },
            { "ExperienceLevel", (parser, assetStore) => assetStore.ExperienceLevels.Add(ExperienceLevel.Parse(parser)) },
            { "ExperienceScalarTable", (parser, assetStore) => assetStore.ExperienceScalarTables.Add(ExperienceScalarTable.Parse(parser)) },
            { "FactionVictoryData", (parser, assetStore) => assetStore.FactionVictoryDatas.Add(FactionVictoryData.Parse(parser)) },
            { "Fire", (parser, assetStore) => Fire.Parse(parser, assetStore.Fire.Current) },
            { "FireLogicSystem", (parser, assetStore) => FireLogicSystem.Parse(parser, assetStore.FireLogicSystem.Current) },
            { "FireEffect", (parser, assetStore) => assetStore.Environment.Current.FireEffect = RingEffect.Parse(parser) },
            { "FontDefaultSettings", (parser, assetStore) => assetStore.FontDefaultSettings.Add(FontDefaultSetting.Parse(parser)) },
            { "FontSubstitution", (parser, assetStore) => assetStore.FontSubstitutions.Add(FontSubstitution.Parse(parser)) },
            { "FormationAssistant", (parser, assetStore) => FormationAssistant.Parse(parser, assetStore.FormationAssistant.Current) },
            { "FXList", (parser, assetStore) => assetStore.FXLists.Add(FXList.Parse(parser)) },
            { "FXParticleSystem", (parser, assetStore) => assetStore.FXParticleSystemTemplates.Add(FXParticleSystemTemplate.Parse(parser)) },
            { "GameData", (parser, assetStore) => GameData.Parse(parser, assetStore.GameData.Current) },
            { "GlowEffect", (parser, assetStore) => assetStore.Environment.Current.GlowEffect = GlowEffect.Parse(parser) },
            { "HeaderTemplate", (parser, assetStore) => assetStore.HeaderTemplates.Add(HeaderTemplate.Parse(parser)) },
            { "HouseColor", (parser, assetStore) => assetStore.HouseColors.Add(HouseColor.Parse(parser)) },
            { "InGameNotificationBox", (parser, assetStore) => InGameNotificationBox.Parse(parser, assetStore.InGameNotificationBox.Current) },
            { "InGameUI", (parser, assetStore) => InGameUI.Parse(parser, assetStore.InGameUI.Current) },
            { "Language", (parser, assetStore) => Language.Parse(parser, assetStore.Language.Current) },
            { "LargeGroupAudioMap", (parser, assetStore) => assetStore.LargeGroupAudioMaps.Add(LargeGroupAudioMap.Parse(parser)) },
            { "LinearCampaign", (parser, assetStore) => LinearCampaign.Parse(parser, assetStore.LinearCampaign.Current) },
            { "LivingWorldAITemplate", (parser, assetStore) => assetStore.LivingWorldAITemplates.Add(LivingWorldAITemplate.Parse(parser)) },
            { "LivingWorldAnimObject", (parser, assetStore) => assetStore.LivingWorldAnimObjects.Add(LivingWorldAnimObject.Parse(parser)) },
            { "LivingWorldArmyIcon", (parser, assetStore) => assetStore.LivingWorldArmyIcons.Add(LivingWorldArmyIcon.Parse(parser)) },
            { "LivingWorldAutoResolveResourceBonus", (parser, assetStore) => LivingWorldAutoResolveResourceBonus.Parse(parser, assetStore.LivingWorldAutoResolveResourceBonus.Current) },
            { "LivingWorldAutoResolveSciencePurchasePointBonus", (parser, assetStore) => LivingWorldAutoResolveSciencePurchasePointBonus.Parse(parser, assetStore.LivingWorldAutoResolveSciencePurchasePointBonus.Current) },
            { "LivingWorldBuilding", (parser, assetStore) => assetStore.LivingWorldBuildings.Add(LivingWorldBuilding.Parse(parser)) },
            { "LivingWorldBuildPlotIcon", (parser, assetStore) => assetStore.LivingWorldBuildPlotIcons.Add(LivingWorldBuildPlotIcon.Parse(parser)) },
            { "LivingWorldBuildingIcon", (parser, assetStore) => assetStore.LivingWorldBuildingIcons.Add(LivingWorldBuildingIcon.Parse(parser)) },
            { "LargeGroupAudioUnusedKnownKeys", (parser, assetStore) => LargeGroupAudioUnusedKnownKeys.Parse(parser, assetStore.LargeGroupAudioUnusedKnownKeys.Current) },
            { "LivingWorldCampaign", (parser, assetStore) => assetStore.LivingWorldCampaigns.Add(LivingWorldCampaign.Parse(parser)) },
            { "LivingWorldMapInfo", (parser, assetStore) => LivingWorldMapInfo.Parse(parser, assetStore.LivingWorldMapInfo.Current) },
            { "LivingWorldObject", (parser, assetStore) => assetStore.LivingWorldObjects.Add(LivingWorldObject.Parse(parser)) },
            { "LivingWorldPlayerArmy", (parser, assetStore) => assetStore.LivingWorldPlayerArmies.Add(LivingWorldPlayerArmy.Parse(parser)) },
            { "LivingWorldPlayerTemplate", (parser, assetStore) => assetStore.LivingWorldPlayerTemplates.Add(LivingWorldPlayerTemplate.Parse(parser)) },
            { "LivingWorldRegionCampaign", (parser, assetStore) => assetStore.LivingWorldRegionCampaigns.Add(LivingWorldRegionCampaign.Parse(parser)) },
            { "LivingWorldRegionEffects", (parser, assetStore) => assetStore.LivingWorldRegionEffects.Add(LivingWorldRegionEffects.Parse(parser)) },
            { "LivingWorldSound", (parser, assetStore) => assetStore.LivingWorldSounds.Add(LivingWorldSound.Parse(parser)) },
            { "LoadSubsystem", (parser, assetStore) => assetStore.Subsystems.Add(LoadSubsystem.Parse(parser)) },
            { "Locomotor", (parser, assetStore) => assetStore.LocomotorTemplates.Add(LocomotorTemplate.Parse(parser)) },
            { "LODPreset", (parser, assetStore) => assetStore.LodPresets.Add(LodPreset.Parse(parser)) },
            { "MapCache", (parser, assetStore) => assetStore.MapCaches.Add(MapCache.Parse(parser)) },
            { "MappedImage", (parser, assetStore) => assetStore.MappedImages.Add(MappedImage.Parse(parser)) },
            { "MeshNameMatches", (parser, assetStore) => assetStore.MeshNameMatches.Add(MeshNameMatches.Parse(parser)) },
            { "MiscAudio", (parser, assetStore) => MiscAudio.Parse(parser, assetStore.MiscAudio.Current) },
            { "MiscEvaData", (parser, assetStore) => MiscEvaData.Parse(parser, assetStore.MiscEvaData.Current) },
            { "MissionObjectiveList", (parser, assetStore) => MissionObjectiveList.Parse(parser, assetStore.MissionObjectiveList.Current) },
            { "ModifierList", (parser, assetStore) => assetStore.ModifierLists.Add(ModifierList.Parse(parser)) },
            { "Mouse", (parser, assetStore) => MouseData.Parse(parser, assetStore.MouseData.Current) },
            { "MouseCursor", (parser, assetStore) => assetStore.MouseCursors.Add(MouseCursor.Parse(parser)) },
            { "MultiplayerColor", (parser, assetStore) => assetStore.MultiplayerColors.Add(MultiplayerColor.Parse(parser)) },
            { "MultiplayerSettings", (parser, assetStore) => MultiplayerSettings.Parse(parser, assetStore.MultiplayerSettings.Current) },
            { "MultiplayerStartingMoneyChoice", (parser, assetStore) => assetStore.MultiplayerStartingMoneyChoices.Add(MultiplayerStartingMoneyChoice.Parse(parser)) },
            { "Multisound", (parser, assetStore) => assetStore.Multisounds.Add(Multisound.Parse(parser)) },
            { "MusicTrack", (parser, assetStore) => assetStore.MusicTracks.Add(MusicTrack.Parse(parser)) },
            { "NewEvaEvent", (parser, assetStore) => assetStore.EvaEvents.Add(EvaEvent.Parse(parser)) },
            { "Object", (parser, assetStore) => assetStore.ObjectDefinitions.Add(ObjectDefinition.Parse(parser)) },
            { "ObjectReskin", (parser, assetStore) => assetStore.ObjectDefinitions.Add(ObjectDefinition.ParseReskin(parser)) },
            { "ObjectCreationList", (parser, assetStore) => assetStore.ObjectCreationLists.Add(ObjectCreationList.Parse(parser)) },
            { "OnlineChatColors", (parser, assetStore) => OnlineChatColors.Parse(parser, assetStore.OnlineChatColors.Current) },
            { "ParticleSystem", (parser, assetStore) => assetStore.FXParticleSystemTemplates.Add(ParticleSystemTemplate.Parse(parser).ToFXParticleSystemTemplate()) },
            { "Pathfinder", (parser, assetStore) => Pathfinder.Parse(parser, assetStore.Pathfinder.Current) },
            { "PlayerAIType", (parser, assetStore) => assetStore.PlayerAITypes.Add(PlayerAIType.Parse(parser)) },
            { "PlayerTemplate", (parser, assetStore) => assetStore.PlayerTemplates.Add(PlayerTemplate.Parse(parser)) },
            { "PredefinedEvaEvent", (parser, assetStore) => assetStore.EvaEvents.Add(EvaEvent.Parse(parser)) },
            { "Rank", (parser, assetStore) => assetStore.Ranks.Add(Rank.Parse(parser)) },
            { "RegionCampain", (parser, assetStore) => assetStore.RegionCampaigns.Add(RegionCampaign.Parse(parser)) },
            { "RingEffect", (parser, assetStore) => assetStore.Environment.Current.RingEffect = RingEffect.Parse(parser) },
            { "Road", (parser, assetStore) => assetStore.RoadTemplates.Add(RoadTemplate.Parse(parser)) },
            { "ReallyLowMHz", (parser, assetStore) => parser.ParseInteger() },
            { "Science", (parser, assetStore) => assetStore.Sciences.Add(Science.Parse(parser)) },
            { "ScoredKillEvaAnnouncer", (parser, assetStore) => assetStore.ScoredKillEvaAnnouncers.Add(ScoredKillEvaAnnouncer.Parse(parser)) },
            { "ShadowMap", (parser, assetStore) => assetStore.Environment.Current.ShadowMap = ShadowMap.Parse(parser) },
            { "ShellMenuScheme", (parser, assetStore) => assetStore.ShellMenuSchemes.Add(ShellMenuScheme.Parse(parser)) },
            { "SkirmishAIData", (parser, assetStore) => assetStore.SkirmishAIDatas.Add(SkirmishAIData.Parse(parser)) },
            { "SkyboxTextureSet", (parser, assetStore) => assetStore.SkyboxTextureSets.Add(SkyboxTextureSet.Parse(parser)) },
            { "SpecialPower", (parser, assetStore) => assetStore.SpecialPowers.Add(SpecialPower.Parse(parser)) },
            { "StanceTemplate", (parser, assetStore) => assetStore.StanceTemplates.Add(StanceTemplate.Parse(parser)) },
            { "StreamedSound", (parser, assetStore) => assetStore.StreamedSounds.Add(StreamedSound.Parse(parser)) },
            { "StaticGameLOD", (parser, assetStore) => assetStore.StaticGameLods.Add(StaticGameLod.Parse(parser)) },
            { "StrategicHUD", (parser, assetStore) => StrategicHud.Parse(parser, assetStore.StrategicHud.Current) },
            { "Terrain", (parser, assetStore) => assetStore.TerrainTextures.Add(TerrainTexture.Parse(parser)) },
            { "Upgrade", (parser, assetStore) => assetStore.Upgrades.Add(UpgradeTemplate.Parse(parser)) },
            { "VictorySystemData", (parser, assetStore) => assetStore.VictorySystemDatas.Add(VictorySystemData.Parse(parser)) },
            { "Video", (parser, assetStore) => assetStore.Videos.Add(Video.Parse(parser)) },
            { "WaterSet", (parser, assetStore) => assetStore.WaterSets.Add(WaterSet.Parse(parser)) },
            { "WaterTextureList", (parser, assetStore) => assetStore.WaterTextureLists.Add(WaterTextureList.Parse(parser)) },
            { "WaterTransparency", (parser, assetStore) => WaterTransparency.Parse(parser, assetStore.WaterTransparency.Current) },
            { "Weapon", (parser, assetStore) => assetStore.WeaponTemplates.Add(WeaponTemplate.Parse(parser)) },
            { "Weather", (parser, assetStore) => Weather.Parse(parser, assetStore.Weather.Current) },
            { "WeatherData", (parser, assetStore) => assetStore.WeatherDatas.Add(WeatherData.Parse(parser)) },
            { "WindowTransition", (parser, assetStore) => assetStore.WindowTransitions.Add(WindowTransition.Parse(parser)) },
        };
    }
}
