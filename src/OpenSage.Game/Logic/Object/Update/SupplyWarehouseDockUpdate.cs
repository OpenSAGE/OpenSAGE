using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class SupplyWarehouseDockUpdateModuleData : DockUpdateModuleData
    {
        internal static SupplyWarehouseDockUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SupplyWarehouseDockUpdateModuleData> FieldParseTable = DockUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<SupplyWarehouseDockUpdateModuleData>
            {
                { "StartingBoxes", (parser, x) => x.StartingBoxes = parser.ParseInteger() },
                { "DeleteWhenEmpty", (parser, x) => x.DeleteWhenEmpty = parser.ParseBoolean() }
            });

        /// <summary>
        /// Used to determine the visual representation of a full warehouse.
        /// </summary>
        public int StartingBoxes { get; private set; }

        /// <summary>
        /// True if warehouse should be deleted when depleted.
        /// </summary>
        public bool DeleteWhenEmpty { get; private set; }
    }
}
