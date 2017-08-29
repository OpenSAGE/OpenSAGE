using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Triggers use of WeaponBonus = parameter on this object's weapons.
    /// </summary>
    public sealed class WeaponBonusUpgradeModuleData : UpgradeModuleData
    {
        internal static WeaponBonusUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<WeaponBonusUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<WeaponBonusUpgradeModuleData>());
    }
}
