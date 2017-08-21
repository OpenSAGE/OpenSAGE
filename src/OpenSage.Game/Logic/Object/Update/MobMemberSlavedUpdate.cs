using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class MobMemberSlavedUpdate : ObjectBehavior
    {
        internal static MobMemberSlavedUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<MobMemberSlavedUpdate> FieldParseTable = new IniParseTable<MobMemberSlavedUpdate>
        {
            { "MustCatchUpRadius", (parser, x) => x.MustCatchUpRadius = parser.ParseInteger() },
            { "NoNeedToCatchUpRadius", (parser, x) => x.NoNeedToCatchUpRadius = parser.ParseInteger() },
            { "Squirrelliness", (parser, x) => x.Squirrelliness = parser.ParseFloat() },
            { "CatchUpCrisisBailTime", (parser, x) => x.CatchUpCrisisBailTime = parser.ParseInteger() },
        };

        public int MustCatchUpRadius { get; private set; }
        public int NoNeedToCatchUpRadius { get; private set; }
        public float Squirrelliness { get; private set; }
        public int CatchUpCrisisBailTime { get; private set; }
    }
}
