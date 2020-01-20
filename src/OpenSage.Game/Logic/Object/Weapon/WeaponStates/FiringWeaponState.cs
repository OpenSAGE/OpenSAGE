using System;
using System.Numerics;
using OpenSage.FX;

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
            base.OnEnterStateImpl(enterTime);

            if (Context.Weapon.UsesClip)
            {
                Context.Weapon.CurrentRounds--;
            }

            Context.GameContext.AudioSystem.PlayAudioEvent(
                Context.WeaponTemplate.FireSound.Value);

            foreach (var nugget in Context.Weapon.Nuggets)
            {
                nugget.Activate(enterTime);
            }

            TriggerWeaponFireFX();

            // TODO: ProjectileObject
        }

        private void TriggerWeaponFireFX()
        {
            var fireFXTransform = Context.GameObject.GetWeaponFireFXBoneTransform(
                Context.Weapon.Slot,
                Context.WeaponIndex);
            if (fireFXTransform == null)
            {
                return;
            }

            var fireFXListData = Context.WeaponTemplate.FireFX?.Value;
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
