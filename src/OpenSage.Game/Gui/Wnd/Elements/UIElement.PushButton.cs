namespace OpenSage.Gui.Wnd.Elements
{
    partial class UIElement
    {
        private static void DefaultPushButtonInput(UIElement element, GuiWindowMessage message, UIElementCallbackContext context)
        {
            // TODO: Capture input on mouse down.
            // TODO: Only fire click event when mouse was pressed and released inside same button.

            switch (message.MessageType)
            {
                case GuiWindowMessageType.MouseEnter:
                    element.CurrentState = UIElementState.Highlighted;
                    break;

                case GuiWindowMessageType.MouseExit:
                    element.CurrentState = UIElementState.Enabled;
                    break;

                case GuiWindowMessageType.MouseDown:
                    element.CurrentState = UIElementState.HighlightedPushed;
                    break;

                case GuiWindowMessageType.MouseUp:
                    element.CurrentState = UIElementState.Highlighted;
                    element.Parent.SystemCallback.Invoke(
                        element,
                        new GuiWindowMessage(GuiWindowMessageType.SelectedButton, element),
                        context);
                    break;
            }
        }
    }
}
