using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.BattleForMiddleEarth)]
    public sealed class FactionVictoryData
    {
        internal static FactionVictoryData Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<FactionVictoryData> FieldParseTable = new IniParseTable<FactionVictoryData>
        {
            { "AllyDeathScaleFactor", (parser, x) => x.AllyDeathScaleFactor = parser.ParseFloat() },
            { "EnemyKillScaleFactor", (parser, x) => x.EnemyKillScaleFactor = parser.ParseFloat() },
            { "VictoryThreshold", (parser, x) => x.VictoryThreshold = parser.ParseFloat() },
            { "MajorUnitValue", (parser, x) => x.MajorUnitValue = parser.ParseFloat() },
            { "MapToCellVictoryRatio", (parser, x) => x.MapToCellVictoryRatio = parser.ParseFloat() },
        };

        public string Name { get; private set; }

        public float AllyDeathScaleFactor { get; private set; }
        public float EnemyKillScaleFactor { get; private set; }
        public float VictoryThreshold { get; private set; }
        public float MajorUnitValue { get; private set; }
        public float MapToCellVictoryRatio { get; private set; }
    }
}
