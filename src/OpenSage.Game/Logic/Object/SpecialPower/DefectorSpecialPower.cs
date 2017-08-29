using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// When used in junction with the SPECIAL_DEFECTOR special power, the unit will defect to 
    /// your side.
    /// </summary>
    public sealed class DefectorSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static DefectorSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<DefectorSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<DefectorSpecialPowerModuleData>());
    }
}
