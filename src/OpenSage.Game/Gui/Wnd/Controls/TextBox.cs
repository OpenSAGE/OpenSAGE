using System;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd.Controls
{
    public class TextBox : Control
    {
        public event EventHandler Click;

        public bool IsReadOnly { get; set; }

        protected override void DrawOverride(DrawingContext2D drawingContext)
        {
            Rectangle textArea = new Rectangle(ClientRectangle.X + 10,
                                               ClientRectangle.Y,
                                               ClientRectangle.Width,
                                               ClientRectangle.Height);

            DrawText(drawingContext, TextAlignment.Leading, textArea);
        }

        protected override void DefaultInputOverride(WndWindowMessage message, ControlCallbackContext context)
        {
            // TODO: Capture input on mouse down.
            // TODO: Only fire click event when mouse was pressed and released inside same button.

            switch (message.MessageType)
            {
                case WndWindowMessageType.MouseUp:
                    Click?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }
    }
}
