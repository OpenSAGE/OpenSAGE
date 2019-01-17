using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class EvaAnnounceClientCreateModuleData : ClientUpdateModuleData
    {
        internal static EvaAnnounceClientCreateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<EvaAnnounceClientCreateModuleData> FieldParseTable = new IniParseTable<EvaAnnounceClientCreateModuleData>
        {
            { "AnnouncementEventEnemy", (parser, x) => x.AnnouncementEventEnemy = parser.ParseAssetReference() },
            { "AnnouncementEventAlly", (parser, x) => x.AnnouncementEventAlly = parser.ParseAssetReference() },
            { "AnnouncementEventOwner", (parser, x) => x.AnnouncementEventOwner = parser.ParseAssetReference() },
            { "OnlyIfVisible", (parser, x) => x.OnlyIfVisible = parser.ParseBoolean() },
            { "CountAsFirstSightedAnnoucement", (parser, x) => x.CountAsFirstSightedAnnoucement = parser.ParseBoolean() },
            { "UseObjectsPosition", (parser, x) => x.UseObjectsPosition = parser.ParseBoolean() },
            { "CreateFakeRadarEvent", (parser, x) => x.CreateFakeRadarEvent = parser.ParseBoolean() },
            { "DelayBeforeAnnouncementMS", (parser, x) => x.DelayBeforeAnnouncementMS = parser.ParseInteger() }
        };

        public string AnnouncementEventEnemy { get; private set; }
        public string AnnouncementEventAlly { get; private set; }
        public string AnnouncementEventOwner { get; private set; }
        public bool OnlyIfVisible { get; private set; }
        public bool CountAsFirstSightedAnnoucement { get; private set; }
        public bool UseObjectsPosition { get; private set; }
        public bool CreateFakeRadarEvent { get; private set; }
        public int DelayBeforeAnnouncementMS { get; private set; }
    }
}
