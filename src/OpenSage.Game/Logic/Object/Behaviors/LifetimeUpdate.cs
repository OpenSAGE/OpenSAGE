using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class LifetimeUpdate : ObjectBehavior
    {
        internal static LifetimeUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LifetimeUpdate> FieldParseTable = new IniParseTable<LifetimeUpdate>
        {
            { "MinLifetime", (parser, x) => x.MinLifetime = parser.ParseInteger() },
            { "MaxLifetime", (parser, x) => x.MaxLifetime = parser.ParseInteger() }
        };

        public int MinLifetime { get; private set; }
        public int MaxLifetime { get; private set; }
    }
}
