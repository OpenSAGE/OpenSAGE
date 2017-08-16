using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// All world objects should use a draw module. This module is used where an object should 
    /// never actually be drawn due either to the nature or type of the object or because its 
    /// drawing is handled by other logic, e.g. bridges.
    /// </summary>
    public sealed class W3dDefaultDraw : ObjectDrawModule
    {
        internal static W3dDefaultDraw Parse(IniParser parser)
        {
            return parser.ParseBlock(DefaultFieldParseTable);
        }

        internal static readonly IniParseTable<W3dDefaultDraw> DefaultFieldParseTable = new IniParseTable<W3dDefaultDraw>();
    }
}
