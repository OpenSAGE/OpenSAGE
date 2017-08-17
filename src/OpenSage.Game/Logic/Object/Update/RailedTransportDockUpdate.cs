using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class RailedTransportDockUpdate : ObjectBehavior
    {
        internal static RailedTransportDockUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<RailedTransportDockUpdate> FieldParseTable = new IniParseTable<RailedTransportDockUpdate>
        {
            { "NumberApproachPositions", (parser, x) => x.NumberApproachPositions = parser.ParseInteger() },
            { "PullInsideDuration", (parser, x) => x.PullInsideDuration = parser.ParseInteger() },
            { "PushOutsideDuration", (parser, x) => x.PushOutsideDuration = parser.ParseInteger() }
        };

        public int NumberApproachPositions { get; private set; }
        public int PullInsideDuration { get; private set; }
        public int PushOutsideDuration { get; private set; }
    }
}
