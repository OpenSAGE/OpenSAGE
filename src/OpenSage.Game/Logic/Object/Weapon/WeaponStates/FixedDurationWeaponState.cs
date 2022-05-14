using System;

namespace OpenSage.Logic.Object
{
    internal abstract class FixedDurationWeaponState : BaseWeaponState
    {
        private LogicFrame _exitTime;

        protected abstract RangeDuration Duration { get; }

        protected FixedDurationWeaponState(WeaponStateContext context)
            : base(context)
        {
        }

        protected override void OnEnterStateImpl()
        {
            // TODO: Randomly pick value between Duration.Min and Duration.Max
            _exitTime = Context.GameContext.GameLogic.CurrentFrame + Duration.Min;
        }

        protected bool IsTimeToExitState() =>
            Context.GameContext.GameLogic.CurrentFrame >= _exitTime;
    }
}
