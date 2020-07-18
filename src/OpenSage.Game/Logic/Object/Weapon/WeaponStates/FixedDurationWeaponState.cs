using System;

namespace OpenSage.Logic.Object
{
    internal abstract class FixedDurationWeaponState : BaseWeaponState
    {
        private TimeSpan _exitTime;

        protected abstract RangeDuration Duration { get; }

        protected FixedDurationWeaponState(WeaponStateContext context)
            : base(context)
        {
        }

        protected override void OnEnterStateImpl(TimeInterval time)
        {
            // TODO: Randomly pick value between Duration.Min and Duration.Max
            _exitTime = time.TotalTime + Duration.Min;
        }

        protected bool IsTimeToExitState(TimeSpan currentTime) =>
            currentTime >= _exitTime;
    }
}
