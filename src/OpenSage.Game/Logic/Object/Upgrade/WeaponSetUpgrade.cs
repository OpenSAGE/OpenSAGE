using System.Linq;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class WeaponSetUpgrade : BehaviorModule
    {
        private readonly GameObject _gameObject;
        private readonly WeaponSetUpgradeModuleData _moduleData;

        internal WeaponSetUpgrade(GameObject gameObject, WeaponSetUpgradeModuleData moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            bool active = false;

            foreach (var trigger in _moduleData.TriggeredBy)
            {
                var upgrade = _gameObject.Upgrades.FirstOrDefault(template => template.Name == trigger);

                if (upgrade != null)
                {
                    active = true;
                    if (_moduleData.RequiresAllTriggers == false)
                    {
                        break;
                    }
                }
                else
                {
                    // Disable the trigger if one condition is not met
                    active = false;
                }
            }

            if (active)
            {
                var weaponSet = _gameObject.Definition.WeaponSets.FirstOrDefault(w => w.Conditions.Get(WeaponSetConditions.PlayerUpgrade));
                _gameObject.SetWeaponSet(weaponSet);
            }
        }
    }

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

        internal override BehaviorModule CreateModule(GameObject gameObject)
        {
            return new WeaponSetUpgrade(gameObject, this);
        }
    }
}
