using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    internal sealed class WeaponBonusUpgrade : UpgradeModule
    {
        internal WeaponBonusUpgrade(GameObject gameObject, WeaponBonusUpgradeModuleData moduleData) : base(gameObject, moduleData)
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

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new WeaponBonusUpgrade(gameObject, this);
        }
    }
}
