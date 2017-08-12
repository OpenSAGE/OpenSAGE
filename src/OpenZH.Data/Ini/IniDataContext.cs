using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
{
    public sealed class IniDataContext
    {
        public AIData AIData { get; internal set; }
        public List<Animation> Animations { get; } = new List<Animation>();
        public List<Armor> Armors { get; } = new List<Armor>();
        public AudioSettings AudioSettings { get; internal set; }
        public List<BenchProfile> BenchProfiles { get; } = new List<BenchProfile>();
        public List<Campaign> Campaigns { get; } = new List<Campaign>();
        public List<CommandButton> CommandButtons { get; } = new List<CommandButton>();
        public List<CommandMap> CommandMaps { get; } = new List<CommandMap>();
        public List<CommandSet> CommandSets { get; } = new List<CommandSet>();
        public List<ControlBarResizer> ControlBarResizers { get; } = new List<ControlBarResizer>();
        public List<ControlBarScheme> ControlBarSchemes { get; } = new List<ControlBarScheme>();
        public List<CrateData> CrateDatas { get; } = new List<CrateData>();
        public Credits Credits { get; internal set; }
        public List<DamageFX> DamageFXs { get; } = new List<DamageFX>();
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
        public MiscAudio MiscAudio { get; internal set; }
        public List<MouseCursor> MouseCursors { get; } = new List<MouseCursor>();
        public MouseData MouseData { get; internal set; }
        public List<MultiplayerColor> MultiplayerColors { get; } = new List<MultiplayerColor>();
        public MultiplayerSettings MultiplayerSettings { get; internal set; }
        public List<ObjectDefinition> Objects { get; } = new List<ObjectDefinition>();
        public OnlineChatColors OnlineChatColors { get; internal set; }
        public int ReallyLowMHz { get; internal set; }
        public List<StaticGameLod> StaticGameLods { get; } = new List<StaticGameLod>();

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
