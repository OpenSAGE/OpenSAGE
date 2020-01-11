using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class DestroyDieModuleData : DieModuleData
    {
        internal static DestroyDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<DestroyDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
            .Concat(new IniParseTable<DestroyDieModuleData>());
    }
}
