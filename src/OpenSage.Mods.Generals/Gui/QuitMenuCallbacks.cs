using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class QuitMenuCallbacks
    {
        public static void QuitMenuSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "QuitMenu.wnd:ButtonReturn":
                            context.WindowManager.PopWindow();
                            break;
                    }

                    break;
            }
        }
    }
}
