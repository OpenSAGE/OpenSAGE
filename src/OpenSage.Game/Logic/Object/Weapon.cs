using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenSage.Logic.Object
{
    class Weapon
    {
        private readonly GameObject _gameObject;
        private readonly WeaponSet _weaponSet;
        private readonly WeaponTemplate _currentWeaponTemplate;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public Weapon(GameObject gameObject, WeaponSet weaponSet)
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
            target.Health -= (decimal)_currentWeaponTemplate.PrimaryDamage;
        }
    }
}
