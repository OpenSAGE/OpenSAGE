using System;
using OpenSage.Data.Wnd;
using OpenSage.Gui.Wnd.Images;
using OpenSage.Input;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd.Controls
{
    public class TextBox : Control
    {
        public event EventHandler Click;

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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
                    context.WindowManager.Focus(this);
                    Click?.Invoke(this, EventArgs.Empty);
                    break;

                case WndWindowMessageType.MouseEnter:
                case WndWindowMessageType.MouseMove:
                case WndWindowMessageType.MouseExit:
                case WndWindowMessageType.MouseDown:

                    break;

                case WndWindowMessageType.KeyDown:
                    if (!IsReadOnly)
                    {
                        var character = KeyMap.GetCharacter(message.Key);

                        // Not a printable character
                        if (character == '\0')
                        {
                            switch (message.Key)
                            {
                                case Veldrid.Key.BackSpace:
                                    if (Text.Length > 0)
                                    {
                                        Text = Text.Remove(Text.Length - 1, 1);
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            if(message.Modifiers.HasFlag(Veldrid.ModifierKeys.Shift))
                            {
                                Text += char.ToUpperInvariant(character);
                            }
                            else
                            {
                                Text += char.ToLowerInvariant(character);
                            }
                        }
                    }
                    break;

                default:
                    logger.Info($"Unhandled event: {message.MessageType} {message.Element} {message.MousePosition} {message.Key}");
                    break;

            }
        }
    }
}
