using System.IO;
using System.Linq;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public sealed class WeaponSetUpgrade : UpgradeModule
    {
        private readonly WeaponSetUpgradeModuleData _moduleData;

        internal WeaponSetUpgrade(GameObject gameObject, WeaponSetUpgradeModuleData moduleData) : base(gameObject, moduleData)
        {
            _moduleData = moduleData;
        }

        internal override void OnTrigger(BehaviorUpdateContext context, bool triggered)
        {
            if (triggered)
            {
                var weaponSet = _gameObject.Definition.WeaponSets.Values.FirstOrDefault(w => w.Conditions.Get(WeaponSetConditions.PlayerUpgrade));
                _gameObject.SetWeaponSet(weaponSet);
            }
            else
            {
                _gameObject.SetDefaultWeapon();
            }
        }

        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 1)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);
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

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new WeaponSetUpgrade(gameObject, this);
        }
    }
}
