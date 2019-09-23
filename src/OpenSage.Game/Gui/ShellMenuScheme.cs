using OpenSage.Data.Ini;

namespace OpenSage.Gui
{
    public sealed class ShellMenuScheme
    {
        internal static ShellMenuScheme Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<ShellMenuScheme> FieldParseTable = new IniParseTable<ShellMenuScheme>
        {
        };

        public string Name { get; private set; }
    }
}
