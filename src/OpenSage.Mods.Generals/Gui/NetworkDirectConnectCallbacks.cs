using System.Net;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Network;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class NetworkDirectConnectCallbacks
    {
        private const string StaticLocalIPPrefix = "NetworkDirectConnect.wnd:StaticLocalIP";
        private const string EditPlayerNamePrefix = "NetworkDirectConnect.wnd:EditPlayerName";
        private const string ComboboxRemoteIPPrefix = "NetworkDirectConnect.wnd:ComboboxRemoteIP";

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
                            var comboboxRemoteIp = (ComboBox) control.Window.Controls.FindControl(ComboboxRemoteIPPrefix);
                            var text = comboboxRemoteIp.Controls[0].Text;
                            // TODO: Connect to the currently selected game
                            var endPoint = new IPEndPoint(IPAddress.Parse(text), Ports.SkirmishHost);
                            context.Game.SkirmishManager.JoinGame(endPoint);
                            context.WindowManager.SetWindow(@"Menus\LanGameOptionsMenu.wnd");
                            break;
                    }
                    break;
            }
        }

        public static void NetworkDirectConnectInit(Window window, Game game)
        {
            // Initialize player name
            var editPlayerName = (TextBox) window.Controls.FindControl(EditPlayerNamePrefix);
            editPlayerName.Text = game.LobbyManager.Username;

            // Initialize local ip
            var staticLocalIp = (Label) window.Controls.FindControl(StaticLocalIPPrefix);
            staticLocalIp.Text = game.LobbyManager.LocalAddress;
        }
    }
}
