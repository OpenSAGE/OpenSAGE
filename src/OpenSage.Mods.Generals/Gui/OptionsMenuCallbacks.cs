using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class OptionsMenuCallbacks
    {
        public static void OptionsMenuSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "OptionsMenu.wnd:ButtonBack":
                            context.WindowManager.PopWindow();
                            break;
                    }
                    break;
            }
        }
    }
}
