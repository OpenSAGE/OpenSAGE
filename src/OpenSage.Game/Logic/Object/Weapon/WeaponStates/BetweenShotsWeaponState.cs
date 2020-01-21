using System;

namespace OpenSage.Logic.Object
{
    internal sealed class BetweenShotsWeaponState : FixedDurationWeaponState
    {
        protected override RangeDuration Duration => Context.Weapon.Template.CoolDownDelayBetweenShots;

        public BetweenShotsWeaponState(WeaponStateContext context)
            : base(context)
        {
        }

        protected override ModelConditionFlag[] GetModelConditionFlags(int weaponIndex) =>
            new[]
            {
                ModelConditionFlagUtility.GetBetweenFiringShotsFlag(weaponIndex)
            };

        public override WeaponState? GetNextState(TimeSpan currentTime)
        {
            if (!Context.Weapon.HasValidTarget)
            {
                return WeaponState.Inactive;
            }

            if (IsTimeToExitState(currentTime))
            {
                return WeaponState.Firing;
            }

            return null;
        }
    }
}
