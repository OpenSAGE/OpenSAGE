using System;

namespace OpenSage.Logic.Object
{
    internal sealed class FiringWeaponState : FixedDurationWeaponState
    {
        protected override RangeDuration Duration => Context.WeaponTemplate.FiringDuration;

        public FiringWeaponState(WeaponStateContext context)
            : base(context)
        {
        }

        protected override ModelConditionFlag[] GetModelConditionFlags(int weaponIndex) =>
            new[]
            {
                ModelConditionFlagUtility.GetFiringFlag(weaponIndex),
                ModelConditionFlagUtility.GetFiringOrPreAttackFlag(weaponIndex),
                ModelConditionFlagUtility.GetFiringOrReloadingFlag(weaponIndex)
            };

        protected override void OnEnterStateImpl(TimeSpan enterTime)
        {
            foreach (var nugget in Context.Weapon.Nuggets)
            {
                nugget.Activate(enterTime);
            }
        }

        public override WeaponState? GetNextState(TimeSpan currentTime)
        {
            if (!Context.Weapon.HasValidTarget)
            {
                return WeaponState.Inactive;
            }

            foreach (var nugget in Context.Weapon.Nuggets)
            {
                nugget.Update(currentTime);
            }

            if (IsTimeToExitState(currentTime))
            {
                return WeaponState.IdleAfterFiring;
            }

            return null;
        }
    }
}
