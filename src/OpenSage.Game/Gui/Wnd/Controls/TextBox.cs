using System;
using OpenSage.Data.Wnd;
using OpenSage.Gui.Wnd.Images;
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
                        switch (message.Key)
                        {
                            case Veldrid.Key.A:
                            case Veldrid.Key.B:
                            case Veldrid.Key.C:
                            case Veldrid.Key.D:
                            case Veldrid.Key.E:
                            case Veldrid.Key.F:
                            case Veldrid.Key.G:
                            case Veldrid.Key.H:
                            case Veldrid.Key.I:
                            case Veldrid.Key.J:
                            case Veldrid.Key.K:
                            case Veldrid.Key.L:
                            case Veldrid.Key.M:
                            case Veldrid.Key.N:
                            case Veldrid.Key.O:
                            case Veldrid.Key.P:
                            case Veldrid.Key.Q:
                            case Veldrid.Key.R:
                            case Veldrid.Key.S:
                            case Veldrid.Key.T:
                            case Veldrid.Key.U:
                            case Veldrid.Key.V:
                            case Veldrid.Key.W:
                            case Veldrid.Key.X:
                            case Veldrid.Key.Y:
                            case Veldrid.Key.Z:
                                Text += message.Key;
                                break;

                            case Veldrid.Key.Space:
                                Text += " ";
                                break;

                            case Veldrid.Key.Minus:
                                Text += "-";
                                break;

                            case Veldrid.Key.Number0:
                            case Veldrid.Key.Number1:
                            case Veldrid.Key.Number2:
                            case Veldrid.Key.Number3:
                            case Veldrid.Key.Number4:
                            case Veldrid.Key.Number5:
                            case Veldrid.Key.Number6:
                            case Veldrid.Key.Number7:
                            case Veldrid.Key.Number8:
                            case Veldrid.Key.Number9:
                                Text += message.Key.ToString().Substring(6, 1);
                                break;


                            case Veldrid.Key.BackSpace:
                                if(Text != "")
                                {
                                    Text = Text.Substring(0, Text.Length - 1);
                                }
                                break;
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
