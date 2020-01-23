using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// All world objects should use a draw module. This module is used where an object should 
    /// never actually be drawn due either to the nature or type of the object or because its 
    /// drawing is handled by other logic, e.g. bridges.
    /// </summary>
    public sealed class W3dDefaultDrawModuleData : DrawModuleData
    {
        internal static W3dDefaultDrawModuleData Parse(IniParser parser) => parser.ParseBlock(DefaultFieldParseTable);

        internal static readonly IniParseTable<W3dDefaultDrawModuleData> DefaultFieldParseTable = new IniParseTable<W3dDefaultDrawModuleData>();
    }
}
