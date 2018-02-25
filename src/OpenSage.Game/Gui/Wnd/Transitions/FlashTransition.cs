using System;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd.Transitions
{
    internal sealed class FlashTransition : WindowTransitionOperation
    {
        // 3 frames to yellow (border colour?), 0 opacity to half final opacity,
        // 3 frames to final colour, half final opacity to final opacity.

        private readonly float _startOpacity;
        private readonly float _endOpacity;

        protected override int FrameDuration => 6;

        public FlashTransition(Control element, TimeSpan startTime)
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

            Element.BackgroundColorOverride = progress < 0.5f
                ? FlashColor
                : Element.BackgroundColor;
        }

        protected override void OnFinish()
        {
            Element.Opacity = _endOpacity;
            Element.BackgroundColorOverride = null;
        }
    }
}
