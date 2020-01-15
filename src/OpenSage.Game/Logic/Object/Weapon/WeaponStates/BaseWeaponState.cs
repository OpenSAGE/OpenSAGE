using System;

namespace OpenSage.Logic.Object
{
    internal abstract class BaseWeaponState
    {
        protected readonly WeaponStateContext Context;

        protected ModelConditionFlag[] ModelConditionFlags { get; }

        protected BaseWeaponState(WeaponStateContext context)
        {
            Context = context;
            ModelConditionFlags = GetModelConditionFlags(context.WeaponIndex);
        }

        private void SetModelConditionFlags(bool value)
        {
            foreach (var flag in ModelConditionFlags)
            {
                Context.GameObject.ModelConditionFlags.Set(flag, value);
            }
        }

        protected abstract ModelConditionFlag[] GetModelConditionFlags(int weaponIndex);

        public abstract WeaponState? GetNextState(TimeSpan currentTime);

        public void OnEnterState(TimeSpan enterTime)
        {
            SetModelConditionFlags(true);

            OnEnterStateImpl(enterTime);
        }

        protected virtual void OnEnterStateImpl(TimeSpan enterTime) { }

        public void OnExitState()
        {
            SetModelConditionFlags(false);

            OnExitStateImpl();
        }

        protected virtual void OnExitStateImpl() { }
    }
}
