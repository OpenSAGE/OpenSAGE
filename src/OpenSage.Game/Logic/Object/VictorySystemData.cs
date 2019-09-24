using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class VictorySystemData : BaseAsset
    {
        internal static VictorySystemData Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("VictorySystemData", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<VictorySystemData> FieldParseTable = new IniParseTable<VictorySystemData>
        {
            { "CellSize", (parser, x) => x.CellSize = parser.ParseInteger() },
            { "ScalePerLogicFrame", (parser, x) => x.ScalePerLogicFrame = parser.ParseFloat() },
            { "SubtractPerLogicFrame", (parser, x) => x.SubtractPerLogicFrame = parser.ParseFloat() },
            { "CellBonusRadius", (parser, x) => x.CellBonusRadius = parser.ParseFloat() },
        };

        public int CellSize { get; private set; }
        public float ScalePerLogicFrame { get; private set; }
        public float SubtractPerLogicFrame { get; private set; }
        public float CellBonusRadius { get; private set; }
    }
}
