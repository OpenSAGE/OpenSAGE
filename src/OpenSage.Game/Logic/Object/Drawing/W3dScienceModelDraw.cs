using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class W3dScienceModelDraw : W3dModelDraw
    {
        internal static W3dScienceModelDraw Parse(IniParser parser)
        {
            return parser.ParseBlock(ScienceModelFieldParseTable);
        }

        private static readonly IniParseTable<W3dScienceModelDraw> ScienceModelFieldParseTable = new IniParseTable<W3dScienceModelDraw>
        {
            { "RequiredScience", (parser, x) => x.RequiredScience = parser.ParseAssetReference() }
        }.Concat<W3dScienceModelDraw, W3dModelDraw>(ModelFieldParseTable);

        public string RequiredScience { get; private set; }
    }
}
