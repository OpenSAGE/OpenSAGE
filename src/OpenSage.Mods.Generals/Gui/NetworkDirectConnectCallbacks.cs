using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class NetworkDirectConnectCallbacks
    {
        public static void NetworkDirectConnectSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "NetworkDirectConnect.wnd:ButtonBack":
                            context.WindowManager.SetWindow(@"Menus\LanLobbyMenu.wnd");
                            // TODO: Go back to Multiplayer sub-menu
                            break;
                        case "NetworkDirectConnect.wnd:ButtonHost":
                            context.WindowManager.SetWindow(@"Menus\LanGameOptionsMenu.wnd");
                            break;
                        case "NetworkDirectConnect.wnd:ButtonJoin":
                            // TODO: Connect to the currently selected game
                            break;
                    }
                    break;
            }
        }
    }
}
