using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class BuildingBehaviorModuleData : BehaviorModuleData
    {
        internal static BuildingBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BuildingBehaviorModuleData> FieldParseTable = new IniParseTable<BuildingBehaviorModuleData>
        {
            { "NightWindowName", (parser, x) => x.NightWindowName = parser.ParseString() },
            { "FireWindowName", (parser, x) => x.FireWindowName = parser.ParseString() },
            { "GlowWindowName", (parser, x) => x.GlowWindowName = parser.ParseString() },
            { "FireName", (parser, x) => x.FireNames.Add(parser.ParseString()) },
        };

        public string NightWindowName { get; private set; }
        public string FireWindowName { get; private set; }
        public string GlowWindowName { get; private set; }
        public List<string> FireNames { get; private set; } = new List<string>();
    }
}
