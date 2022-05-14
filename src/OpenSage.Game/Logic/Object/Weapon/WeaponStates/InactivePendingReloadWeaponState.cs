using System;

namespace OpenSage.Logic.Object
{
    internal sealed class InactivePendingReloadWeaponState : FixedDurationWeaponState
    {
        protected override RangeDuration Duration => Context.Weapon.Template.AutoReloadWhenIdle;

        public InactivePendingReloadWeaponState(WeaponStateContext context)
            : base(context)
        {
        }

        protected override ModelConditionFlag[] GetModelConditionFlags(int weaponIndex) =>
            Array.Empty<ModelConditionFlag>();

        public override WeaponState? GetNextState()
        {
            if (Context.Weapon.HasValidTarget)
            {
                return WeaponState.PreAttack;
            }

            if (IsTimeToExitState())
            {
                return WeaponState.Reloading;
            }

            return null;
        }
    }
}
