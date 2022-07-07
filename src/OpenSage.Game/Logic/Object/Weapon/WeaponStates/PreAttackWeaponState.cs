namespace OpenSage.Logic.Object
{
    internal sealed class PreAttackWeaponState : FixedDurationWeaponState
    {
        protected override RangeDuration Duration => Context.Weapon.Template.PreAttackDelay;

        public PreAttackWeaponState(WeaponStateContext context)
            : base(context)
        {
        }

        protected override ModelConditionFlag[] GetModelConditionFlags(int weaponIndex) =>
            new[]
            {
                ModelConditionFlagUtility.GetPreAttackFlag(weaponIndex),
                ModelConditionFlagUtility.GetFiringOrPreAttackFlag(weaponIndex)
            };

        public override WeaponState? GetNextState()
        {
            if (!Context.Weapon.HasValidTarget)
            {
                return WeaponState.Inactive;
            }

            if (IsTimeToExitState())
            {
                return WeaponState.Firing;
            }

            return null;
        }
    }
}
