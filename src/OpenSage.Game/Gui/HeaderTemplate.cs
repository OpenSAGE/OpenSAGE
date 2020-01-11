using OpenSage.Data.Ini;

namespace OpenSage.Gui
{
    public sealed class HeaderTemplate : BaseAsset
    {
        internal static HeaderTemplate Parse(IniParser parser)
        {
           return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("HeaderTemplate", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<HeaderTemplate> FieldParseTable = new IniParseTable<HeaderTemplate>
        {
            { "Font", (parser, x) => x.Font.Name = parser.ParseString() },
            { "Point", (parser, x) => x.Font.Size = parser.ParseUnsignedInteger() },
            { "Bold", (parser, x) => x.Font.Bold = parser.ParseBoolean() }
        };

        public FontDesc Font { get; } = new FontDesc();
    }

    public sealed class FontDesc
    {
        public string Name { get; set; }
        public uint Size { get; set; }
        public bool Bold { get; set; }
    }
}
