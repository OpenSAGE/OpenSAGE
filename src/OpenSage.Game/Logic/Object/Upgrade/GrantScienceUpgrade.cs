using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class GrantScienceUpgradeModuleData : UpgradeModuleData
    {
        internal static GrantScienceUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<GrantScienceUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<GrantScienceUpgradeModuleData>
            {
                { "GrantScience", (parser, x) => x.GrantScience = parser.ParseAssetReference() },
            });

        public string GrantScience { get; private set; }
    }
}
