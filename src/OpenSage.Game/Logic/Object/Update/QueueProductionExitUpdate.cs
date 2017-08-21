using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class QueueProductionExitUpdate : ObjectBehavior
    {
        internal static QueueProductionExitUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<QueueProductionExitUpdate> FieldParseTable = new IniParseTable<QueueProductionExitUpdate>
        {
            { "UnitCreatePoint", (parser, x) => x.UnitCreatePoint = Coord3D.Parse(parser) },
            { "NaturalRallyPoint", (parser, x) => x.NaturalRallyPoint = Coord3D.Parse(parser) },
            { "ExitDelay", (parser, x) => x.ExitDelay = parser.ParseInteger() },
            { "InitialBurst", (parser, x) => x.InitialBurst = parser.ParseInteger() },
        };

        public Coord3D UnitCreatePoint { get; private set; }

        /// <summary>
        /// <see cref="NaturalRallyPoint.X"/> must match <see cref="ObjectDefinition.GeometryMajorRadius"/>.
        /// </summary>
        public Coord3D NaturalRallyPoint { get; private set; }

        /// <summary>
        /// Used for Red Guards to make them come out one at a time.
        /// </summary>
        public int ExitDelay { get; private set; }

        public int InitialBurst { get; private set; }
    }
}
