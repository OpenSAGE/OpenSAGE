using System;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Gui.Wnd.Transitions
{
    internal sealed class MainMenuScaleUpTransition : WindowTransitionOperation
    {
        protected override int FrameDuration => 0; // TODO

        public MainMenuScaleUpTransition(Control element, TimeSpan startTime)
            : base(element, startTime)
        {
        }

        protected override void OnUpdate(float progress)
        {
            // TODO
        }
    }
}
