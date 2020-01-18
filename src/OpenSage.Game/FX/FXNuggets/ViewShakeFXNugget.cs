using OpenSage.Data.Ini;

namespace OpenSage.FX
{
    public sealed class ViewShakeFXNugget : FXNugget
    {
        internal static ViewShakeFXNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ViewShakeFXNugget> FieldParseTable = FXNuggetFieldParseTable.Concat(new IniParseTable<ViewShakeFXNugget>
        {
            { "Type", (parser, x) => x.Type = parser.ParseEnum<ViewShakeType>() }
        });

        public ViewShakeType Type { get; private set; }
    }
}
