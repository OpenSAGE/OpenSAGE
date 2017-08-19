using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Requires the <see cref="ObjectKinds.SupplySource"/> KindOf defined in order to work properly.
    /// </summary>
    public sealed class SupplyCenterProductionExitUpdate : ObjectBehavior
    {
        internal static SupplyCenterProductionExitUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SupplyCenterProductionExitUpdate> FieldParseTable = new IniParseTable<SupplyCenterProductionExitUpdate>
        {
            { "UnitCreatePoint", (parser, x) => x.UnitCreatePoint = Coord3D.Parse(parser) },
            { "NaturalRallyPoint", (parser, x) => x.NaturalRallyPoint = Coord3D.Parse(parser) },
        };

        public Coord3D UnitCreatePoint { get; private set; }

        /// <summary>
        /// <see cref="NaturalRallyPoint.X"/> must match <see cref="ObjectDefinition.GeometryMajorRadius"/>.
        /// </summary>
        public Coord3D NaturalRallyPoint { get; private set; }
    }
}
