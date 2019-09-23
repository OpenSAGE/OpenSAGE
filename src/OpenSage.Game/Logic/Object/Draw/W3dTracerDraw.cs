using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Requires object to be KindOf = DRAWABLE_ONLY.
    /// </summary>
    public sealed class W3dTracerDrawModuleData : DrawModuleData
    {
        internal static W3dTracerDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<W3dTracerDrawModuleData> FieldParseTable = new IniParseTable<W3dTracerDrawModuleData>();
    }
}
