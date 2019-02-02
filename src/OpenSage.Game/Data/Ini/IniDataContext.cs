using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenSage.Data.Ini.Parser;
using OpenSage.Diagnostics;
using OpenSage.Logic.Object;

namespace OpenSage.Data.Ini
{
    public sealed class IniDataContext
    {
        private readonly FileSystem _fileSystem;
        private readonly SageGame _game;

        // TODO: Remove this once we can load all INI files upfront.
        private readonly List<string> _alreadyLoaded = new List<string>();

        public AIData AIData { get; internal set; }
        public List<AmbientStream> AmbientStreams { get; } = new List<AmbientStream>();
        public List<Animation> Animations { get; } = new List<Animation>();
        public AnimationSoundClientBehaviorGlobalSetting AnimationSoundClientBehaviorGlobalSetting { get; internal set; }

        [AddedIn(SageGame.Bfme2)]
        public AptButtonTooltipMap AptButtonTooltipMap { get; internal set;  }

        public List<Armor> Armors { get; } = new List<Armor>();
        public Dictionary<string, AudioEvent> AudioEvents { get; } = new Dictionary<string, AudioEvent>();
        public List<AudioLod> AudioLods { get; } = new List<AudioLod>();
        public int AudioLowMHz { get; internal set; }
        public AudioSettings AudioSettings { get; internal set; }

        [AddedIn(SageGame.Bfme2)]
        public List<AutoResolveArmor> AutoResolveArmors { get; } = new List<AutoResolveArmor>();

        [AddedIn(SageGame.Bfme2)]
        public AwardSystem AwardSystem { get; internal set; }

        public List<BannerType> BannerTypes { get; } = new List<BannerType>();
        public BannerUI BannerUI { get; internal set; }
        public List<BenchProfile> BenchProfiles { get; } = new List<BenchProfile>();
        public List<BridgeTemplate> Bridges { get; } = new List<BridgeTemplate>();
        public List<Campaign> Campaigns { get; } = new List<Campaign>();
        public ChallengeGenerals ChallengeGenerals { get; internal set; }
        public List<CommandButton> CommandButtons { get; } = new List<CommandButton>();
        public List<CommandMap> CommandMaps { get; } = new List<CommandMap>();
        public List<CommandSet> CommandSets { get; } = new List<CommandSet>();
        public List<ControlBarResizer> ControlBarResizers { get; } = new List<ControlBarResizer>();
        public ControlBarSchemeCollection ControlBarSchemes { get; } = new ControlBarSchemeCollection();
        public List<CrateData> CrateDatas { get; } = new List<CrateData>();
        public Credits Credits { get; internal set; }

        [AddedIn(SageGame.Bfme2)]
        public List<CrowdResponse> CrowdResponses { get; } = new List<CrowdResponse>();

        public List<DamageFX> DamageFXs { get; } = new List<DamageFX>();
        public List<DialogEvent> DialogEvents { get; } = new List<DialogEvent>();
        public DrawGroupInfo DrawGroupInfo { get; internal set; }
        public List<DynamicGameLod> DynamicGameLods { get; } = new List<DynamicGameLod>();
        public List<EmotionNugget> EmotionNuggets { get; } = new List<EmotionNugget>();
        public Environment Environment { get; } = new Environment();
        public Dictionary<string, EvaEvent> EvaEvents { get; } = new Dictionary<string, EvaEvent>();
        public List<ExperienceLevel> ExperienceLevels { get; } = new List<ExperienceLevel>();
        public List<ExperienceScalarTable> ExperienceScalarTables { get; } = new List<ExperienceScalarTable>();
        public List<FactionVictoryData> FactionVictoryDatas { get; } = new List<FactionVictoryData>();
        public List<FontDefaultSetting> FontDefaultSettings { get; } = new List<FontDefaultSetting>();
        public List<FontSubstitution> FontSubstitutions { get; } = new List<FontSubstitution>();
        public List<FXList> FXLists { get; } = new List<FXList>();
        public List<FXParticleSystemTemplate> FXParticleSystems { get; } = new List<FXParticleSystemTemplate>();
        public GameData GameData { get; internal set; }
        public List<HeaderTemplate> HeaderTemplates { get; } = new List<HeaderTemplate>();
        public List<HouseColor> HouseColors { get; } = new List<HouseColor>();
        public InGameUI InGameUI { get; internal set; }
        public Language Language { get; internal set; }
        public List<LargeGroupAudioMap> LargeGroupAudioMaps { get; } = new List<LargeGroupAudioMap>();

        [AddedIn(SageGame.Bfme2)]
        public LivingWorldAiTemplate LivingWorldAiTemplate { get; internal set; }

        public LargeGroupAudioUnusedKnownKeys LargeGroupAudioUnusedKnownKeys { get; internal set; }
        public List<LivingWorldCampaign> LivingWorldCampaigns { get; } = new List<LivingWorldCampaign>();

        [AddedIn(SageGame.Bfme)]
        public LivingWorldMapInfo LivingWorldMapInfo { get; set; }

        [AddedIn(SageGame.Bfme)]
        public List<LivingWorldObject> LivingWorldObjects { get; } = new List<LivingWorldObject>();
        public List<LivingWorldPlayerArmy> LivingWorldPlayerArmies { get; } = new List<LivingWorldPlayerArmy>();

        [AddedIn(SageGame.Bfme)]
        public List<LivingWorldArmyIcon> LivingWorldArmyIcons { get; } = new List<LivingWorldArmyIcon>();

        [AddedIn(SageGame.Bfme)]
        public List<LivingWorldAnimObject> LivingWorldAnimObjects { get; } = new List<LivingWorldAnimObject>();

        [AddedIn(SageGame.Bfme)]
        public List<LivingWorldSound> LivingWorldSounds { get; } = new List<LivingWorldSound>();
        public List<LivingWorldRegionCampaign> LivingWorldRegionCampaigns { get; } = new List<LivingWorldRegionCampaign>();
        public List<Locomotor> Locomotors { get; } = new List<Locomotor>();
        public List<LodPreset> LodPresets { get; } = new List<LodPreset>();
        public List<MapCache> MapCaches { get; } = new List<MapCache>();
        public List<MappedImage> MappedImages { get; } = new List<MappedImage>();
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
        public List<MusicTrack> MusicTracks { get; } = new List<MusicTrack>();
        public List<ObjectDefinition> Objects { get; } = new List<ObjectDefinition>();
        public List<ObjectCreationList> ObjectCreationLists { get; } = new List<ObjectCreationList>();
        public OnlineChatColors OnlineChatColors { get; internal set; }
        public List<ParticleSystemDefinition> ParticleSystems { get; } = new List<ParticleSystemDefinition>();

        [AddedIn(SageGame.Bfme)]
        public Pathfinder Pathfinder { get; set; }

        [AddedIn(SageGame.Bfme)]
        public List<PlayerAIType> PlayerAITypes { get; } = new List<PlayerAIType>();

        public List<PlayerTemplate> PlayerTemplates { get; } = new List<PlayerTemplate>();
        public List<Rank> Ranks { get; } = new List<Rank>();

        [AddedIn(SageGame.Bfme)]
        public RegionCampain RegionCampaign{ get; set; }
        public List<RoadTemplate> RoadTemplates { get; } = new List<RoadTemplate>();
        public int ReallyLowMHz { get; internal set; }
        public List<Science> Sciences { get; } = new List<Science>();
        public List<ShellMenuScheme> ShellMenuSchemes { get; } = new List<ShellMenuScheme>();

        [AddedIn(SageGame.Bfme2)]
        public SkirmishAIData SkirmishAIData { get; internal set; }

        public List<SkyboxTextureSet> SkyboxTextureSets { get; } = new List<SkyboxTextureSet>();
        public List<SpecialPower> SpecialPowers { get; } = new List<SpecialPower>();

        [AddedIn(SageGame.Bfme2)]
        public List<StanceTemplate> StanceTemplates { get; } = new List<StanceTemplate>();

        public List<StaticGameLod> StaticGameLods { get; } = new List<StaticGameLod>();
        public List<StreamedSound> StreamedSounds { get; } = new List<StreamedSound>();
        public List<LoadSubsystem> Subsystems { get; } = new List<LoadSubsystem>();
        public List<TerrainTexture> TerrainTextures { get; } = new List<TerrainTexture>();
        public List<Upgrade> Upgrades { get; } = new List<Upgrade>();
        public List<VictorySystemData> VictorySystemDatas { get; } = new List<VictorySystemData>();
        public List<Video> Videos { get; } = new List<Video>();
        public List<WaterSet> WaterSets { get; } = new List<WaterSet>();
        public List<WaterTextureList> WaterTextureLists { get; } = new List<WaterTextureList>();
        public WaterTransparency WaterTransparency { get; internal set; }
        public List<Weapon> Weapons { get; } = new List<Weapon>();
        public Weather Weather { get; internal set; }

        [AddedIn(SageGame.Bfme)]
        public List<WeatherData> WeatherDatas { get; } = new List<WeatherData>();

        public List<WebpageUrl> WebpageUrls { get; } = new List<WebpageUrl>();
        public List<WindowTransition> WindowTransitions { get; } = new List<WindowTransition>();
        internal Dictionary<string, IniToken> Defines { get; } = new Dictionary<string, IniToken>();

        [AddedIn(SageGame.Bfme2)]
        public Fire Fire { get; internal set; }

        [AddedIn(SageGame.Bfme2)]
        public ArmySummaryDescription ArmySummaryDescription { get; internal set; }

        public IniDataContext(FileSystem fileSystem, SageGame game)
        {
            _fileSystem = fileSystem;
            _game = game;
        }

        public void LoadIniFiles(string folder)
        {
            foreach (var iniFile in _fileSystem.GetFiles(folder))
            {
                LoadIniFile(iniFile);
            }
        }

        public void LoadIniFile(string filePath, bool included = false)
        {
            LoadIniFile(_fileSystem.GetFile(filePath), included);
        }

        public void LoadIniFile(FileSystemEntry entry, bool included = false)
        {
            using (GameTrace.TraceDurationEvent($"LoadIniFile('{entry.FilePath}'"))
            {
                if (!included && !entry.FilePath.ToLowerInvariant().EndsWith(".ini"))
                {
                    return;
                }

                if (_alreadyLoaded.Contains(entry.FilePath))
                {
                    return;
                }

                string source;

                using (var stream = entry.Open())
                using (var reader = new StreamReader(stream, Encoding.ASCII))
                {
                    source = reader.ReadToEnd();
                }

                var parser = new IniParser(source, entry, this, _game);
                parser.ParseFile();

                _alreadyLoaded.Add(entry.FilePath);
            }
        }

        public string GetIniFileContent(string filePath)
        {
            var source = _fileSystem.GetFile(filePath);
            var streamReader = new StreamReader(source.Open());
            return streamReader.ReadToEnd();
        }
    }
}
