using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class W3dSupplyDrawModuleData : W3dModelDrawModuleData
    {
        internal static W3dSupplyDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<W3dSupplyDrawModuleData> FieldParseTable = W3dModelDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dSupplyDrawModuleData>
            {
                { "SupplyBonePrefix", (parser, x) => x.SupplyBonePrefix = parser.ParseString() }
            });

        public string SupplyBonePrefix { get; private set; }
    }
}
