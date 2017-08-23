using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Requires object to be KindOf = DRAWABLE_ONLY.
    /// </summary>
    public sealed class W3dRopeDraw : ObjectDrawModule
    {
        internal static W3dRopeDraw Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<W3dRopeDraw> FieldParseTable = new IniParseTable<W3dRopeDraw>();
    }
}
