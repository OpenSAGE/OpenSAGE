using System;
using OpenSage.Gui.Wnd.Images;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd.Controls
{
    public class Button : Control
    {
        public event EventHandler Click;

        public Image PushedBackgroundImage { get; set; }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return (BackgroundImage != null)
                ? BackgroundImage.NaturalSize
                : Size.Zero;
        }

        protected override void LayoutOverride()
        {
            PushedBackgroundImage?.SetSize(Size);
        }

        protected override void DrawOverride(DrawingContext2D drawingContext)
        {
            DrawText(drawingContext, TextAlignment.Center);
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

            Window.Game?.Audio.PlaySound(Window.ContentManager.IniDataContext.MiscAudio.GuiClickSound);

            Parent.SystemCallback.Invoke(
                this,
                new WndWindowMessage(WndWindowMessageType.SelectedButton, this),
                context);
        }
    }
}
