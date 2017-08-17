using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class SupplyWarehouseDockUpdateBehavior : ObjectBehavior
    {
        internal static SupplyWarehouseDockUpdateBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SupplyWarehouseDockUpdateBehavior> FieldParseTable = new IniParseTable<SupplyWarehouseDockUpdateBehavior>
        {
            { "NumberApproachPositions", (parser, x) => x.NumberApproachPositions = parser.ParseInteger() },
            { "StartingBoxes", (parser, x) => x.StartingBoxes = parser.ParseInteger() },
            { "AllowsPassthrough", (parser, x) => x.AllowsPassthrough = parser.ParseBoolean() },
            { "DeleteWhenEmpty", (parser, x) => x.DeleteWhenEmpty = parser.ParseBoolean() }
        };

        /// <summary>
        /// Number of approach bones in the model. If this is -1, infinite harvesters can approach.
        /// </summary>
        public int NumberApproachPositions { get; private set; }

        /// <summary>
        /// Used to determine the visual representation of a full warehouse.
        /// </summary>
        public int StartingBoxes { get; private set; }

        /// <summary>
        /// Can harvesters drive through this warehouse? Should be set to false if all dock points are external.
        /// </summary>
        public bool AllowsPassthrough { get; private set; } = true;

        /// <summary>
        /// True if warehouse should be deleted when depleted.
        /// </summary>
        public bool DeleteWhenEmpty { get; private set; }
    }
}
