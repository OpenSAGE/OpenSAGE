using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class WeaponBonusUpgrade : UpgradeModule
    {
        internal WeaponBonusUpgrade(WeaponBonusUpgradeModuleData moduleData) : base(moduleData)
        {
        }
    }

    /// <summary>
    /// Triggers use of WeaponBonus = parameter on this object's weapons.
    /// </summary>
    public sealed class WeaponBonusUpgradeModuleData : UpgradeModuleData
    {
        internal static WeaponBonusUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<WeaponBonusUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<WeaponBonusUpgradeModuleData>());

        internal override BehaviorModule CreateModule(GameObject gameObject)
        {
            return new WeaponBonusUpgrade(this);
        }
    }
}
