using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Used by FS_AIRFIELD KindOfs. If <see cref="HasRunways"/> is set then the model requires 
    /// RunwayStartN, RunwayEndN, RunwayNPrepN, RunwayNParkingN and RunwayNParkNHan bones where N 
    /// corresponds to rows and columns. Module will only use the HeliPark01 bone for helicopters.
    /// </summary>
    public sealed class ParkingPlaceBehaviorModuleData : BehaviorModuleData
    {
        internal static ParkingPlaceBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ParkingPlaceBehaviorModuleData> FieldParseTable = new IniParseTable<ParkingPlaceBehaviorModuleData>
        {
            { "HealAmountPerSecond", (parser, x) => x.HealAmountPerSecond = parser.ParseInteger() },
            { "NumRows", (parser, x) => x.NumRows = parser.ParseInteger() },
            { "NumCols", (parser, x) => x.NumCols = parser.ParseInteger() },
            { "HasRunways", (parser, x) => x.HasRunways = parser.ParseBoolean() },
            { "ApproachHeight", (parser, x) => x.ApproachHeight = parser.ParseInteger() },
            { "ParkInHangars", (parser, x) => x.ParkInHangars = parser.ParseBoolean() },
        };

        public int HealAmountPerSecond { get; private set; }
        public int NumRows { get; private set; }
        public int NumCols { get; private set; }
        public bool HasRunways { get; private set; }
        public int ApproachHeight { get; private set; }
        public bool ParkInHangars { get; private set; }
    }
}
