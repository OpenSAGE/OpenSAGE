using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class DefaultProductionExitUpdate : ObjectBehavior
    {
        internal static DefaultProductionExitUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<DefaultProductionExitUpdate> FieldParseTable = new IniParseTable<DefaultProductionExitUpdate>
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
