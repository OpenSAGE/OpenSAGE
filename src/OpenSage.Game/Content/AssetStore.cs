using System.Collections.Generic;
using OpenSage.Audio;
using OpenSage.Content.Loaders;
using OpenSage.Data.StreamFS;
using OpenSage.Eva;
using OpenSage.FX;
using OpenSage.Graphics;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Graphics.Shaders;
using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Gui.ControlBar;
using OpenSage.Gui.InGame;
using OpenSage.Gui.Wnd.Transitions;
using OpenSage.Input;
using OpenSage.Input.Cursors;
using OpenSage.IO;
using OpenSage.LivingWorld;
using OpenSage.LivingWorld.AutoResolve;
using OpenSage.Lod;
using OpenSage.Logic;
using OpenSage.Logic.AI;
using OpenSage.Logic.Object;
using OpenSage.Logic.Pathfinding;
using OpenSage.Rendering;
using OpenSage.Terrain;
using OpenSage.Terrain.Roads;
using OpenSage.Utilities;
using Veldrid;

namespace OpenSage.Content
{
    public sealed class AssetStore
    {
        private readonly List<IScopedSingleAssetStorage> _scopedSingleAssetStorage;
        private readonly Dictionary<uint, IScopedSingleAssetStorage> _singleAssetStorageByTypeId;

        private readonly List<IScopedAssetCollection> _scopedAssetCollections;
        private readonly Dictionary<uint, IScopedAssetCollection> _byTypeId;

        internal AssetLoadContext LoadContext { get; }

        // Singletons
        public ScopedSingleAsset<AIData> AIData { get; }
        public ScopedSingleAsset<AnimationSoundClientBehaviorGlobalSetting> AnimationSoundClientBehaviorGlobalSetting { get; }
        public ScopedSingleAsset<AptButtonTooltipMap> AptButtonTooltipMap { get; }
        public ScopedSingleAsset<ArmySummaryDescription> ArmySummaryDescription { get; }
        public ScopedSingleAsset<AudioSettings> AudioSettings { get; }
        public ScopedSingleAsset<AwardSystem> AwardSystem { get; }
        public ScopedSingleAsset<BannerUI> BannerUI { get; }
        public ScopedSingleAsset<ChallengeGenerals> ChallengeGenerals { get; }
        public ScopedSingleAsset<CreateAHeroSystem> CreateAHeroSystem { get; }
        public ScopedSingleAsset<Credits> Credits { get; }
        public ScopedSingleAsset<DrawGroupInfo> DrawGroupInfo { get; }
        public ScopedSingleAsset<Environment> Environment { get; }
        public ScopedSingleAsset<Fire> Fire { get; }
        public ScopedSingleAsset<FireLogicSystem> FireLogicSystem { get; }
        public ScopedSingleAsset<FormationAssistant> FormationAssistant { get; }
        public ScopedSingleAsset<GameData> GameData { get; }
        public ScopedSingleAsset<InGameNotificationBox> InGameNotificationBox { get; }
        public ScopedSingleAsset<InGameUI> InGameUI { get; }
        public ScopedSingleAsset<Language> Language { get; }
        public ScopedSingleAsset<LinearCampaign> LinearCampaign { get; }
        public ScopedSingleAsset<LargeGroupAudioUnusedKnownKeys> LargeGroupAudioUnusedKnownKeys { get; }
        public ScopedSingleAsset<LivingWorldMapInfo> LivingWorldMapInfo { get; }
        public ScopedSingleAsset<LivingWorldAutoResolveResourceBonus> LivingWorldAutoResolveResourceBonus { get; }
        public ScopedSingleAsset<LivingWorldAutoResolveSciencePurchasePointBonus> LivingWorldAutoResolveSciencePurchasePointBonus { get; }
        public ScopedSingleAsset<MiscAudio> MiscAudio { get; }
        public ScopedSingleAsset<MiscEvaData> MiscEvaData { get; }
        public ScopedSingleAsset<MissionObjectiveList> MissionObjectiveList { get; }
        public ScopedSingleAsset<MouseData> MouseData { get; }
        public ScopedSingleAsset<MultiplayerSettings> MultiplayerSettings { get; }
        public ScopedSingleAsset<OnlineChatColors> OnlineChatColors { get; }
        public ScopedSingleAsset<Pathfinder> Pathfinder { get; }
        public ScopedSingleAsset<StrategicHud> StrategicHud { get; }
        public ScopedSingleAsset<WaterTransparency> WaterTransparency { get; }
        public ScopedSingleAsset<Weather> Weather { get; }

        // Collections
        public ScopedAssetCollection<AIBase> AIBases { get; }
        public ScopedAssetCollection<AIDozerAssignment> AIDozerAssignments { get; }
        public ScopedAssetCollection<AmbientStream> AmbientStreams { get; }
        public ScopedAssetCollection<AnimationTemplate> Animations { get; }
        public ScopedAssetCollection<ArmorTemplate> ArmorTemplates { get; }
        public ScopedAssetCollection<ArmyDefinition> ArmyDefinitions { get; }
        public ScopedAssetCollection<AudioEvent> AudioEvents { get; }
        public ScopedAssetCollection<AudioFile> AudioFiles { get; }
        public ScopedAssetCollection<AudioLod> AudioLods { get; }
        public ScopedAssetCollection<AutoResolveArmor> AutoResolveArmors { get; }
        public ScopedAssetCollection<AutoResolveBody> AutoResolveBodies { get; }
        public ScopedAssetCollection<AutoResolveCombatChain> AutoResolveCombatChains { get; }
        public ScopedAssetCollection<AutoResolveHandicapLevel> AutoResolveHandicapLevels { get; }
        public ScopedAssetCollection<AutoResolveLeadership> AutoResolveLeaderships { get; }
        public ScopedAssetCollection<AutoResolveReinforcementSchedule> AutoResolveReinforcementSchedules { get; }
        public ScopedAssetCollection<AutoResolveWeapon> AutoResolveWeapons { get; }
        public ScopedAssetCollection<BannerType> BannerTypes { get; }
        public ScopedAssetCollection<BenchProfile> BenchProfiles { get; }
        public ScopedAssetCollection<BridgeTemplate> BridgeTemplates { get; }
        public ScopedAssetCollection<CampaignTemplate> CampaignTemplates { get; }
        public ScopedAssetCollection<CommandButton> CommandButtons { get; }
        public ScopedAssetCollection<CommandMap> CommandMaps { get; }
        public ScopedAssetCollection<CommandSet> CommandSets { get; }
        public ScopedAssetCollection<ControlBarResizer> ControlBarResizers { get; }
        public ScopedAssetCollection<ControlBarScheme> ControlBarSchemes { get; }
        public ScopedAssetCollection<CrateData> CrateDatas { get; }
        public ScopedAssetCollection<CrowdResponse> CrowdResponses { get; }
        public ScopedAssetCollection<DamageFX> DamageFXs { get; }
        public ScopedAssetCollection<DialogEvent> DialogEvents { get; }
        public ScopedAssetCollection<DynamicGameLod> DynamicGameLods { get; }
        public ScopedAssetCollection<EmotionNugget> EmotionNuggets { get; }
        public ScopedAssetCollection<EvaEvent> EvaEvents { get; }
        public ScopedAssetCollection<ExperienceLevel> ExperienceLevels { get; }
        public ScopedAssetCollection<ExperienceScalarTable> ExperienceScalarTables { get; }
        public ScopedAssetCollection<FactionVictoryData> FactionVictoryDatas { get; }
        public ScopedAssetCollection<FontDefaultSetting> FontDefaultSettings { get; }
        public ScopedAssetCollection<FontSubstitution> FontSubstitutions { get; }
        public ScopedAssetCollection<FXList> FXLists { get; }
        public ScopedAssetCollection<FXParticleSystemTemplate> FXParticleSystemTemplates { get; }
        public ScopedAssetCollection<GuiTextureAsset> GuiTextures { get; }
        public ScopedAssetCollection<HeaderTemplate> HeaderTemplates { get; }
        public ScopedAssetCollection<HouseColor> HouseColors { get; }
        public ScopedAssetCollection<LargeGroupAudioMap> LargeGroupAudioMaps { get; }
        public ScopedAssetCollection<LivingWorldAITemplate> LivingWorldAITemplates { get; }
        public ScopedAssetCollection<LivingWorldAnimObject> LivingWorldAnimObjects { get; }
        public ScopedAssetCollection<LivingWorldArmyIcon> LivingWorldArmyIcons { get; }
        public ScopedAssetCollection<LivingWorldBuilding> LivingWorldBuildings { get; }
        public ScopedAssetCollection<LivingWorldBuildingIcon> LivingWorldBuildingIcons { get; }
        public ScopedAssetCollection<LivingWorldBuildPlotIcon> LivingWorldBuildPlotIcons { get; }
        public ScopedAssetCollection<LivingWorldCampaign> LivingWorldCampaigns { get; }
        public ScopedAssetCollection<LivingWorldObject> LivingWorldObjects { get; }
        public ScopedAssetCollection<LivingWorldPlayerArmy> LivingWorldPlayerArmies { get; }
        public ScopedAssetCollection<LivingWorldPlayerTemplate> LivingWorldPlayerTemplates { get; }
        public ScopedAssetCollection<LivingWorldRegionCampaign> LivingWorldRegionCampaigns { get; }
        public ScopedAssetCollection<LivingWorldRegionEffects> LivingWorldRegionEffects { get; }
        public ScopedAssetCollection<LivingWorldSound> LivingWorldSounds { get; }
        public ScopedAssetCollection<LocomotorTemplate> LocomotorTemplates { get; }
        public ScopedAssetCollection<LodPreset> LodPresets { get; }
        public ScopedAssetCollection<MapCache> MapCaches { get; }
        public ScopedAssetCollection<MappedImage> MappedImages { get; }
        public ScopedAssetCollection<MeshNameMatches> MeshNameMatches { get; }
        public ScopedAssetCollection<Model> Models { get; }
        public ScopedAssetCollection<Graphics.Animation.W3DAnimation> ModelAnimations { get; }
        public ScopedAssetCollection<ModelBoneHierarchy> ModelBoneHierarchies { get; }
        public ScopedAssetCollection<ModelMesh> ModelMeshes { get; }
        public ScopedAssetCollection<ModifierList> ModifierLists { get; }
        public ScopedAssetCollection<MouseCursor> MouseCursors { get; }
        public ScopedAssetCollection<MultiplayerColor> MultiplayerColors { get; }
        public ScopedAssetCollection<MultiplayerStartingMoneyChoice> MultiplayerStartingMoneyChoices { get; }
        public ScopedAssetCollection<Multisound> Multisounds { get; }
        public ScopedAssetCollection<MusicTrack> MusicTracks { get; }
        public ScopedAssetCollection<ObjectCreationList> ObjectCreationLists { get; }
        public ScopedAssetCollection<ObjectDefinition> ObjectDefinitions { get; }
        public ScopedAssetCollection<PlayerAIType> PlayerAITypes { get; }
        public ScopedAssetCollection<PlayerTemplate> PlayerTemplates { get; }
        public ScopedAssetCollection<RankTemplate> Ranks { get; }
        public ScopedAssetCollection<RegionCampaign> RegionCampaigns { get; }
        public ScopedAssetCollection<RoadTemplate> RoadTemplates { get; }
        public ScopedAssetCollection<Science> Sciences { get; }
        public ScopedAssetCollection<ScoredKillEvaAnnouncer> ScoredKillEvaAnnouncers { get; }
        public ScopedAssetCollection<ShellMenuScheme> ShellMenuSchemes { get; }
        public ScopedAssetCollection<SkirmishAIData> SkirmishAIDatas { get; }
        public ScopedAssetCollection<SkyboxTextureSet> SkyboxTextureSets { get; }
        public ScopedAssetCollection<SpecialPower> SpecialPowers { get; }
        public ScopedAssetCollection<StanceTemplate> StanceTemplates { get; }
        public ScopedAssetCollection<StaticGameLod> StaticGameLods { get; }
        public ScopedAssetCollection<StreamedSound> StreamedSounds { get; }
        public ScopedAssetCollection<LoadSubsystem> Subsystems { get; }
        public ScopedAssetCollection<TerrainTexture> TerrainTextures { get; }
        public ScopedAssetCollection<TextureAsset> Textures { get; }
        public ScopedAssetCollection<UpgradeTemplate> Upgrades { get; }
        public ScopedAssetCollection<VictorySystemData> VictorySystemDatas { get; }
        public ScopedAssetCollection<Video> Videos { get; }
        public ScopedAssetCollection<WaterSet> WaterSets { get; }
        public ScopedAssetCollection<WaterTextureList> WaterTextureLists { get; }
        public ScopedAssetCollection<WeaponTemplate> WeaponTemplates { get; }
        public ScopedAssetCollection<WeatherData> WeatherDatas { get; }
        public ScopedAssetCollection<WindowTransition> WindowTransitions { get; }

        internal AssetStore(
            SageGame sageGame,
            FileSystem fileSystem,
            string language,
            GraphicsDevice graphicsDevice,
            StandardGraphicsResources standardGraphicsResources,
            ShaderResourceManager shaderResources,
            ShaderSetStore shaderSetStore,
            OnDemandAssetLoadStrategy loadStrategy)
        {
            LoadContext = new AssetLoadContext(
                fileSystem,
                language,
                graphicsDevice,
                standardGraphicsResources,
                shaderResources,
                shaderSetStore,
                this);

            _scopedSingleAssetStorage = new List<IScopedSingleAssetStorage>();
            _singleAssetStorageByTypeId = new Dictionary<uint, IScopedSingleAssetStorage>();

            void AddSingleAssetStorage<TAsset>(ScopedSingleAsset<TAsset> assetStorage)
                where TAsset : BaseSingletonAsset, new()
            {
                _scopedSingleAssetStorage.Add(assetStorage);

                var typeId = AssetTypeUtility.GetAssetTypeId<TAsset>(sageGame);
                _singleAssetStorageByTypeId.Add(typeId, assetStorage);
            }

            AddSingleAssetStorage(AIData = new ScopedSingleAsset<AIData>());
            AddSingleAssetStorage(AnimationSoundClientBehaviorGlobalSetting = new ScopedSingleAsset<AnimationSoundClientBehaviorGlobalSetting>());
            AddSingleAssetStorage(AptButtonTooltipMap = new ScopedSingleAsset<AptButtonTooltipMap>());
            AddSingleAssetStorage(ArmySummaryDescription = new ScopedSingleAsset<ArmySummaryDescription>());
            AddSingleAssetStorage(AudioSettings = new ScopedSingleAsset<AudioSettings>());
            AddSingleAssetStorage(AwardSystem = new ScopedSingleAsset<AwardSystem>());
            AddSingleAssetStorage(BannerUI = new ScopedSingleAsset<BannerUI>());
            AddSingleAssetStorage(ChallengeGenerals = new ScopedSingleAsset<ChallengeGenerals>());
            AddSingleAssetStorage(CreateAHeroSystem = new ScopedSingleAsset<CreateAHeroSystem>());
            AddSingleAssetStorage(Credits = new ScopedSingleAsset<Credits>());
            AddSingleAssetStorage(DrawGroupInfo = new ScopedSingleAsset<DrawGroupInfo>());
            AddSingleAssetStorage(Environment = new ScopedSingleAsset<Environment>());
            AddSingleAssetStorage(Fire = new ScopedSingleAsset<Fire>());
            AddSingleAssetStorage(FireLogicSystem = new ScopedSingleAsset<FireLogicSystem>());
            AddSingleAssetStorage(FormationAssistant = new ScopedSingleAsset<FormationAssistant>());
            AddSingleAssetStorage(GameData = new ScopedSingleAsset<GameData>());
            AddSingleAssetStorage(InGameNotificationBox = new ScopedSingleAsset<InGameNotificationBox>());
            AddSingleAssetStorage(InGameUI = new ScopedSingleAsset<InGameUI>());
            AddSingleAssetStorage(Language = new ScopedSingleAsset<Language>());
            AddSingleAssetStorage(LinearCampaign = new ScopedSingleAsset<LinearCampaign>());
            AddSingleAssetStorage(LargeGroupAudioUnusedKnownKeys = new ScopedSingleAsset<LargeGroupAudioUnusedKnownKeys>());
            AddSingleAssetStorage(LivingWorldMapInfo = new ScopedSingleAsset<LivingWorldMapInfo>());
            AddSingleAssetStorage(LivingWorldAutoResolveResourceBonus = new ScopedSingleAsset<LivingWorldAutoResolveResourceBonus>());
            AddSingleAssetStorage(LivingWorldAutoResolveSciencePurchasePointBonus = new ScopedSingleAsset<LivingWorldAutoResolveSciencePurchasePointBonus>());
            AddSingleAssetStorage(MiscAudio = new ScopedSingleAsset<MiscAudio>());
            AddSingleAssetStorage(MiscEvaData = new ScopedSingleAsset<MiscEvaData>());
            AddSingleAssetStorage(MissionObjectiveList = new ScopedSingleAsset<MissionObjectiveList>());
            AddSingleAssetStorage(MouseData = new ScopedSingleAsset<MouseData>());
            AddSingleAssetStorage(MultiplayerSettings = new ScopedSingleAsset<MultiplayerSettings>());
            AddSingleAssetStorage(OnlineChatColors = new ScopedSingleAsset<OnlineChatColors>());
            AddSingleAssetStorage(Pathfinder = new ScopedSingleAsset<Pathfinder>());
            AddSingleAssetStorage(StrategicHud = new ScopedSingleAsset<StrategicHud>());
            AddSingleAssetStorage(WaterTransparency = new ScopedSingleAsset<WaterTransparency>());
            AddSingleAssetStorage(Weather = new ScopedSingleAsset<Weather>());

            _scopedAssetCollections = new List<IScopedAssetCollection>();
            _byTypeId = new Dictionary<uint, IScopedAssetCollection>();

            void AddAssetCollection<TAsset>(ScopedAssetCollection<TAsset> assetCollection)
                where TAsset : BaseAsset
            {
                _scopedAssetCollections.Add(assetCollection);

                var typeId = AssetTypeUtility.GetAssetTypeId<TAsset>(sageGame);
                _byTypeId.Add(typeId, assetCollection);
            }

            AddAssetCollection(AIBases = new ScopedAssetCollection<AIBase>(this));
            AddAssetCollection(AIDozerAssignments = new ScopedAssetCollection<AIDozerAssignment>(this));
            AddAssetCollection(AmbientStreams = new ScopedAssetCollection<AmbientStream>(this));
            AddAssetCollection(Animations = new ScopedAssetCollection<AnimationTemplate>(this));
            AddAssetCollection(ArmorTemplates = new ScopedAssetCollection<ArmorTemplate>(this));
            AddAssetCollection(ArmyDefinitions = new ScopedAssetCollection<ArmyDefinition>(this));
            AddAssetCollection(AudioEvents = new ScopedAssetCollection<AudioEvent>(this));
            AddAssetCollection(AudioFiles = new ScopedAssetCollection<AudioFile>(this, loadStrategy.CreateAudioFileLoader()));
            AddAssetCollection(AudioLods = new ScopedAssetCollection<AudioLod>(this));
            AddAssetCollection(AutoResolveArmors = new ScopedAssetCollection<AutoResolveArmor>(this));
            AddAssetCollection(AutoResolveBodies = new ScopedAssetCollection<AutoResolveBody>(this));
            AddAssetCollection(AutoResolveCombatChains = new ScopedAssetCollection<AutoResolveCombatChain>(this));
            AddAssetCollection(AutoResolveHandicapLevels = new ScopedAssetCollection<AutoResolveHandicapLevel>(this));
            AddAssetCollection(AutoResolveLeaderships = new ScopedAssetCollection<AutoResolveLeadership>(this));
            AddAssetCollection(AutoResolveReinforcementSchedules = new ScopedAssetCollection<AutoResolveReinforcementSchedule>(this));
            AddAssetCollection(AutoResolveWeapons = new ScopedAssetCollection<AutoResolveWeapon>(this));
            AddAssetCollection(BannerTypes = new ScopedAssetCollection<BannerType>(this));
            AddAssetCollection(BenchProfiles = new ScopedAssetCollection<BenchProfile>(this));
            AddAssetCollection(BridgeTemplates = new ScopedAssetCollection<BridgeTemplate>(this));
            AddAssetCollection(CampaignTemplates = new ScopedAssetCollection<CampaignTemplate>(this));
            AddAssetCollection(CommandButtons = new ScopedAssetCollection<CommandButton>(this));
            AddAssetCollection(CommandMaps = new ScopedAssetCollection<CommandMap>(this));
            AddAssetCollection(CommandSets = new ScopedAssetCollection<CommandSet>(this));
            AddAssetCollection(ControlBarResizers = new ScopedAssetCollection<ControlBarResizer>(this));
            AddAssetCollection(ControlBarSchemes = new ScopedAssetCollection<ControlBarScheme>(this));
            AddAssetCollection(CrateDatas = new ScopedAssetCollection<CrateData>(this));
            AddAssetCollection(CrowdResponses = new ScopedAssetCollection<CrowdResponse>(this));
            AddAssetCollection(DamageFXs = new ScopedAssetCollection<DamageFX>(this));
            AddAssetCollection(DialogEvents = new ScopedAssetCollection<DialogEvent>(this));
            AddAssetCollection(DynamicGameLods = new ScopedAssetCollection<DynamicGameLod>(this));
            AddAssetCollection(EmotionNuggets = new ScopedAssetCollection<EmotionNugget>(this));
            AddAssetCollection(EvaEvents = new ScopedAssetCollection<EvaEvent>(this));
            AddAssetCollection(ExperienceLevels = new ScopedAssetCollection<ExperienceLevel>(this));
            AddAssetCollection(ExperienceScalarTables = new ScopedAssetCollection<ExperienceScalarTable>(this));
            AddAssetCollection(FactionVictoryDatas = new ScopedAssetCollection<FactionVictoryData>(this));
            AddAssetCollection(FontDefaultSettings = new ScopedAssetCollection<FontDefaultSetting>(this));
            AddAssetCollection(FontSubstitutions = new ScopedAssetCollection<FontSubstitution>(this));
            AddAssetCollection(FXLists = new ScopedAssetCollection<FXList>(this));
            AddAssetCollection(FXParticleSystemTemplates = new ScopedAssetCollection<FXParticleSystemTemplate>(this));
            AddAssetCollection(GuiTextures = new ScopedAssetCollection<GuiTextureAsset>(this, loadStrategy.CreateGuiTextureLoader()));
            AddAssetCollection(HeaderTemplates = new ScopedAssetCollection<HeaderTemplate>(this));
            AddAssetCollection(HouseColors = new ScopedAssetCollection<HouseColor>(this));
            AddAssetCollection(LargeGroupAudioMaps = new ScopedAssetCollection<LargeGroupAudioMap>(this));
            AddAssetCollection(LivingWorldAITemplates = new ScopedAssetCollection<LivingWorldAITemplate>(this));
            AddAssetCollection(LivingWorldAnimObjects = new ScopedAssetCollection<LivingWorldAnimObject>(this));
            AddAssetCollection(LivingWorldArmyIcons = new ScopedAssetCollection<LivingWorldArmyIcon>(this));
            AddAssetCollection(LivingWorldBuildings = new ScopedAssetCollection<LivingWorldBuilding>(this));
            AddAssetCollection(LivingWorldBuildingIcons = new ScopedAssetCollection<LivingWorldBuildingIcon>(this));
            AddAssetCollection(LivingWorldBuildPlotIcons = new ScopedAssetCollection<LivingWorldBuildPlotIcon>(this));
            AddAssetCollection(LivingWorldCampaigns = new ScopedAssetCollection<LivingWorldCampaign>(this));
            AddAssetCollection(LivingWorldObjects = new ScopedAssetCollection<LivingWorldObject>(this));
            AddAssetCollection(LivingWorldPlayerArmies = new ScopedAssetCollection<LivingWorldPlayerArmy>(this));
            AddAssetCollection(LivingWorldPlayerTemplates = new ScopedAssetCollection<LivingWorldPlayerTemplate>(this));
            AddAssetCollection(LivingWorldRegionCampaigns = new ScopedAssetCollection<LivingWorldRegionCampaign>(this));
            AddAssetCollection(LivingWorldRegionEffects = new ScopedAssetCollection<LivingWorldRegionEffects>(this));
            AddAssetCollection(LivingWorldSounds = new ScopedAssetCollection<LivingWorldSound>(this));
            AddAssetCollection(LocomotorTemplates = new ScopedAssetCollection<LocomotorTemplate>(this));
            AddAssetCollection(LodPresets = new ScopedAssetCollection<LodPreset>(this));
            AddAssetCollection(MapCaches = new ScopedAssetCollection<MapCache>(this));
            AddAssetCollection(MappedImages = new ScopedAssetCollection<MappedImage>(this));
            AddAssetCollection(MeshNameMatches = new ScopedAssetCollection<MeshNameMatches>(this));
            AddAssetCollection(Models = new ScopedAssetCollection<Model>(this, loadStrategy.CreateModelLoader()));
            AddAssetCollection(ModelAnimations = new ScopedAssetCollection<Graphics.Animation.W3DAnimation>(this, loadStrategy.CreateAnimationLoader()));
            AddAssetCollection(ModelBoneHierarchies = new ScopedAssetCollection<ModelBoneHierarchy>(this, loadStrategy.CreateModelBoneHierarchyLoader()));
            AddAssetCollection(ModelMeshes = new ScopedAssetCollection<ModelMesh>(this)); // TODO: ModelMesh loader?
            AddAssetCollection(ModifierLists = new ScopedAssetCollection<ModifierList>(this));
            AddAssetCollection(MouseCursors = new ScopedAssetCollection<MouseCursor>(this));
            AddAssetCollection(MultiplayerColors = new ScopedAssetCollection<MultiplayerColor>(this));
            AddAssetCollection(MultiplayerStartingMoneyChoices = new ScopedAssetCollection<MultiplayerStartingMoneyChoice>(this));
            AddAssetCollection(Multisounds = new ScopedAssetCollection<Multisound>(this));
            AddAssetCollection(MusicTracks = new ScopedAssetCollection<MusicTrack>(this));
            AddAssetCollection(ObjectCreationLists = new ScopedAssetCollection<ObjectCreationList>(this));
            AddAssetCollection(ObjectDefinitions = new ScopedAssetCollection<ObjectDefinition>(this));
            AddAssetCollection(PlayerAITypes = new ScopedAssetCollection<PlayerAIType>(this));
            AddAssetCollection(PlayerTemplates = new ScopedAssetCollection<PlayerTemplate>(this));
            AddAssetCollection(Ranks = new ScopedAssetCollection<RankTemplate>(this));
            AddAssetCollection(RegionCampaigns = new ScopedAssetCollection<RegionCampaign>(this));
            AddAssetCollection(RoadTemplates = new ScopedAssetCollection<RoadTemplate>(this));
            AddAssetCollection(Sciences = new ScopedAssetCollection<Science>(this));
            AddAssetCollection(ScoredKillEvaAnnouncers = new ScopedAssetCollection<ScoredKillEvaAnnouncer>(this));
            AddAssetCollection(ShellMenuSchemes = new ScopedAssetCollection<ShellMenuScheme>(this));
            AddAssetCollection(SkirmishAIDatas = new ScopedAssetCollection<SkirmishAIData>(this));
            AddAssetCollection(SkyboxTextureSets = new ScopedAssetCollection<SkyboxTextureSet>(this));
            AddAssetCollection(SpecialPowers = new ScopedAssetCollection<SpecialPower>(this));
            AddAssetCollection(StanceTemplates = new ScopedAssetCollection<StanceTemplate>(this));
            AddAssetCollection(StaticGameLods = new ScopedAssetCollection<StaticGameLod>(this));
            AddAssetCollection(StreamedSounds = new ScopedAssetCollection<StreamedSound>(this));
            AddAssetCollection(Subsystems = new ScopedAssetCollection<LoadSubsystem>(this));
            AddAssetCollection(TerrainTextures = new ScopedAssetCollection<TerrainTexture>(this));
            AddAssetCollection(Textures = new ScopedAssetCollection<TextureAsset>(this, loadStrategy.CreateTextureLoader()));
            AddAssetCollection(Upgrades = new ScopedAssetCollection<UpgradeTemplate>(this));
            AddAssetCollection(VictorySystemDatas = new ScopedAssetCollection<VictorySystemData>(this));
            AddAssetCollection(Videos = new ScopedAssetCollection<Video>(this));
            AddAssetCollection(WaterSets = new ScopedAssetCollection<WaterSet>(this));
            AddAssetCollection(WaterTextureLists = new ScopedAssetCollection<WaterTextureList>(this));
            AddAssetCollection(WeaponTemplates = new ScopedAssetCollection<WeaponTemplate>(this));
            AddAssetCollection(WeatherDatas = new ScopedAssetCollection<WeatherData>(this));
            AddAssetCollection(WindowTransitions = new ScopedAssetCollection<WindowTransition>(this));
        }

        internal IScopedAssetCollection GetAssetCollection(uint typeId)
        {
            _byTypeId.TryGetValue(typeId, out var result);
            return result;
        }

        internal IScopedSingleAssetStorage GetSingleAsset(uint typeId)
        {
            _singleAssetStorageByTypeId.TryGetValue(typeId, out var result);
            return result;
        }

        internal void PushScope()
        {
            foreach (var scopedAssetCollection in _scopedAssetCollections)
            {
                scopedAssetCollection.PushScope();
            }
            foreach (var scopedSingleton in _scopedSingleAssetStorage)
            {
                scopedSingleton.PushScope();
            }
        }

        internal void PopScope()
        {
            foreach (var scopedAssetCollection in _scopedAssetCollections)
            {
                scopedAssetCollection.PopScope();
            }
            foreach (var scopedSingleton in _scopedSingleAssetStorage)
            {
                scopedSingleton.PopScope();
            }
        }

        internal IEnumerable<BaseAsset> GetAllAssets()
        {
            foreach (var scopedSingleton in _scopedSingleAssetStorage)
            {
                yield return scopedSingleton.Current;
            }
            foreach (var scopedAssetCollection in _scopedAssetCollections)
            {
                foreach (var asset in scopedAssetCollection.GetAssets())
                {
                    yield return asset;
                }
            }
        }
    }
}
