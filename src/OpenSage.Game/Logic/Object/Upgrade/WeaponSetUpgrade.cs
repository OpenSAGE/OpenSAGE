using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Triggers use of PLAYER_UPGRADE WeaponSet on this object.
    /// Allows the use of WeaponUpgradeSound within UnitSpecificSounds section of the object.
    /// Allows the use of the WEAPONSET_PLAYER_UPGRADE ModelConditionState.
    /// </summary>
    public sealed class WeaponSetUpgradeModuleData : UpgradeModuleData
    {
        internal static WeaponSetUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<WeaponSetUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<WeaponSetUpgradeModuleData>
            {
                { "WeaponCondition", (parser, x) => x.WeaponCondition = parser.ParseEnum<WeaponSetConditions>() },
            });

        public WeaponSetConditions WeaponCondition { get; private set; }
    }
}
