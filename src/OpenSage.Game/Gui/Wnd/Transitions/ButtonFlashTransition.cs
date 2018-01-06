using System;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd.Transitions
{
    internal sealed class ButtonFlashTransition : WindowTransitionOperation
    {
        // 3 frames to yellow, 5 frames to black, 2 frames nothing, then 2 frames to show text.

        private readonly float _startOpacity;
        private readonly float _endOpacity;

        protected override int FrameDuration => 12;

        public ButtonFlashTransition(WndWindow element, TimeSpan startTime)
            : base(element, startTime)
        {
            _startOpacity = element.Opacity;
            _endOpacity = element.Opacity == 1 ? 0 : 1;
        }

        protected override void OnUpdate(float progress)
        {
            if (progress < 3.0f / FrameDuration)
            {
                Element.Opacity = MathUtility.Lerp(
                    _startOpacity,
                    _endOpacity,
                    progress / 3 * FrameDuration);

                Element.OverlayColorOverride = FlashColor;
            }
            else if (progress < 8.0f / FrameDuration)
            {
                Element.Opacity = _endOpacity;

                var overlayColor = FlashColor;
                overlayColor.A = (byte) MathUtility.Lerp(
                    _endOpacity * 255,
                    _startOpacity * 255,
                    (progress - 3.0f / FrameDuration) / 8 * FrameDuration);

                Element.OverlayColorOverride = overlayColor;
            }
            else if (progress < 10.0f / FrameDuration)
            {
                // Nothing
                Element.Opacity = _endOpacity;
                Element.OverlayColorOverride = null;
                Element.BackgroundColorOverride = null;
            }
            else
            {
                Element.Opacity = _endOpacity;
                Element.OverlayColorOverride = null;
                Element.TextOpacity = MathUtility.Lerp(
                    _startOpacity,
                    _endOpacity,
                    (progress - 10.0f / FrameDuration) * (FrameDuration / 2.0f));
            }
        }

        protected override void OnFinish()
        {
            Element.Opacity = _endOpacity;
            Element.TextOpacity = _endOpacity;
            Element.OverlayColorOverride = null;
            Element.BackgroundColorOverride = null;
        }
    }
}
