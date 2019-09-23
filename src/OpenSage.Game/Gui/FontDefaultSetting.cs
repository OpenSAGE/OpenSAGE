using OpenSage.Data.Ini;

namespace OpenSage.Gui
{
    [AddedIn(SageGame.Bfme)]
    public sealed class FontDefaultSetting
    {
        internal static FontDefaultSetting Parse(IniParser parser)
        {
            var fontName = parser.ParseQuotedString();

            var result = parser.ParseTopLevelBlock(FieldParseTable);

            result.Name = fontName;

            return result;
        }

        private static readonly IniParseTable<FontDefaultSetting> FieldParseTable = new IniParseTable<FontDefaultSetting>
        {
            { "Antialiased", (parser, x) => x.Antialiased = parser.ParseBoolean() }
        };

        public string Name { get; private set; }

        public bool Antialiased { get; private set; }
    }
}
