using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class MapCache
    {
        internal static MapCache Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<MapCache> FieldParseTable = new IniParseTable<MapCache>
        {
            { "fileSize", (parser, x) => x.FileSize = parser.ParseInteger() },
            { "fileCRC", (parser, x) => x.FileCrc = parser.ParseLong() },
            { "timestampLo", (parser, x) => x.TimestampLo = parser.ParseLong() },
            { "timestampHi", (parser, x) => x.TimestampHi = parser.ParseLong() },
            { "isOfficial", (parser, x) => x.IsOfficial = parser.ParseBoolean() },
            { "isMultiplayer", (parser, x) => x.IsMultiplayer = parser.ParseBoolean() },
            { "numPlayers", (parser, x) => x.NumPlayers = parser.ParseInteger() },
            { "extentMin", (parser, x) => x.ExtentMin = Coord3D.Parse(parser) },
            { "extentMax", (parser, x) => x.ExtentMax = Coord3D.Parse(parser) },
            { "displayName", (parser, x) => x.DisplayName = parser.ParseIdentifier() },
            { "InitialCameraPosition", (parser, x) => x.InitialCameraPosition = Coord3D.Parse(parser) },
            { "Player_1_Start", (parser, x) => x.Player1Start = Coord3D.Parse(parser) },
            { "Player_2_Start", (parser, x) => x.Player2Start = Coord3D.Parse(parser) },
            { "Player_3_Start", (parser, x) => x.Player3Start = Coord3D.Parse(parser) },
            { "Player_4_Start", (parser, x) => x.Player4Start = Coord3D.Parse(parser) },
            { "Player_5_Start", (parser, x) => x.Player5Start = Coord3D.Parse(parser) },
            { "Player_6_Start", (parser, x) => x.Player6Start = Coord3D.Parse(parser) },
            { "Player_7_Start", (parser, x) => x.Player7Start = Coord3D.Parse(parser) },
            { "Player_8_Start", (parser, x) => x.Player8Start = Coord3D.Parse(parser) },
            { "techPosition", (parser, x) => x.TechPositions.Add(Coord3D.Parse(parser)) },
            { "supplyPosition", (parser, x) => x.SupplyPositions.Add(Coord3D.Parse(parser)) }
        };

        public string Name { get; private set; }

        public int FileSize { get; private set; }
        public long FileCrc { get; private set; }
        public long TimestampLo { get; private set; }
        public long TimestampHi { get; private set; }
        public bool IsOfficial { get; private set; }
        public bool IsMultiplayer { get; private set; }
        public int NumPlayers { get; private set; }
        public Coord3D ExtentMin { get; private set; }
        public Coord3D ExtentMax { get; private set; }
        public string DisplayName { get; private set; }
        public Coord3D InitialCameraPosition { get; private set; }
        public Coord3D Player1Start { get; private set; }
        public Coord3D Player2Start { get; private set; }
        public Coord3D Player3Start { get; private set; }
        public Coord3D Player4Start { get; private set; }
        public Coord3D Player5Start { get; private set; }
        public Coord3D Player6Start { get; private set; }
        public Coord3D Player7Start { get; private set; }
        public Coord3D Player8Start { get; private set; }
        public List<Coord3D> TechPositions { get; } = new List<Coord3D>();
        public List<Coord3D> SupplyPositions { get; } = new List<Coord3D>();
    }
}
