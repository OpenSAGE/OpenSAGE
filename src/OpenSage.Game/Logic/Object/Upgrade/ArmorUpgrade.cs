using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Triggers use of PLAYER_UPGRADE ArmorSet on this object.
    /// </summary>
    public sealed class ArmorUpgradeModuleData : UpgradeModuleData
    {
        internal static ArmorUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ArmorUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<ArmorUpgradeModuleData>());
    }
}
