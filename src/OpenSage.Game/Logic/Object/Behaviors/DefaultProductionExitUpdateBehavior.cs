using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class DefaultProductionExitUpdateBehavior : ObjectBehavior
    {
        internal static DefaultProductionExitUpdateBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<DefaultProductionExitUpdateBehavior> FieldParseTable = new IniParseTable<DefaultProductionExitUpdateBehavior>
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
