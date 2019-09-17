using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class IniDataContext
    {
        [AddedIn(SageGame.Bfme2)]
        public List<AIBase> AIBases { get; } = new List<AIBase>();

        public AIData AIData { get; internal set; }

        [AddedIn(SageGame.Bfme2)]
        public List<AIDozerAssignment> AIDozerAssignments { get; } = new List<AIDozerAssignment>();

        public List<Animation> Animations { get; } = new List<Animation>();
        public AnimationSoundClientBehaviorGlobalSetting AnimationSoundClientBehaviorGlobalSetting { get; internal set; }

        [AddedIn(SageGame.Bfme2)]
        public AptButtonTooltipMap AptButtonTooltipMap { get; internal set;  }

        public List<Armor> Armors { get; } = new List<Armor>();

        [AddedIn(SageGame.Bfme2)]
        public List<ArmyDefinition> ArmyDefinitions { get; } = new List<ArmyDefinition>();

        public int AudioLowMHz { get; internal set; }

        [AddedIn(SageGame.Bfme2)]
        public List<AutoResolveArmor> AutoResolveArmors { get; } = new List<AutoResolveArmor>();

        [AddedIn(SageGame.Bfme2)]
        public List<AutoResolveBody> AutoResolveBodies { get; } = new List<AutoResolveBody>();

        [AddedIn(SageGame.Bfme2)]
        public List<AutoResolveCombatChain> AutoResolveCombatChains { get; } = new List<AutoResolveCombatChain>();

        [AddedIn(SageGame.Bfme2)]
        public List<AutoResolveHandicapLevel> AutoResolveHandicapLevels { get; } = new List<AutoResolveHandicapLevel>();

        [AddedIn(SageGame.Bfme2)]
        public List<AutoResolveLeadership> AutoResolveLeaderships { get; } = new List<AutoResolveLeadership>();

        [AddedIn(SageGame.Bfme2)]
        public List<AutoResolveReinforcementSchedule> AutoResolveReinforcementSchedules { get; } = new List<AutoResolveReinforcementSchedule>();

        [AddedIn(SageGame.Bfme2)]
        public List<AutoResolveWeapon> AutoResolveWeapons { get; } = new List<AutoResolveWeapon>();

        [AddedIn(SageGame.Bfme2)]
        public AwardSystem AwardSystem { get; internal set; }

        public List<BannerType> BannerTypes { get; } = new List<BannerType>();
        public BannerUI BannerUI { get; internal set; }
        public List<BenchProfile> BenchProfiles { get; } = new List<BenchProfile>();        
        public List<Campaign> Campaigns { get; } = new List<Campaign>();
        public ChallengeGenerals ChallengeGenerals { get; internal set; }
        public List<CommandButton> CommandButtons { get; } = new List<CommandButton>();
        public List<CommandMap> CommandMaps { get; } = new List<CommandMap>();
        public List<CommandSet> CommandSets { get; } = new List<CommandSet>();
        public List<ControlBarResizer> ControlBarResizers { get; } = new List<ControlBarResizer>();
        public ControlBarSchemeCollection ControlBarSchemes { get; } = new ControlBarSchemeCollection();
        public List<CrateData> CrateDatas { get; } = new List<CrateData>();

        [AddedIn(SageGame.Bfme2)]
        public CreateAHeroSystem CreateAHeroSystem { get; internal set; }

        public Credits Credits { get; internal set; }

        [AddedIn(SageGame.Bfme2)]
        public List<CrowdResponse> CrowdResponses { get; } = new List<CrowdResponse>();

        public List<DamageFX> DamageFXs { get; } = new List<DamageFX>();
        public DrawGroupInfo DrawGroupInfo { get; internal set; }
        public List<DynamicGameLod> DynamicGameLods { get; } = new List<DynamicGameLod>();
        public List<EmotionNugget> EmotionNuggets { get; } = new List<EmotionNugget>();
        public Environment Environment { get; } = new Environment();
        public Dictionary<string, EvaEvent> EvaEvents { get; } = new Dictionary<string, EvaEvent>();
        public List<ExperienceLevel> ExperienceLevels { get; } = new List<ExperienceLevel>();
        public List<ExperienceScalarTable> ExperienceScalarTables { get; } = new List<ExperienceScalarTable>();
        public List<FactionVictoryData> FactionVictoryDatas { get; } = new List<FactionVictoryData>();

        [AddedIn(SageGame.Bfme2)]
        public FireLogicSystem FireLogicSystem { get; internal set; }

        public List<FontDefaultSetting> FontDefaultSettings { get; } = new List<FontDefaultSetting>();
        public List<FontSubstitution> FontSubstitutions { get; } = new List<FontSubstitution>();

        [AddedIn(SageGame.Bfme2)]
        public FormationAssistant FormationAssistant { get; internal set; }

        public List<FXList> FXLists { get; } = new List<FXList>();
        public GameData GameData { get; internal set; }
        public List<HeaderTemplate> HeaderTemplates { get; } = new List<HeaderTemplate>();
        public List<HouseColor> HouseColors { get; } = new List<HouseColor>();

        [AddedIn(SageGame.Bfme2)]
        public InGameNotificationBox InGameNotificationBox { get; internal set; }

        public InGameUI InGameUI { get; internal set; }
        public Language Language { get; internal set; }
        public List<LargeGroupAudioMap> LargeGroupAudioMaps { get; } = new List<LargeGroupAudioMap>();

        [AddedIn(SageGame.Bfme2)]
        public LinearCampaign LinearCampaign { get; internal set; }

        [AddedIn(SageGame.Bfme2)]
        public LivingWorldAiTemplate LivingWorldAiTemplate { get; internal set; }

        public LargeGroupAudioUnusedKnownKeys LargeGroupAudioUnusedKnownKeys { get; internal set; }
        public List<LivingWorldCampaign> LivingWorldCampaigns { get; } = new List<LivingWorldCampaign>();

        [AddedIn(SageGame.Bfme)]
        public LivingWorldMapInfo LivingWorldMapInfo { get; set; }

        [AddedIn(SageGame.Bfme)]
        public List<LivingWorldObject> LivingWorldObjects { get; } = new List<LivingWorldObject>();
        public List<LivingWorldPlayerArmy> LivingWorldPlayerArmies { get; } = new List<LivingWorldPlayerArmy>();

        [AddedIn(SageGame.Bfme2)]
        public List<LivingWorldPlayerTemplate> LivingWorldPlayerTemplates { get; } = new List<LivingWorldPlayerTemplate>();

        [AddedIn(SageGame.Bfme)]
        public List<LivingWorldArmyIcon> LivingWorldArmyIcons { get; } = new List<LivingWorldArmyIcon>();

        [AddedIn(SageGame.Bfme2)]
        public LivingWorldAutoResolveResourceBonus LivingWorldAutoResolveResourceBonus { get; internal set; }

        [AddedIn(SageGame.Bfme2)]
        public LivingWorldAutoResolveSciencePurchasePointBonus LivingWorldAutoResolveSciencePurchasePointBonus { get; internal set; }

        [AddedIn(SageGame.Bfme2)]
        public List<LivingWorldBuilding> LivingWorldBuildings { get; } = new List<LivingWorldBuilding>();

        [AddedIn(SageGame.Bfme2)]
        public List<LivingWorldBuildingIcon> LivingWorldBuildingIcons { get; } = new List<LivingWorldBuildingIcon>();

        [AddedIn(SageGame.Bfme2)]
        public List<LivingWorldBuildPlotIcon> LivingWorldBuildPlotIcons { get; } = new List<LivingWorldBuildPlotIcon>();

        [AddedIn(SageGame.Bfme)]
        public List<LivingWorldAnimObject> LivingWorldAnimObjects { get; } = new List<LivingWorldAnimObject>();

        [AddedIn(SageGame.Bfme)]
        public List<LivingWorldSound> LivingWorldSounds { get; } = new List<LivingWorldSound>();
        public List<LivingWorldRegionCampaign> LivingWorldRegionCampaigns { get; } = new List<LivingWorldRegionCampaign>();

        [AddedIn(SageGame.Bfme2)]
        public LivingWorldRegionEffects LivingWorldRegionEffects { get; internal set; }
        public List<LodPreset> LodPresets { get; } = new List<LodPreset>();
        public List<MapCache> MapCaches { get; } = new List<MapCache>();

        [AddedIn(SageGame.Bfme2)]
        public List<MeshNameMatches> MeshNameMatches { get; } = new List<MeshNameMatches>();

        public MiscAudio MiscAudio { get; internal set; }
        public MiscEvaData MiscEvaData { get; internal set; }

        [AddedIn(SageGame.Bfme2)]
        public MissionObjectiveList MissionObjectiveList { get; internal set; }

        public List<ModifierList> ModifierLists { get; } = new List<ModifierList>();
        public List<MouseCursor> MouseCursors { get; } = new List<MouseCursor>();
        public MouseData MouseData { get; internal set; }
        public List<MultiplayerColor> MultiplayerColors { get; } = new List<MultiplayerColor>();
        public MultiplayerSettings MultiplayerSettings { get; internal set; }
        public List<MultiplayerStartingMoneyChoice> MultiplayerStartingMoneyChoices { get; } = new List<MultiplayerStartingMoneyChoice>();
        public List<Multisound> Multisounds { get; } = new List<Multisound>();
        public List<ObjectCreationList> ObjectCreationLists { get; } = new List<ObjectCreationList>();
        public OnlineChatColors OnlineChatColors { get; internal set; }

        [AddedIn(SageGame.Bfme)]
        public Pathfinder Pathfinder { get; set; }

        [AddedIn(SageGame.Bfme)]
        public List<PlayerAIType> PlayerAITypes { get; } = new List<PlayerAIType>();

        public List<Rank> Ranks { get; } = new List<Rank>();

        [AddedIn(SageGame.Bfme)]
        public RegionCampain RegionCampaign{ get; set; }
        public int ReallyLowMHz { get; internal set; }
        public List<Science> Sciences { get; } = new List<Science>();

        [AddedIn(SageGame.Bfme2)]
        public List<ScoredKillEvaAnnouncer> ScoredKillEvaAnnouncers { get; } = new List<ScoredKillEvaAnnouncer>();

        public List<ShellMenuScheme> ShellMenuSchemes { get; } = new List<ShellMenuScheme>();

        [AddedIn(SageGame.Bfme2)]
        public SkirmishAIData SkirmishAIData { get; internal set; }

        public List<SkyboxTextureSet> SkyboxTextureSets { get; } = new List<SkyboxTextureSet>();
        public List<SpecialPower> SpecialPowers { get; } = new List<SpecialPower>();

        [AddedIn(SageGame.Bfme2)]
        public List<StanceTemplate> StanceTemplates { get; } = new List<StanceTemplate>();

        public List<StaticGameLod> StaticGameLods { get; } = new List<StaticGameLod>();

        [AddedIn(SageGame.Bfme2)]
        public StrategicHud StrategicHud { get; internal set; }

        public List<LoadSubsystem> Subsystems { get; } = new List<LoadSubsystem>();
        public List<Upgrade> Upgrades { get; } = new List<Upgrade>();
        public List<VictorySystemData> VictorySystemDatas { get; } = new List<VictorySystemData>();
        public List<Video> Videos { get; } = new List<Video>();
        public List<WaterTextureList> WaterTextureLists { get; } = new List<WaterTextureList>();
        public WaterTransparency WaterTransparency { get; internal set; }
        public List<Weapon> Weapons { get; } = new List<Weapon>();
        public Weather Weather { get; internal set; }

        [AddedIn(SageGame.Bfme)]
        public List<WeatherData> WeatherDatas { get; } = new List<WeatherData>();

        public List<WebpageUrl> WebpageUrls { get; } = new List<WebpageUrl>();
        internal Dictionary<string, IniToken> Defines { get; } = new Dictionary<string, IniToken>();

        [AddedIn(SageGame.Bfme2)]
        public Fire Fire { get; internal set; }

        [AddedIn(SageGame.Bfme2)]
        public ArmySummaryDescription ArmySummaryDescription { get; internal set; }
    }
}
