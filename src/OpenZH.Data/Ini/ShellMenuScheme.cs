using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
{
    public sealed class ShellMenuScheme
    {
        internal static ShellMenuScheme Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<ShellMenuScheme> FieldParseTable = new IniParseTable<ShellMenuScheme>
        {
        };

        public string Name { get; private set; }
    }
}
