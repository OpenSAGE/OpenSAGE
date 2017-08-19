using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Requires the <see cref="ObjectKinds.SupplySource"/> KindOf defined in order to work properly.
    /// Ensures the object acts as a destination for collection of supplies.
    /// </summary>
    public sealed class SupplyCenterCreate : ObjectBehavior
    {
        internal static SupplyCenterCreate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SupplyCenterCreate> FieldParseTable = new IniParseTable<SupplyCenterCreate>();
    }
}
