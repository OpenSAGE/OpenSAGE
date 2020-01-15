using System;

namespace OpenSage.Logic.Object
{
    internal sealed class ReloadingWeaponState : FixedDurationWeaponState
    {
        protected override RangeDuration Duration => Context.WeaponTemplate.ClipReloadTime;

        public ReloadingWeaponState(WeaponStateContext context)
            : base(context)
        {
        }

        protected override ModelConditionFlag[] GetModelConditionFlags(int weaponIndex) =>
            new[]
            {
                ModelConditionFlagUtility.GetReloadingFlag(weaponIndex),
                ModelConditionFlagUtility.GetFiringOrReloadingFlag(weaponIndex)
            };

        public override WeaponState? GetNextState(TimeSpan currentTime)
        {
            if (IsTimeToExitState(currentTime))
            {
                Context.Weapon.FillClip();

                if (!Context.Weapon.HasValidTarget)
                {
                    return WeaponState.Inactive;
                }
                else if (Context.WeaponTemplate.PreAttackType == WeaponPrefireType.PerClip
                    || Context.WeaponTemplate.PreAttackType == WeaponPrefireType.PerShot)
                {
                    return WeaponState.PreAttack;
                }
                return WeaponState.Firing;
            }

            return null;
        }
    }
}
