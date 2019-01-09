using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class LanLobbyMenuCallbacks
    {
        public static void LanLobbyMenuSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "LanLobbyMenu.wnd:ButtonBack":
                            context.WindowManager.SetWindow(@"Menus\MainMenu.wnd");
                            // TODO: Go back to Multiplayer sub-menu
                            break;
                        case "LanLobbyMenu.wnd:ButtonHost":
                            context.WindowManager.SetWindow(@"Menus\LanGameOptionsMenu.wnd");
                            break;
                        case "LanLobbyMenu.wnd:ButtonJoin":
                            // TODO: Connect to the currently selected game
                            break;
                        case "LanLobbyMenu.wnd:ButtonDirectConnect":
                            context.WindowManager.SetWindow(@"Menus\NetworkDirectConnect.wnd");
                            break;
                    }
                    break;
            }
        }
    }
}
