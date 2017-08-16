using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenSage.Data.Ini.Parser;
using OpenSage.Logic.Object;

namespace OpenSage.Data.Ini
{
    public sealed class IniDataContext
    {
        public AIData AIData { get; internal set; }
        public List<Animation> Animations { get; } = new List<Animation>();
        public List<Armor> Armors { get; } = new List<Armor>();
        public List<AudioEvent> AudioEvents { get; } = new List<AudioEvent>();
        public AudioSettings AudioSettings { get; internal set; }
        public List<BenchProfile> BenchProfiles { get; } = new List<BenchProfile>();
        public List<Bridge> Bridges { get; } = new List<Bridge>();
        public List<Campaign> Campaigns { get; } = new List<Campaign>();
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
        public List<EvaEvent> EvaEvents { get; } = new List<EvaEvent>();
        public List<FXList> FXLists { get; } = new List<FXList>();
        public GameData GameData { get; internal set; }
        public List<HeaderTemplate> HeaderTemplates { get; } = new List<HeaderTemplate>();
        public InGameUI InGameUI { get; internal set; }
        public Language Language { get; internal set; }
        public List<Locomotor> Locomotors { get; } = new List<Locomotor>();
        public List<LodPreset> LodPresets { get; } = new List<LodPreset>();
        public List<MappedImage> MappedImages { get; } = new List<MappedImage>();
        public MiscAudio MiscAudio { get; internal set; }
        public List<MouseCursor> MouseCursors { get; } = new List<MouseCursor>();
        public MouseData MouseData { get; internal set; }
        public List<MultiplayerColor> MultiplayerColors { get; } = new List<MultiplayerColor>();
        public MultiplayerSettings MultiplayerSettings { get; internal set; }
        public List<MusicTrack> MusicTracks { get; } = new List<MusicTrack>();
        public List<ObjectDefinition> Objects { get; } = new List<ObjectDefinition>();
        public List<ObjectReskin> ObjectReskins { get; } = new List<ObjectReskin>();
        public List<ObjectCreationList> ObjectCreationLists { get; } = new List<ObjectCreationList>();
        public OnlineChatColors OnlineChatColors { get; internal set; }
        public List<ParticleSystem> ParticleSystems { get; } = new List<ParticleSystem>();
        public List<PlayerTemplate> PlayerTemplates { get; } = new List<PlayerTemplate>();
        public List<Rank> Ranks { get; } = new List<Rank>();
        public List<Road> Roads { get; } = new List<Road>();
        public int ReallyLowMHz { get; internal set; }
        public List<Science> Sciences { get; } = new List<Science>();
        public List<ShellMenuScheme> ShellMenuSchemes { get; } = new List<ShellMenuScheme>();
        public List<SpecialPower> SpecialPowers { get; } = new List<SpecialPower>();
        public List<StaticGameLod> StaticGameLods { get; } = new List<StaticGameLod>();
        public List<Terrain> Terrains { get; } = new List<Terrain>();
        public List<Upgrade> Upgrades { get; } = new List<Upgrade>();
        public List<Video> Videos { get; } = new List<Video>();
        public List<WaterSet> WaterSets { get; } = new List<WaterSet>();
        public WaterTransparency WaterTransparency { get; internal set; }
        public List<Weapon> Weapons { get; } = new List<Weapon>();
        public List<WebpageUrl> WebpageUrls { get; } = new List<WebpageUrl>();
        public List<WindowTransition> WindowTransitions { get; } = new List<WindowTransition>();

        public void LoadIniFile(Stream stream, string fileName)
        {
            using (var reader = new StreamReader(stream, Encoding.ASCII))
            {
                var source = reader.ReadToEnd();
                var parser = new IniParser(source, fileName);

                parser.ParseFile(this);
            }
        }
    }
}
