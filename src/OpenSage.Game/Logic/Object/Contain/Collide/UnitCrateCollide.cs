using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class UnitCrateCollideModuleData : CrateCollideModuleData
    {
        internal static UnitCrateCollideModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<UnitCrateCollideModuleData> FieldParseTable = CrateCollideModuleData.FieldParseTable
            .Concat(new IniParseTable<UnitCrateCollideModuleData>
            {
                { "UnitCount", (parser, x) => x.UnitCount = parser.ParseInteger() },
                { "UnitName", (parser, x) => x.UnitName = parser.ParseAssetReference() }
            });

        public int UnitCount { get; private set; }
        public string UnitName { get; private set; }
    }
}
