using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows the object to take damage but not die. The object will only die from irresistable damage.
    /// </summary>
    public sealed class HighlanderBodyModuleData : ActiveBodyModuleData
    {
        internal static new HighlanderBodyModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<HighlanderBodyModuleData> FieldParseTable = ActiveBodyModuleData.FieldParseTable
            .Concat(new IniParseTable<HighlanderBodyModuleData>());
    }
}
