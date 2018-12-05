using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class TerrainResourceBehaviorModuleData : UpgradeModuleData
    {
        internal static TerrainResourceBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<TerrainResourceBehaviorModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<TerrainResourceBehaviorModuleData>
            {
                { "Radius", (parser, x) => x.Radius = parser.ParseInteger() },
                { "MaxIncome", (parser, x) => x.MaxIncome = parser.ParseInteger() },
                { "IncomeInterval", (parser, x) => x.IncomeInterval = parser.ParseInteger() },
                { "HighPriority", (parser, x) => x.HighPriority = parser.ParseBoolean() },
                { "Visible", (parser, x) => x.Visible = parser.ParseBoolean() }
            });

        public int Radius { get; private set; }
        public int MaxIncome { get; private set; }
        public int IncomeInterval { get; private set; }
        public bool HighPriority { get; private set; }
        public bool Visible { get; private set; }
    }
}
