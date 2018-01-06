using System;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd.Transitions
{
    internal sealed class WinFadeTransition : WindowTransitionOperation
    {
        private readonly float _startOpacity;
        private readonly float _endOpacity;

        protected override int FrameDuration => 12;

        public WinFadeTransition(WndWindow element, TimeSpan startTime)
            : base(element, startTime)
        {
            _startOpacity = element.Opacity;
            _endOpacity = element.Opacity == 1 ? 0 : 1;
        }

        protected override void OnUpdate(float progress)
        {
            Element.Opacity = MathUtility.Lerp(
                _startOpacity,
                _endOpacity,
                progress);
        }
    }
}
