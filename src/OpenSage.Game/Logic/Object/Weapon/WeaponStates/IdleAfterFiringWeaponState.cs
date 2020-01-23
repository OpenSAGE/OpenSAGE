using System;

namespace OpenSage.Logic.Object
{
    internal sealed class IdleAfterFiringWeaponState : FixedDurationWeaponState
    {
        protected override RangeDuration Duration => Context.Weapon.Template.IdleAfterFiringDelay;

        public IdleAfterFiringWeaponState(WeaponStateContext context)
            : base(context)
        {
        }

        protected override ModelConditionFlag[] GetModelConditionFlags(int weaponIndex) =>
            Array.Empty<ModelConditionFlag>();

        public override WeaponState? GetNextState(TimeSpan currentTime)
        {
            if (!Context.Weapon.HasValidTarget)
            {
                return WeaponState.Inactive;
            }

            if (IsTimeToExitState(currentTime))
            {
                if (Context.Weapon.IsClipEmpty())
                {
                    return WeaponState.Reloading;
                }
                else if (Context.Weapon.Template.PreAttackType == WeaponPrefireType.PerShot)
                {
                    return WeaponState.PreAttack;
                }
                return WeaponState.BetweenShots;
            }

            return null;
        }
    }
}
