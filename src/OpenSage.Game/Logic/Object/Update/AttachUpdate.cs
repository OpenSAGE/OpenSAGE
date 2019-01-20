using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class AttachUpdateModuleData : UpdateModuleData
    {
        internal static AttachUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AttachUpdateModuleData> FieldParseTable = new IniParseTable<AttachUpdateModuleData>
        {
            { "ObjectFilter", (parser, x) => x.ObjectFilter = ObjectFilter.Parse(parser) },
            { "ScanRange", (parser, x) => x.ScanRange = parser.ParseInteger() }, 
            { "ParentStatus", (parser, x) => x.ParentStatus = parser.ParseEnum<ObjectStatus>() },
            { "ParentOwnerAttachmentEvaEvent", (parser, x) => x.ParentOwnerAttachmentEvaEvent = parser.ParseAssetReference() },
            { "ParentEnemyAttachmentEvaEvent", (parser, x) => x.ParentEnemyAttachmentEvaEvent = parser.ParseAssetReference() },
            { "ParentOwnerDiedEvaEvent", (parser, x) => x.ParentOwnerDiedEvaEvent = parser.ParseAssetReference() },
            { "AlwaysTeleport", (parser, x) => x.AlwaysTeleport = parser.ParseBoolean() },
            { "AnchorToTopOfGeometry", (parser, x) => x.AnchorToTopOfGeometry = parser.ParseBoolean() },
        };

        public ObjectFilter ObjectFilter { get; private set; }
        public int ScanRange { get; private set; }
        public ObjectStatus ParentStatus { get; private set; }
        public string ParentOwnerAttachmentEvaEvent { get; private set; }
        public string ParentEnemyAttachmentEvaEvent { get; private set; }
        public string ParentOwnerDiedEvaEvent { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public bool AlwaysTeleport { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public bool AnchorToTopOfGeometry { get; private set; }
    }
}
