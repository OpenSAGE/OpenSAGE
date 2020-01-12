using System.Numerics;

namespace OpenSage.Logic.Object
{
    public sealed class Weapon
    {
        private readonly GameObject _gameObject;
        private readonly WeaponTemplateSet _weaponSet;
        private readonly WeaponTemplate _currentWeaponTemplate;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        internal Weapon(GameObject gameObject, WeaponTemplateSet weaponSet)
        {
            _gameObject = gameObject;
            _weaponSet = weaponSet;
            _currentWeaponTemplate = weaponSet.Slots[WeaponSlot.Primary].Weapon.Value;
        }

        public void LocalLogicTick(in TimeInterval gameTime, GameObject target)
        {
            var distance = Vector3.Distance(target.Transform.Translation,
                                       _gameObject.Transform.Translation);

            if (distance > _currentWeaponTemplate.AttackRange)
            {
                //TODO: move closer to the target
                return;
            }

            //TODO: take care of weaponspeed and spawn projectile
            // TODO: Let ActiveBody set its own health?
            target.Body.Health -= (decimal)_currentWeaponTemplate.PrimaryDamage;
        }
    }
}
