using System;
using OpenSage.Content;
using OpenSage.Content.Util;
using OpenSage.Data.Wnd;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd
{
    internal class WndWindowStateConfiguration : DisposableBase
    {
        public static WndWindowStateConfiguration Create(
            WndWindowDefinition wndWindow,
            WndWindowState state,
            ContentManager contentManager)
        {
            WndDrawData wndDrawData;
            ColorRgba textColor, textColorBorder;
            switch (state)
            {
                case WndWindowState.Enabled:
                    wndDrawData = wndWindow.EnabledDrawData;
                    textColor = wndWindow.TextColor.Enabled;
                    textColorBorder = wndWindow.TextColor.EnabledBorder;
                    break;

                case WndWindowState.Highlighted:
                    wndDrawData = wndWindow.HiliteDrawData;
                    textColor = wndWindow.TextColor.Hilite;
                    textColorBorder = wndWindow.TextColor.HiliteBorder;
                    break;

                case WndWindowState.Disabled:
                    wndDrawData = wndWindow.DisabledDrawData;
                    textColor = wndWindow.TextColor.Disabled;
                    textColorBorder = wndWindow.TextColor.DisabledBorder;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state));
            }

            return new WndWindowStateConfiguration(
                wndWindow,
                textColor,
                textColorBorder,
                wndDrawData);
        }

        public ColorRgba TextColor { get; }
        public ColorRgba TextBorderColor { get; }

        public ColorRgbaF? BackgroundColor { get; }
        public ColorRgbaF? BorderColor { get; }

        private WndWindowStateConfiguration(
            WndWindowDefinition wndWindow,
            ColorRgba textColor,
            ColorRgba textBorderColor,
            WndDrawData wndDrawData)
        {
            TextColor = textColor;
            TextBorderColor = textBorderColor;

            if (!wndWindow.Status.HasFlag(WndWindowStatusFlags.Image))
            {
                BackgroundColor = ConversionExtensions.ToColorRgbaF(wndDrawData.Items[0].Color);
            }

            if (BackgroundColor != null || wndWindow.Status.HasFlag(WndWindowStatusFlags.Border))
            {
                BorderColor = ConversionExtensions.ToColorRgbaF(wndDrawData.Items[0].BorderColor);
            }
        }
    }
}
