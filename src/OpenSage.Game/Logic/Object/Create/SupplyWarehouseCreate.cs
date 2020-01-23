using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Ensures the object acts as a source for supply collection.
    /// </summary>
    public sealed class SupplyWarehouseCreateModuleData : CreateModuleData
    {
        internal static SupplyWarehouseCreateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SupplyWarehouseCreateModuleData> FieldParseTable = new IniParseTable<SupplyWarehouseCreateModuleData>();
    }
}
