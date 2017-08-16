using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class W3dTankDraw : W3dModelDraw
    {
        internal static W3dTankDraw Parse(IniParser parser)
        {
            return parser.ParseBlock(TankFieldParseTable);
        }

        private static readonly IniParseTable<W3dTankDraw> TankFieldParseTable = new IniParseTable<W3dTankDraw>
        {
            { "TrackMarks", (parser, x) => x.TrackMarks = parser.ParseFileName() },
            { "TreadDriveSpeedFraction", (parser, x) => x.TreadDriveSpeedFraction = parser.ParseFloat() },
        }.Concat<W3dTankDraw, W3dModelDraw>(ModelFieldParseTable);

        public string TrackMarks { get; private set; }
        public float TreadDriveSpeedFraction { get; private set; }
    }
}
