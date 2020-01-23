using System;
using OpenSage.Data.Wnd;
using OpenSage.Gui.Wnd.Images;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd.Controls
{
    public class TextBox : Control
    {
        public event EventHandler Click;

        public bool IsReadOnly { get; set; }

        public TextBox(WndWindowDefinition wndWindow, ImageLoader imageLoader)
        {
            BackgroundImage = imageLoader.CreateFromStretchableWndDrawData(wndWindow.EnabledDrawData, 0, 2, 1);
            HoverBackgroundImage = imageLoader.CreateFromStretchableWndDrawData(wndWindow.HiliteDrawData, 0, 2, 1);
            DisabledBackgroundImage = imageLoader.CreateFromStretchableWndDrawData(wndWindow.DisabledDrawData, 0, 2, 1);

            HoverTextColor = wndWindow.TextColor.Hilite.ToColorRgbaF();
            DisabledTextColor = wndWindow.TextColor.Disabled.ToColorRgbaF();
        }

        public TextBox() { }

        protected override void DrawOverride(DrawingContext2D drawingContext)
        {
            var textArea = new Rectangle(
                ClientRectangle.X + 10,
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
