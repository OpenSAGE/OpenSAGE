using System;

namespace OpenSage.Logic.Object
{
    internal sealed class InactiveWeaponState : BaseWeaponState
    {
        public InactiveWeaponState(WeaponStateContext context)
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

            if (Context.Weapon.Template.AutoReloadsClip != WeaponReloadType.None
                && Context.Weapon.UsesClip
                && Context.Weapon.CurrentRounds < Context.Weapon.Template.ClipSize)
            {
                return WeaponState.InactivePendingReload;
            }

            return null;
        }
    }
}
