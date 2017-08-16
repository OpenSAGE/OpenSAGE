using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class LifetimeUpdateBehavior : ObjectBehavior
    {
        internal static LifetimeUpdateBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LifetimeUpdateBehavior> FieldParseTable = new IniParseTable<LifetimeUpdateBehavior>
        {
            { "MinLifetime", (parser, x) => x.MinLifetime = parser.ParseInteger() },
            { "MaxLifetime", (parser, x) => x.MaxLifetime = parser.ParseInteger() }
        };

        public int MinLifetime { get; private set; }
        public int MaxLifetime { get; private set; }
    }
}
