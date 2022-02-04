using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    internal sealed class WeaponSetUpgrade : UpgradeModule
    {
        private readonly WeaponSetUpgradeModuleData _moduleData;

        internal WeaponSetUpgrade(GameObject gameObject, WeaponSetUpgradeModuleData moduleData) : base(gameObject, moduleData)
        {
            _moduleData = moduleData;
        }

        protected override void OnUpgrade()
        {
            _gameObject.SetWeaponSetCondition(WeaponSetConditions.PlayerUpgrade, true);
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
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
