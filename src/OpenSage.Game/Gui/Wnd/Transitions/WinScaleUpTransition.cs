using System;

namespace OpenSage.Gui.Wnd.Transitions
{
    // TODO: Implement.
    internal class WinScaleUpTransition : WindowTransitionOperation
    {
        public WinScaleUpTransition(WndWindow element, TimeSpan startTime) : base(element, startTime) { }

        protected override int FrameDuration => 6;

        protected override void OnUpdate(float progress) { }
    }
}
