using OpenSage.Gui.Wnd.Images;

namespace OpenSage.Gui.Wnd.Controls
{
    public class Button : Control
    {
        public ImageBase PushedBackgroundImage { get; set; }

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
                    Parent.SystemCallback.Invoke(
                        this,
                        new WndWindowMessage(WndWindowMessageType.SelectedButton, this),
                        context);
                    break;
            }
        }
    }
}
