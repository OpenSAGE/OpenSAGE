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
        public List<HeaderTemplate> HeaderTemplates { get; } = new List<HeaderTemplate>();
        public Language Language { get; internal set; }

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
