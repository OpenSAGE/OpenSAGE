using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class SpecialEnemySenseUpdateModuleData : UpdateModuleData
    {
        internal static SpecialEnemySenseUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SpecialEnemySenseUpdateModuleData> FieldParseTable = new IniParseTable<SpecialEnemySenseUpdateModuleData>
        {
           { "SpecialEnemyFilter", (parser, x) => x.EnemyFilter = ObjectFilter.Parse(parser) },
           { "ScanRange", (parser, x) => x.ScanRange = parser.ParseInteger() },
           { "ScanInterval", (parser, x) => x.ScanInterval = parser.ParseInteger() },
        };

        public ObjectFilter EnemyFilter { get; internal set; }
        public int ScanRange { get; internal set; }
        public int ScanInterval { get; internal set; }
    }
}
