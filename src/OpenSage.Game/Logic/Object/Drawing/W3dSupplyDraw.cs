using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class W3dSupplyDraw : W3dModelDraw
    {
        internal static W3dSupplyDraw Parse(IniParser parser)
        {
            return parser.ParseBlock(SupplyFieldParseTable);
        }

        private static readonly IniParseTable<W3dSupplyDraw> SupplyFieldParseTable = new IniParseTable<W3dSupplyDraw>
        {
            { "SupplyBonePrefix", (parser, x) => x.SupplyBonePrefix = parser.ParseString() }
        }.Concat<W3dSupplyDraw, W3dModelDraw>(ModelFieldParseTable);

        public string SupplyBonePrefix { get; private set; }
    }
}
