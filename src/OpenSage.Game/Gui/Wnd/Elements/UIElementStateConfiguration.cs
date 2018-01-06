using OpenSage.LowLevel.Graphics3D;
using OpenSage.Content;
using OpenSage.Content.Util;
using OpenSage.Data.Wnd;
using OpenSage.Mathematics;
using OpenSage.LowLevel.Graphics2D;
using System;

namespace OpenSage.Gui.Wnd.Elements
{
    internal sealed class UIElementStateConfiguration : DisposableBase
    {
        public static UIElementStateConfiguration Create(
            WndWindowDefinition wndWindow,
            UIElementState state,
            ContentManager contentManager,
            GraphicsDevice graphicsDevice,
            GraphicsDevice2D graphicsDevice2D)
        {
            WndDrawData wndDrawData;
            ColorRgba textColor, textColorBorder;
            switch (state)
            {
                case UIElementState.Enabled:
                    wndDrawData = wndWindow.EnabledDrawData;
                    textColor = wndWindow.TextColor.Enabled;
                    textColorBorder = wndWindow.TextColor.EnabledBorder;
                    break;

                case UIElementState.Highlighted:
                case UIElementState.HighlightedPushed:
                    wndDrawData = wndWindow.HiliteDrawData;
                    textColor = wndWindow.TextColor.Hilite;
                    textColorBorder = wndWindow.TextColor.HiliteBorder;
                    break;

                case UIElementState.Disabled:
                    wndDrawData = wndWindow.DisabledDrawData;
                    textColor = wndWindow.TextColor.Disabled;
                    textColorBorder = wndWindow.TextColor.DisabledBorder;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state));
            }

            StretchableImage createImage()
            {
                switch (wndWindow.WindowType)
                {
                    case WndWindowType.PushButton:
                        switch (state)
                        {
                            case UIElementState.HighlightedPushed:
                                return StretchableImage.CreatePushButtonImage(
                                    wndWindow,
                                    wndDrawData,
                                    contentManager,
                                    1, 3, 4);

                            default:
                                return StretchableImage.CreatePushButtonImage(
                                    wndWindow,
                                    wndDrawData,
                                    contentManager,
                                    0, 5, 6);
                        }

                    default:
                        return StretchableImage.CreateNormalImage(
                            wndWindow,
                            wndDrawData,
                            contentManager);
                }
            }

            var image = wndWindow.Status.HasFlag(WndWindowStatusFlags.Image)
                ? createImage()
                : null;

            return new UIElementStateConfiguration(
                wndWindow,
                textColor,
                textColorBorder,
                wndDrawData,
                image,
                graphicsDevice,
                graphicsDevice2D);
        }

        public ColorRgba TextColor { get; }
        public ColorRgba TextBorderColor { get; }

        public ColorRgbaF? BackgroundColor { get; }
        public ColorRgbaF? BorderColor { get; }

        public Texture ImageTexture { get; }

        private UIElementStateConfiguration(
            WndWindowDefinition wndWindow,
            ColorRgba textColor,
            ColorRgba textBorderColor,
            WndDrawData wndDrawData,
            StretchableImage image,
            GraphicsDevice graphicsDevice,
            GraphicsDevice2D graphicsDevice2D)
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

            if (image != null)
            {
                ImageTexture = AddDisposable(image.RenderToTexture(graphicsDevice, graphicsDevice2D));
            }
        }
    }
}
