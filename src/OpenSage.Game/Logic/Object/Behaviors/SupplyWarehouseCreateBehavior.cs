using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Ensures the object acts as a source for supply collection.
    /// </summary>
    public sealed class SupplyWarehouseCreateBehavior : ObjectBehavior
    {
        internal static SupplyWarehouseCreateBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<SupplyWarehouseCreateBehavior> FieldParseTable = new IniParseTable<SupplyWarehouseCreateBehavior>();
    }
}
