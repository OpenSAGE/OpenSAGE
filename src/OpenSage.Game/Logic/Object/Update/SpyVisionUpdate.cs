using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class SpyVisionUpdateModuleData : UpdateModuleData
    {
        internal static SpyVisionUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SpyVisionUpdateModuleData> FieldParseTable = new IniParseTable<SpyVisionUpdateModuleData>
        {
            { "SpyOnKindof", (parser, x) => x.SpyOnKindof = parser.ParseEnum<ObjectKinds>() },
            { "NeedsUpgrade", (parser, x) => x.NeedsUpgrade = parser.ParseBoolean() },
            { "SelfPowered", (parser, x) => x.SelfPowered = parser.ParseBoolean() },
            { "SelfPoweredDuration", (parser, x) => x.SelfPoweredDuration = parser.ParseInteger() },
            { "SelfPoweredInterval", (parser, x) => x.SelfPoweredInterval = parser.ParseInteger() },
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReference() },
        };

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public ObjectKinds SpyOnKindof { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool NeedsUpgrade { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool SelfPowered { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int SelfPoweredDuration { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int SelfPoweredInterval { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string TriggeredBy { get; private set; }
    }
}
