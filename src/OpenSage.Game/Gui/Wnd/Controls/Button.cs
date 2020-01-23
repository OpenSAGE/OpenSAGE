using System;
using OpenSage.Data.Wnd;
using OpenSage.Gui.Wnd.Images;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd.Controls
{
    public class Button : Control
    {
        public event EventHandler Click;

        public Image PushedBackgroundImage { get; set; }
        public Image HoverOverlayImage { get; set; }
        public Image PushedOverlayImage { get; set; }

        public Button(WndWindowDefinition wndWindow, ImageLoader imageLoader)
        {
            BackgroundImage = imageLoader.CreateFromStretchableWndDrawData(wndWindow.EnabledDrawData, 0, 5, 6);
            HoverBackgroundImage = imageLoader.CreateFromStretchableWndDrawData(wndWindow.HiliteDrawData, 0, 5, 6);
            DisabledBackgroundImage = imageLoader.CreateFromStretchableWndDrawData(wndWindow.DisabledDrawData, 0, 5, 6);
            PushedBackgroundImage = imageLoader.CreateFromStretchableWndDrawData(wndWindow.HiliteDrawData, 1, 3, 4);

            HoverTextColor = wndWindow.TextColor.Hilite.ToColorRgbaF();
            DisabledTextColor = wndWindow.TextColor.Disabled.ToColorRgbaF();
        }

        public Button() { }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return (BackgroundImage != null)
                ? BackgroundImage.NaturalSize
                : Size.Zero;
        }

        protected override void LayoutOverride()
        {
            PushedBackgroundImage?.SetSize(Size);
            HoverOverlayImage?.SetSize(Size);
            PushedOverlayImage?.SetSize(Size);
        }

        protected override void DrawOverride(DrawingContext2D drawingContext)
        {
            DrawText(drawingContext, TextAlignment.Center);

            if (IsMouseDown && PushedOverlayImage != null)
            {
                PushedOverlayImage.Draw(drawingContext, ClientRectangle);
            }
            else if (IsMouseOver && HoverOverlayImage != null)
            {
                HoverOverlayImage.Draw(drawingContext, ClientRectangle);
            }
        }

        protected override void DrawBackgroundImage(DrawingContext2D drawingContext)
        {
            if (IsMouseDown && PushedBackgroundImage != null)
            {
                PushedBackgroundImage.Draw(drawingContext, ClientRectangle);
            }
            else
            {
                base.DrawBackgroundImage(drawingContext);
            }
        }

        protected override void DefaultInputOverride(WndWindowMessage message, ControlCallbackContext context)
        {
            // TODO: Capture input on mouse down.
            // TODO: Only fire click event when mouse was pressed and released inside same button.

            switch (message.MessageType)
            {
                case WndWindowMessageType.MouseUp:
                    RaiseClick(context);
                    break;
            }
        }

        private void RaiseClick(ControlCallbackContext context)
        {
            Click?.Invoke(this, EventArgs.Empty);

            Window.Game?.Audio.PlayAudioEvent(Window.Game.AssetStore.MiscAudio.Current.GuiClickSound.Value);

            SystemCallback.Invoke(
                this,
                new WndWindowMessage(WndWindowMessageType.SelectedButton, this),
                context);
        }
    }
}
