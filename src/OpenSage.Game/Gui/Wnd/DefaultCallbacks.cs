using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Gui.Wnd
{
    [WndCallbacks]
    internal static class DefaultCallbacks
    {
        public static void W3DNoDraw(Control control, DrawingContext2D drawingContext) { }

        public static void PassSelectedButtonsToParentSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            if (message.MessageType != WndWindowMessageType.SelectedButton)
            {
                return;
            }

            control.Parent.SystemCallback.Invoke(control.Parent, message, context);
        }

        public static void PassMessagesToParentSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            control.Parent.SystemCallback.Invoke(control.Parent, message, context);
        }
    }
}
