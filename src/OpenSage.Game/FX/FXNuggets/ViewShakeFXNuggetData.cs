using OpenSage.Data.Ini;

namespace OpenSage.FX
{
    public sealed class ViewShakeFXNuggetData : FXNuggetData
    {
        internal static ViewShakeFXNuggetData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ViewShakeFXNuggetData> FieldParseTable = FXNuggetFieldParseTable.Concat(new IniParseTable<ViewShakeFXNuggetData>
        {
            { "Type", (parser, x) => x.Type = parser.ParseEnum<ViewShakeType>() }
        });

        public ViewShakeType Type { get; private set; }
    }
}
