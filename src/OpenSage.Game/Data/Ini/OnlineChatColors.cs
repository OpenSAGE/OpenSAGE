using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Data.Ini
{
    public sealed class OnlineChatColors
    {
        internal static OnlineChatColors Parse(IniParser parser)
        {
            return parser.ParseTopLevelBlock(FieldParseTable);
        }

        private static readonly IniParseTable<OnlineChatColors> FieldParseTable = new IniParseTable<OnlineChatColors>
        {
            { "Default", (parser, x) => x.Default = parser.ParseColorRgb() },
            { "CurrentRoom", (parser, x) => x.CurrentRoom = parser.ParseColorRgb() },
            { "ChatRoom", (parser, x) => x.ChatRoom = parser.ParseColorRgb() },
            { "Game", (parser, x) => x.Game = parser.ParseColorRgb() },
            { "GameFull", (parser, x) => x.GameFull = parser.ParseColorRgb() },
            { "GameCRCMismatch", (parser, x) => x.GameCrcMismatch = parser.ParseColorRgb() },
            { "PlayerNormal", (parser, x) => x.PlayerNormal = parser.ParseColorRgb() },
            { "PlayerOwner", (parser, x) => x.PlayerOwner = parser.ParseColorRgb() },
            { "PlayerBuddy", (parser, x) => x.PlayerBuddy = parser.ParseColorRgb() },
            { "PlayerSelf", (parser, x) => x.PlayerSelf = parser.ParseColorRgb() },
            { "PlayerIgnored", (parser, x) => x.PlayerIgnored = parser.ParseColorRgb() },
            { "ChatNormal", (parser, x) => x.ChatNormal = parser.ParseColorRgb() },
            { "ChatEmote", (parser, x) => x.ChatEmote = parser.ParseColorRgb() },
            { "ChatOwner", (parser, x) => x.ChatOwner = parser.ParseColorRgb() },
            { "ChatOwnerEmote", (parser, x) => x.ChatOwnerEmote = parser.ParseColorRgb() },
            { "ChatPriv", (parser, x) => x.ChatPriv = parser.ParseColorRgb() },
            { "ChatPrivEmote", (parser, x) => x.ChatPrivEmote = parser.ParseColorRgb() },
            { "ChatPrivOwner", (parser, x) => x.ChatPrivOwner = parser.ParseColorRgb() },
            { "ChatPrivOwnerEmote", (parser, x) => x.ChatPrivOwnerEmote = parser.ParseColorRgb() },
            { "ChatBuddy", (parser, x) => x.ChatBuddy = parser.ParseColorRgb() },
            { "ChatSelf", (parser, x) => x.ChatSelf = parser.ParseColorRgb() },
            { "AcceptTrue", (parser, x) => x.AcceptTrue = parser.ParseColorRgb() },
            { "AcceptFalse", (parser, x) => x.AcceptFalse = parser.ParseColorRgb() },
            { "MapSelected", (parser, x) => x.MapSelected = parser.ParseColorRgb() },
            { "MapUnselected", (parser, x) => x.MapUnselected = parser.ParseColorRgb() },
            { "MOTD", (parser, x) => x.Motd = parser.ParseColorRgb() },
            { "MOTDHeading", (parser, x) => x.MotdHeading = parser.ParseColorRgb() },
            { "OfflinePlayerBuddy", (parser, x) => x.OfflinePlayerBuddy = parser.ParseColorRgb() },
            { "OfflinePlayerIgnored", (parser, x) => x.OfflinePlayerIgnored = parser.ParseColorRgb() },
        };

        public ColorRgb Default { get; private set; }
        public ColorRgb CurrentRoom { get; private set; }
        public ColorRgb ChatRoom { get; private set; }
        public ColorRgb Game { get; private set; }
        public ColorRgb GameFull { get; private set; }
        public ColorRgb GameCrcMismatch { get; private set; }
        public ColorRgb PlayerNormal { get; private set; }
        public ColorRgb PlayerOwner { get; private set; }
        public ColorRgb PlayerBuddy { get; private set; }
        public ColorRgb PlayerSelf { get; private set; }
        public ColorRgb PlayerIgnored { get; private set; }
        public ColorRgb ChatNormal { get; private set; }
        public ColorRgb ChatEmote { get; private set; }
        public ColorRgb ChatOwner { get; private set; }
        public ColorRgb ChatOwnerEmote { get; private set; }
        public ColorRgb ChatPriv { get; private set; }
        public ColorRgb ChatPrivEmote { get; private set; }
        public ColorRgb ChatPrivOwner { get; private set; }
        public ColorRgb ChatPrivOwnerEmote { get; private set; }
        public ColorRgb ChatBuddy { get; private set; }
        public ColorRgb ChatSelf { get; private set; }
        public ColorRgb AcceptTrue { get; private set; }
        public ColorRgb AcceptFalse { get; private set; }
        public ColorRgb MapSelected { get; private set; }
        public ColorRgb MapUnselected { get; private set; }
        public ColorRgb Motd { get; private set; }
        public ColorRgb MotdHeading { get; private set; }
        [AddedIn(SageGame.Bfme)]
        public ColorRgb OfflinePlayerBuddy { get; private set; }
        [AddedIn(SageGame.Bfme)]
        public ColorRgb OfflinePlayerIgnored { get; private set; }
    }
}
