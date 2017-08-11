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
        public List<EvaEvent> EvaEvents { get; } = new List<EvaEvent>();
        public List<FXList> FXLists { get; } = new List<FXList>();
        public GameData GameData { get; internal set; }
        public List<HeaderTemplate> HeaderTemplates { get; } = new List<HeaderTemplate>();
        public Language Language { get; internal set; }
        public List<ObjectDefinition> Objects { get; } = new List<ObjectDefinition>();

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
