using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
{
    public sealed class CommandMap
    {
        internal static CommandMap Parse(IniParser parser)
        {
           return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<CommandMap> FieldParseTable = new IniParseTable<CommandMap>
        {
            { "Key", (parser, x) => x.Key = parser.ParseKey() },
            { "Transition", (parser, x) => x.Transition = parser.ParseKeyTransition() },
            { "Modifiers", (parser, x) => x.Modifiers = parser.ParseKeyModifiers() },
            { "UseableIn", (parser, x) => x.UseableIn = parser.ParseCommandMapUsabilityFlags() },
            { "Category", (parser, x) => x.Category = parser.ParseCommandMapCategory() },
            { "Description", (parser, x) => x.Description = parser.ParseIdentifier() },
            { "DisplayName", (parser, x) => x.DisplayName = parser.ParseIdentifier() }
        };

        public string Name { get; private set; }

        public Key Key { get; private set; }
        public KeyTransition Transition { get; private set; }
        public KeyModifiers Modifiers { get; private set; }
        public CommandMapUsabilityFlags UseableIn { get; private set; }
        public CommandMapCategory Category { get; private set; }
        public string Description { get; private set; }
        public string DisplayName { get; private set; }
    }
}
