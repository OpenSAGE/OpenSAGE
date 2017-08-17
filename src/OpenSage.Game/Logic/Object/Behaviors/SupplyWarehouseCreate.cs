using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Ensures the object acts as a source for supply collection.
    /// </summary>
    public sealed class SupplyWarehouseCreate : ObjectBehavior
    {
        internal static SupplyWarehouseCreate Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<SupplyWarehouseCreate> FieldParseTable = new IniParseTable<SupplyWarehouseCreate>();
    }
}
