using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class DeletionUpdateBehavior : ObjectBehavior
    {
        internal static DeletionUpdateBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<DeletionUpdateBehavior> FieldParseTable = new IniParseTable<DeletionUpdateBehavior>
        {
            { "MinLifetime", (parser, x) => x.MinLifetime = parser.ParseInteger() },
            { "MaxLifetime", (parser, x) => x.MaxLifetime = parser.ParseInteger() }
        };

        public int MinLifetime { get; private set; }
        public int MaxLifetime { get; private set; }
    }
}
