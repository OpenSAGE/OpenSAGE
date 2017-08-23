using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Requires object to be KindOf = DRAWABLE_ONLY.
    /// </summary>
    public sealed class W3dTracerDraw : ObjectDrawModule
    {
        internal static W3dTracerDraw Parse(IniParser parser) => parser.ParseBlock(DefaultFieldParseTable);

        internal static readonly IniParseTable<W3dTracerDraw> DefaultFieldParseTable = new IniParseTable<W3dTracerDraw>();
    }
}
