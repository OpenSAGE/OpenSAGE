using System;
using System.Numerics;
using OpenSage.FX;

namespace OpenSage.Logic.Object
{
    internal sealed class FiringWeaponState : FixedDurationWeaponState
    {
        protected override RangeDuration Duration => Context.Weapon.Template.FiringDuration;

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

        protected override void OnEnterStateImpl()
        {
            base.OnEnterStateImpl();

            if (Context.Weapon.UsesClip)
            {
                Context.Weapon.CurrentRounds--;
            }

            Context.GameContext.AudioSystem.PlayAudioEvent(
                Context.Weapon.Template.FireSound?.Value);

            var weaponEffectExecutionContext = new WeaponEffectExecutionContext(Context.Weapon, Context.GameContext);
            foreach (var nugget in Context.Weapon.Template.Nuggets)
            {
                nugget.Execute(weaponEffectExecutionContext);
            }

            TriggerWeaponFireFX();
        }

        private void TriggerWeaponFireFX()
        {
            var fireFXTransform = Context.GameObject.Drawable.GetWeaponFireFXBoneTransform(
                Context.Weapon.Slot,
                Context.Weapon.WeaponIndex);
            if (fireFXTransform == null)
            {
                return;
            }

            var fireFXListData = Context.Weapon.Template.FireFX?.Value;
            if (fireFXListData == null)
            {
                return;
            }

            Matrix4x4.Decompose(
                fireFXTransform.Value,
                out _,
                out var rotation,
                out var translation);

            fireFXListData.Execute(
                new FXListExecutionContext(
                    rotation,
                    translation,
                    Context.GameContext));
        }

        public override WeaponState? GetNextState()
        {
            if (!Context.Weapon.HasValidTarget)
            {
                return WeaponState.Inactive;
            }

            if (IsTimeToExitState())
            {
                return WeaponState.IdleAfterFiring;
            }

            return null;
        }
    }
}
