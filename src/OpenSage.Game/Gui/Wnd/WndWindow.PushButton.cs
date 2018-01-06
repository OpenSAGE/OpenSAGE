namespace OpenSage.Gui.Wnd
{
    partial class WndWindow
    {
        private static void DefaultPushButtonInput(WndWindow element, WndWindowMessage message, UIElementCallbackContext context)
        {
            // TODO: Capture input on mouse down.
            // TODO: Only fire click event when mouse was pressed and released inside same button.

            switch (message.MessageType)
            {
                case WndWindowMessageType.MouseEnter:
                    element.CurrentState = WndWindowState.Highlighted;
                    break;

                case WndWindowMessageType.MouseExit:
                    element.CurrentState = WndWindowState.Enabled;
                    break;

                case WndWindowMessageType.MouseDown:
                    element.CurrentState = WndWindowState.HighlightedPushed;
                    break;

                case WndWindowMessageType.MouseUp:
                    element.CurrentState = WndWindowState.Highlighted;
                    element.Parent.SystemCallback.Invoke(
                        element,
                        new WndWindowMessage(WndWindowMessageType.SelectedButton, element),
                        context);
                    break;
            }
        }
    }
}
