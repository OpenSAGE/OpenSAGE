using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class DeletionUpdate : ObjectBehavior
    {
        internal static DeletionUpdate Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<DeletionUpdate> FieldParseTable = new IniParseTable<DeletionUpdate>
        {
            { "MinLifetime", (parser, x) => x.MinLifetime = parser.ParseInteger() },
            { "MaxLifetime", (parser, x) => x.MaxLifetime = parser.ParseInteger() }
        };

        public int MinLifetime { get; private set; }
        public int MaxLifetime { get; private set; }
    }
}
