using System;
using OpenSage.Data.Wnd;
using OpenSage.Gui.Wnd.Images;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd.Controls
{
    public class RadioButton : Control
    {
        public event EventHandler Click;

        public Image HoverOverlayImage { get; set; }
        public Image StateActiveImage { get; set; }
        public bool State { get; set; }

        public RadioButton(WndWindowDefinition wndWindow, WndImageLoader imageLoader)
        {
            BackgroundImage = imageLoader.CreateStretchableImage(wndWindow.EnabledDrawData, 0, 1, 2);
            HoverBackgroundImage = imageLoader.CreateStretchableImage(wndWindow.HiliteDrawData, 0, 1, 2);
            DisabledBackgroundImage = imageLoader.CreateStretchableImage(wndWindow.DisabledDrawData, 0, 1, 2);

            HoverTextColor = wndWindow.TextColor.Hilite.ToColorRgbaF();
            DisabledTextColor = wndWindow.TextColor.Disabled.ToColorRgbaF();
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return (BackgroundImage != null)
                ? BackgroundImage.NaturalSize
                : Size.Zero;
        }

        protected override void LayoutOverride()
        {
            HoverOverlayImage?.SetSize(Size);
            StateActiveImage?.SetSize(Size);
        }

        protected override void DrawOverride(DrawingContext2D drawingContext)
        {
            DrawText(drawingContext, TextAlignment.Center);

            if (IsMouseOver && HoverOverlayImage != null)
            {
                HoverOverlayImage.Draw(drawingContext, ClientRectangle);
            }
        }

        protected override void DrawBackgroundImage(DrawingContext2D drawingContext)
        {
            if (IsMouseDown && StateActiveImage != null)
            {
                StateActiveImage.Draw(drawingContext, ClientRectangle);
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

            Window.Game?.Audio.PlayAudioEvent(Window.ContentManager.IniDataContext.MiscAudio.GuiClickSound);

            SystemCallback.Invoke(
                this,
                new WndWindowMessage(WndWindowMessageType.SelectedButton, this),
                context);
        }
    }
}

