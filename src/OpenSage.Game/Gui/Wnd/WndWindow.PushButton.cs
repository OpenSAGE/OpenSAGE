namespace OpenSage.Gui.Wnd.Elements
{
    partial class WndWindow
    {
        private static void DefaultPushButtonInput(WndWindow element, GuiWindowMessage message, UIElementCallbackContext context)
        {
            // TODO: Capture input on mouse down.
            // TODO: Only fire click event when mouse was pressed and released inside same button.

            switch (message.MessageType)
            {
                case GuiWindowMessageType.MouseEnter:
                    element.CurrentState = WndWindowState.Highlighted;
                    break;

                case GuiWindowMessageType.MouseExit:
                    element.CurrentState = WndWindowState.Enabled;
                    break;

                case GuiWindowMessageType.MouseDown:
                    element.CurrentState = WndWindowState.HighlightedPushed;
                    break;

                case GuiWindowMessageType.MouseUp:
                    element.CurrentState = WndWindowState.Highlighted;
                    element.Parent.SystemCallback.Invoke(
                        element,
                        new GuiWindowMessage(GuiWindowMessageType.SelectedButton, element),
                        context);
                    break;
            }
        }
    }
}
