using OpenSage.Data.Ini;

namespace OpenSage.Gui
{
    [AddedIn(SageGame.Bfme)]
    public sealed class FontDefaultSetting : BaseAsset
    {
        internal static FontDefaultSetting Parse(IniParser parser)
        {
            var fontName = parser.ParseQuotedString();

            var result = parser.ParseTopLevelBlock(FieldParseTable);

            result.SetNameAndInstanceId("FontDefaultSetting", fontName);

            return result;
        }

        private static readonly IniParseTable<FontDefaultSetting> FieldParseTable = new IniParseTable<FontDefaultSetting>
        {
            { "Antialiased", (parser, x) => x.Antialiased = parser.ParseBoolean() }
        };

        public bool Antialiased { get; private set; }
    }
}
