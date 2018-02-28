using System;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Gui.Wnd.Transitions
{
    // TODO: Implement when we have audio support.
    internal class ReverseSoundTransition : WindowTransitionOperation
    {
        public ReverseSoundTransition(Control element, TimeSpan startTime) : base(element, startTime) { }

        protected override int FrameDuration => 0;

        protected override void OnUpdate(float progress) { }
    }
}
