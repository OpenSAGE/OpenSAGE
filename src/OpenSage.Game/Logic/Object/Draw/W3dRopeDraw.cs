using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Requires object to be KindOf = DRAWABLE_ONLY.
    /// </summary>
    public sealed class W3dRopeDrawModuleData : DrawModuleData
    {
        internal static W3dRopeDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<W3dRopeDrawModuleData> FieldParseTable = new IniParseTable<W3dRopeDrawModuleData>();
    }
}
