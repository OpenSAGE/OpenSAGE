using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
{
    public sealed class OnlineChatColors
    {
        internal static OnlineChatColors Parse(IniParser parser)
        {
            return parser.ParseTopLevelBlock(FieldParseTable);
        }

        private static readonly IniParseTable<OnlineChatColors> FieldParseTable = new IniParseTable<OnlineChatColors>
        {
            { "Default", (parser, x) => x.Default = IniColorRgb.Parse(parser) },
            { "CurrentRoom", (parser, x) => x.CurrentRoom = IniColorRgb.Parse(parser) },
            { "ChatRoom", (parser, x) => x.ChatRoom = IniColorRgb.Parse(parser) },
            { "Game", (parser, x) => x.Game = IniColorRgb.Parse(parser) },
            { "GameFull", (parser, x) => x.GameFull = IniColorRgb.Parse(parser) },
            { "GameCRCMismatch", (parser, x) => x.GameCrcMismatch = IniColorRgb.Parse(parser) },
            { "PlayerNormal", (parser, x) => x.PlayerNormal = IniColorRgb.Parse(parser) },
            { "PlayerOwner", (parser, x) => x.PlayerOwner = IniColorRgb.Parse(parser) },
            { "PlayerBuddy", (parser, x) => x.PlayerBuddy = IniColorRgb.Parse(parser) },
            { "PlayerSelf", (parser, x) => x.PlayerSelf = IniColorRgb.Parse(parser) },
            { "PlayerIgnored", (parser, x) => x.PlayerIgnored = IniColorRgb.Parse(parser) },
            { "ChatNormal", (parser, x) => x.ChatNormal = IniColorRgb.Parse(parser) },
            { "ChatEmote", (parser, x) => x.ChatEmote = IniColorRgb.Parse(parser) },
            { "ChatOwner", (parser, x) => x.ChatOwner = IniColorRgb.Parse(parser) },
            { "ChatOwnerEmote", (parser, x) => x.ChatOwnerEmote = IniColorRgb.Parse(parser) },
            { "ChatPriv", (parser, x) => x.ChatPriv = IniColorRgb.Parse(parser) },
            { "ChatPrivEmote", (parser, x) => x.ChatPrivEmote = IniColorRgb.Parse(parser) },
            { "ChatPrivOwner", (parser, x) => x.ChatPrivOwner = IniColorRgb.Parse(parser) },
            { "ChatPrivOwnerEmote", (parser, x) => x.ChatPrivOwnerEmote = IniColorRgb.Parse(parser) },
            { "ChatBuddy", (parser, x) => x.ChatBuddy = IniColorRgb.Parse(parser) },
            { "ChatSelf", (parser, x) => x.ChatSelf = IniColorRgb.Parse(parser) },
            { "AcceptTrue", (parser, x) => x.AcceptTrue = IniColorRgb.Parse(parser) },
            { "AcceptFalse", (parser, x) => x.AcceptFalse = IniColorRgb.Parse(parser) },
            { "MapSelected", (parser, x) => x.MapSelected = IniColorRgb.Parse(parser) },
            { "MapUnselected", (parser, x) => x.MapUnselected = IniColorRgb.Parse(parser) },
            { "MOTD", (parser, x) => x.Motd = IniColorRgb.Parse(parser) },
            { "MOTDHeading", (parser, x) => x.MotdHeading = IniColorRgb.Parse(parser) },
        };

        public IniColorRgb Default { get; private set; }
        public IniColorRgb CurrentRoom { get; private set; }
        public IniColorRgb ChatRoom { get; private set; }
        public IniColorRgb Game { get; private set; }
        public IniColorRgb GameFull { get; private set; }
        public IniColorRgb GameCrcMismatch { get; private set; }
        public IniColorRgb PlayerNormal { get; private set; }
        public IniColorRgb PlayerOwner { get; private set; }
        public IniColorRgb PlayerBuddy { get; private set; }
        public IniColorRgb PlayerSelf { get; private set; }
        public IniColorRgb PlayerIgnored { get; private set; }
        public IniColorRgb ChatNormal { get; private set; }
        public IniColorRgb ChatEmote { get; private set; }
        public IniColorRgb ChatOwner { get; private set; }
        public IniColorRgb ChatOwnerEmote { get; private set; }
        public IniColorRgb ChatPriv { get; private set; }
        public IniColorRgb ChatPrivEmote { get; private set; }
        public IniColorRgb ChatPrivOwner { get; private set; }
        public IniColorRgb ChatPrivOwnerEmote { get; private set; }
        public IniColorRgb ChatBuddy { get; private set; }
        public IniColorRgb ChatSelf { get; private set; }
        public IniColorRgb AcceptTrue { get; private set; }
        public IniColorRgb AcceptFalse { get; private set; }
        public IniColorRgb MapSelected { get; private set; }
        public IniColorRgb MapUnselected { get; private set; }
        public IniColorRgb Motd { get; private set; }
        public IniColorRgb MotdHeading { get; private set; }
    }
}
