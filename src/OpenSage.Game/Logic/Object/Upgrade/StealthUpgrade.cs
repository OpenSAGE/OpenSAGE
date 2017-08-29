using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Eenables use of <see cref="StealthUpdateModuleData"/> module on this object. Requires 
    /// <see cref="StealthUpdateModuleData.InnateStealth"/> = No defined in the <see cref="StealthUpdateModuleData"/> 
    /// module.
    /// </summary>
    public sealed class StealthUpgradeModuleData : UpgradeModuleData
    {
        internal static StealthUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<StealthUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<StealthUpgradeModuleData>());
    }
}
