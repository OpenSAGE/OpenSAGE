using System;

namespace OpenSage.Logic.Object
{
    internal sealed class InactivePendingReloadWeaponState : FixedDurationWeaponState
    {
        protected override RangeDuration Duration => Context.WeaponTemplate.AutoReloadWhenIdle;

        public InactivePendingReloadWeaponState(WeaponStateContext context)
            : base(context)
        {
        }

        protected override ModelConditionFlag[] GetModelConditionFlags(int weaponIndex) =>
            Array.Empty<ModelConditionFlag>();

        public override WeaponState? GetNextState(TimeSpan currentTime)
        {
            if (Context.Weapon.HasValidTarget)
            {
                return WeaponState.PreAttack;
            }

            if (IsTimeToExitState(currentTime))
            {
                return WeaponState.Reloading;
            }

            return null;
        }
    }
}
