using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenSage.Data.Ini.Parser;
using OpenSage.Logic.Object;

namespace OpenSage.Data.Ini
{
    public sealed class IniDataContext
    {
        private readonly FileSystem _fileSystem;

        // TODO: Remove this once we can load all INI files upfront.
        private readonly List<string> _alreadyLoaded = new List<string>();

        public AIData AIData { get; internal set; }
        public List<AmbientStream> AmbientStreams { get; } = new List<AmbientStream>();
        public List<Animation> Animations { get; } = new List<Animation>();
        public AnimationSoundClientBehaviorGlobalSetting AnimationSoundClientBehaviorGlobalSetting { get; internal set; }
        public List<Armor> Armors { get; } = new List<Armor>();
        public List<AudioEvent> AudioEvents { get; } = new List<AudioEvent>();
        public AudioSettings AudioSettings { get; internal set; }
        public List<BannerType> BannerTypes { get; } = new List<BannerType>();
        public BannerUI BannerUI { get; internal set; }
        public List<BenchProfile> BenchProfiles { get; } = new List<BenchProfile>();
        public List<Bridge> Bridges { get; } = new List<Bridge>();
        public List<Campaign> Campaigns { get; } = new List<Campaign>();
        public ChallengeGenerals ChallengeGenerals { get; internal set; }
        public List<CommandButton> CommandButtons { get; } = new List<CommandButton>();
        public List<CommandMap> CommandMaps { get; } = new List<CommandMap>();
        public List<CommandSet> CommandSets { get; } = new List<CommandSet>();
        public List<ControlBarResizer> ControlBarResizers { get; } = new List<ControlBarResizer>();
        public List<ControlBarScheme> ControlBarSchemes { get; } = new List<ControlBarScheme>();
        public List<CrateData> CrateDatas { get; } = new List<CrateData>();
        public Credits Credits { get; internal set; }
        public List<DamageFX> DamageFXs { get; } = new List<DamageFX>();
        public List<DialogEvent> DialogEvents { get; } = new List<DialogEvent>();
        public DrawGroupInfo DrawGroupInfo { get; internal set; }
        public List<DynamicGameLod> DynamicGameLods { get; } = new List<DynamicGameLod>();
        public List<EmotionNugget> EmotionNuggets { get; } = new List<EmotionNugget>();
        public Environment Environment { get; } = new Environment();
        public List<EvaEvent> EvaEvents { get; } = new List<EvaEvent>();
        public List<ExperienceLevel> ExperienceLevels { get; } = new List<ExperienceLevel>();
        public List<ExperienceScalarTable> ExperienceScalarTables { get; } = new List<ExperienceScalarTable>();
        public List<FactionVictoryData> FactionVictoryDatas { get; } = new List<FactionVictoryData>();
        public List<FontDefaultSetting> FontDefaultSettings { get; } = new List<FontDefaultSetting>();
        public List<FontSubstitution> FontSubstitutions { get; } = new List<FontSubstitution>();
        public List<FXList> FXLists { get; } = new List<FXList>();
        public GameData GameData { get; internal set; }
        public List<HeaderTemplate> HeaderTemplates { get; } = new List<HeaderTemplate>();
        public InGameUI InGameUI { get; internal set; }
        public Language Language { get; internal set; }
        public List<LivingWorldCampaign> LivingWorldCampaigns { get; } = new List<LivingWorldCampaign>();
        public List<LivingWorldPlayerArmy> LivingWorldPlayerArmies { get; } = new List<LivingWorldPlayerArmy>();
        public List<LivingWorldRegionCampaign> LivingWorldRegionCampaigns { get; } = new List<LivingWorldRegionCampaign>();
        public List<Locomotor> Locomotors { get; } = new List<Locomotor>();
        public List<LodPreset> LodPresets { get; } = new List<LodPreset>();
        public List<MapCache> MapCaches { get; } = new List<MapCache>();
        public List<MappedImage> MappedImages { get; } = new List<MappedImage>();
        public MiscAudio MiscAudio { get; internal set; }
        public MiscEvaData MiscEvaData { get; internal set; }
        public List<ModifierList> ModifierLists { get; } = new List<ModifierList>();
        public List<MouseCursor> MouseCursors { get; } = new List<MouseCursor>();
        public MouseData MouseData { get; internal set; }
        public List<MultiplayerColor> MultiplayerColors { get; } = new List<MultiplayerColor>();
        public MultiplayerSettings MultiplayerSettings { get; internal set; }
        public List<MultiplayerStartingMoneyChoice> MultiplayerStartingMoneyChoices { get; } = new List<MultiplayerStartingMoneyChoice>();
        public List<MusicTrack> MusicTracks { get; } = new List<MusicTrack>();
        public List<ObjectDefinition> Objects { get; } = new List<ObjectDefinition>();
        public List<ObjectCreationList> ObjectCreationLists { get; } = new List<ObjectCreationList>();
        public OnlineChatColors OnlineChatColors { get; internal set; }
        public List<ParticleSystemDefinition> ParticleSystems { get; } = new List<ParticleSystemDefinition>();
        public List<PlayerTemplate> PlayerTemplates { get; } = new List<PlayerTemplate>();
        public List<Rank> Ranks { get; } = new List<Rank>();
        public List<Road> Roads { get; } = new List<Road>();
        public int ReallyLowMHz { get; internal set; }
        public List<Science> Sciences { get; } = new List<Science>();
        public List<ShellMenuScheme> ShellMenuSchemes { get; } = new List<ShellMenuScheme>();
        public List<SkyboxTextureSet> SkyboxTextureSets { get; } = new List<SkyboxTextureSet>();
        public List<SpecialPower> SpecialPowers { get; } = new List<SpecialPower>();
        public List<StaticGameLod> StaticGameLods { get; } = new List<StaticGameLod>();
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
        public List<WebpageUrl> WebpageUrls { get; } = new List<WebpageUrl>();
        public List<WindowTransition> WindowTransitions { get; } = new List<WindowTransition>();

        internal Dictionary<string, IniToken> Defines { get; } = new Dictionary<string, IniToken>();

        public IniDataContext(FileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void LoadIniFiles(string folder)
        {
            foreach (var iniFile in _fileSystem.GetFiles(folder))
            {
                LoadIniFile(iniFile);
            }
        }

        public void LoadIniFile(string filePath)
        {
            LoadIniFile(_fileSystem.GetFile(filePath));
        }

        public void LoadIniFile(FileSystemEntry entry)
        {
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

            var parser = new IniParser(source, entry, this);
            parser.ParseFile();

            _alreadyLoaded.Add(entry.FilePath);
        }
    }
}
