using System;
using System.Collections.Generic;
using OpenSage.Audio;
using OpenSage.Content.Loaders;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Handles transitions between a weapon's various states.
    ///
    /// If the state is anything other than Inactive, and there is no current target
    /// (because it is dead, or it has moved out of range, etc.) then the weapon
    /// transitions to Inactive.
    ///
    /// When first created, the weapon has a full clip equal to ClipSize.
    /// 
    /// Inactive
    /// ========
    /// The weapon starts out in the Inactive state.
    /// 
    /// If:
    /// - the weapon is in the Inactive state
    /// - and it has less than a full clip
    /// - and AutoReloadsClip is Yes or ReturnToBase
    /// then it transitions to InactivePendingReload.
    /// 
    /// When the weapon is given a new target, it transitions to the PreAttack state.
    ///
    /// InactivePendingReload
    /// =====================
    /// The weapon remains this state for the duration specified in AutoReloadWhenIdle.
    /// If no new target has been set in this time, it transitions to Reloading.
    /// (For ReturnToBase, does it transition to Reloading only when it reaches its base?)
    /// If the weapon is given a new target while in this state,
    /// it transitions to the PreAttack state.
    ///
    /// PreAttack
    /// =========
    /// The weapon remains in the PreAttack state for the duration specified in PreAttackDelay.
    /// Then it transitions to Firing.
    ///
    /// Firing
    /// ======
    /// When the weapon transitions to the Firing state, it activates each of its weapon effect nuggets.
    /// The weapon will remain in the Firing state for the duration specified in FiringDuration.
    /// After firing, the weapon transitions to IdleAfterFiring.
    ///
    /// IdleAfterFiring
    /// ===============
    /// The weapon waits for the duration specified in IdleAfterFiringDelay.
    /// Then one of several things can happen:
    /// - If the clip is now empty, the weapon transitions to Reloading.
    /// - If PreAttackType = PerShot, the weapon transitions to PreAttack.
    /// - Otherwise, the weapon transitions to BetweenShots.
    ///
    /// BetweenShots
    /// ====================
    /// The weapon waits for the duration specified in CoolDownDelayBetweenShots.
    /// Then it transitions to Firing.
    ///
    /// Reloading
    /// =========
    /// The weapon will remain in the Reloading state for the duration specified in ClipReloadTime.
    /// At the end of this time, the clip will be filled with ClipSize rounds.
    /// Now one of several things can happen:
    /// - If there is no current target, this was an auto-reload, so the weapon transitions to Inactive.
    /// - If PreAttack = PerClip or PerShot, the weapon transitions to PreAttack.
    /// - Otherwise, the weapon transitions to Firing.
    /// </summary>
    internal sealed class WeaponStateMachine
    {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly WeaponStateContext _context;
        private readonly Dictionary<WeaponState, BaseWeaponState> _states;
        private BaseWeaponState _currentState;

        public WeaponStateMachine(WeaponStateContext context)
        {
            _context = context;

            _states = new Dictionary<WeaponState, BaseWeaponState>
            {
                { WeaponState.Inactive, new InactiveWeaponState(context) },
                { WeaponState.InactivePendingReload, new InactivePendingReloadWeaponState(context) },
                { WeaponState.PreAttack, new PreAttackWeaponState(context) },
                { WeaponState.Firing, new FiringWeaponState(context) },
                { WeaponState.IdleAfterFiring, new IdleAfterFiringWeaponState(context) },
                { WeaponState.BetweenShots, new BetweenShotsWeaponState(context) },
                { WeaponState.Reloading, new ReloadingWeaponState(context) }
            };

            TransitionToState(WeaponState.Inactive, TimeSpan.Zero);
        }

        public void TransitionToState(WeaponState state, TimeSpan currentTime)
        {
            Logger.Info($"Weapon {_context.WeaponTemplate.Name} on game object {_context.GameObject.Name} transitioning to state {state}");

            _currentState?.OnExitState();

            _currentState = _states[state];

            _currentState.OnEnterState(currentTime);
        }

        public void Update(TimeSpan currentTime)
        {
            while (true)
            {
                // TODO: Fix timing. Need to use actual time that previous state ended.
                var nextState = _currentState.GetNextState(currentTime);

                if (nextState != null)
                {
                    TransitionToState(nextState.Value, currentTime);
                }
                else
                {
                    break;
                }
            }
        }
    }

    internal sealed class WeaponStateContext
    {
        public readonly GameObject GameObject;

        public readonly Weapon Weapon;
        public readonly WeaponTemplate WeaponTemplate;
        public readonly int WeaponIndex;

        public readonly AudioSystem AudioSystem;
        public readonly AssetLoadContext AssetLoadContext;

        public WeaponStateContext(
            GameObject gameObject,
            Weapon weapon,
            WeaponTemplate weaponTemplate,
            int weaponIndex,
            AudioSystem audioSystem,
            AssetLoadContext assetLoadContext)
        {
            GameObject = gameObject;
            Weapon = weapon;
            WeaponTemplate = weaponTemplate;
            WeaponIndex = weaponIndex;
            AudioSystem = audioSystem;
            AssetLoadContext = assetLoadContext;
        }
    }

    internal enum WeaponState
    {
        Inactive,
        InactivePendingReload,
        PreAttack,
        Firing,
        IdleAfterFiring,
        BetweenShots,
        Reloading,
    }
}
