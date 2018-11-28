using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class ToolTipUpgradeModuleData : UpgradeModuleData
    {
        internal static ToolTipUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ToolTipUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<ToolTipUpgradeModuleData>
            {
                { "DisplayName", (parser, x) => x.DisplayName = parser.ParseLocalizedStringKey() }
            });

        public string DisplayName { get; private set; }
    }
}
