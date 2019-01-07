using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class MessageBoxCallbacks
    {
        public static void MessageBoxSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "MessageBox.wnd:ButtonOk":
                            context.WindowManager.PopWindow();
                            break;
                    }
                    break;
            }
        }
    }
}
