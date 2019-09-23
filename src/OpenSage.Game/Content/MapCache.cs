using System.Collections.Generic;
using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.Content
{
    public sealed class MapCache
    {
        internal static MapCache Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = IniParser.ToAsciiString(name),
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
            { "extentMin", (parser, x) => x.ExtentMin = parser.ParseVector3() },
            { "extentMax", (parser, x) => x.ExtentMax = parser.ParseVector3() },
            { "nameLookupTag", (parser, x) => x.NameLookupTag = parser.GetNextTokenOptional()?.Text },
            { "displayName", (parser, x) => x.DisplayName = parser.ParseUnicodeString() },
            { "InitialCameraPosition", (parser, x) => x.InitialCameraPosition = parser.ParseVector3() },
            { "Player_1_Start", (parser, x) => x.Player1Start = parser.ParseVector3() },
            { "Player_2_Start", (parser, x) => x.Player2Start = parser.ParseVector3() },
            { "Player_3_Start", (parser, x) => x.Player3Start = parser.ParseVector3() },
            { "Player_4_Start", (parser, x) => x.Player4Start = parser.ParseVector3() },
            { "Player_5_Start", (parser, x) => x.Player5Start = parser.ParseVector3() },
            { "Player_6_Start", (parser, x) => x.Player6Start = parser.ParseVector3() },
            { "Player_7_Start", (parser, x) => x.Player7Start = parser.ParseVector3() },
            { "Player_8_Start", (parser, x) => x.Player8Start = parser.ParseVector3() },
            { "techPosition", (parser, x) => x.TechPositions.Add(parser.ParseVector3()) },
            { "supplyPosition", (parser, x) => x.SupplyPositions.Add(parser.ParseVector3()) },
            { "isScenarioMP", (parser, x) => x.IsScenarioMP = parser.ParseBoolean() },
            { "description", (parser, x) => x.Description = parser.ParseUnicodeString() },
            { "PlayerPosition", (parser, x) => x.PlayerPositions.Add(PlayerPosition.Parse(parser)) },
        };

        public string Name { get; private set; }

        public int FileSize { get; private set; }
        public long FileCrc { get; private set; }
        public long TimestampLo { get; private set; }
        public long TimestampHi { get; private set; }
        public bool IsOfficial { get; private set; }
        public bool IsMultiplayer { get; private set; }
        public int NumPlayers { get; private set; }
        public Vector3 ExtentMin { get; private set; }
        public Vector3 ExtentMax { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string NameLookupTag { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool IsScenarioMP { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string Description { get; private set; }

        public string DisplayName { get; private set; }
        public Vector3 InitialCameraPosition { get; private set; }
        public Vector3 Player1Start { get; private set; }
        public Vector3 Player2Start { get; private set; }
        public Vector3 Player3Start { get; private set; }
        public Vector3 Player4Start { get; private set; }
        public Vector3 Player5Start { get; private set; }
        public Vector3 Player6Start { get; private set; }
        public Vector3 Player7Start { get; private set; }
        public Vector3 Player8Start { get; private set; }
        public List<Vector3> TechPositions { get; } = new List<Vector3>();
        public List<Vector3> SupplyPositions { get; } = new List<Vector3>();

        [AddedIn(SageGame.Bfme2Rotwk)]
        public List<PlayerPosition> PlayerPositions { get; } = new List<PlayerPosition>();

        public string GetNameKey()
        {
            var result = "Unnamed";

            if (NameLookupTag != null)
            {
                result = NameLookupTag;
            }
            else if (DisplayName != null)
            {
                result = DisplayName.Replace("$", "");
            }

            return result;
        }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public class PlayerPosition
        {
            internal static PlayerPosition Parse(IniParser parser)
            {
                return parser.ParseIndexedBlock(
                    (x, id) => x.ID = id,
                    FieldParseTable);
            }

            private static readonly IniParseTable<PlayerPosition> FieldParseTable = new IniParseTable<PlayerPosition>
            {
                { "Human", (parser, x) => x.Human = parser.ParseBoolean() },
                { "Computer", (parser, x) => x.Computer = parser.ParseBoolean() },
                { "LoadAIScripts", (parser, x) => x.LoadAIScripts = parser.ParseBoolean() },
                { "ForcePlayerTeam", (parser, x) => x.ForcePlayerTeam = parser.ParseInteger() },
            };

            public int ID { get; private set; }

            public bool Human { get; private set; }
            public bool Computer { get; private set; }
            public bool LoadAIScripts { get; private set; }
            public int ForcePlayerTeam { get; private set; }
        }
    }
}
