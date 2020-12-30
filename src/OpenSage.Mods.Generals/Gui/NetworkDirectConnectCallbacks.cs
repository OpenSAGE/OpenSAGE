using System;
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

        public static async void NetworkDirectConnectSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "NetworkDirectConnect.wnd:ButtonBack":
                            context.WindowManager.SetWindow(@"Menus\LanLobbyMenu.wnd");
                            break;
                        case "NetworkDirectConnect.wnd:ButtonHost":
                            if (UPnP.Status == UPnPStatus.Enabled)
                            {
                                await UPnP.ForwardPortsAsync();
                            }

                            NetworkUtils.HostGame(context);
                            break;
                        case "NetworkDirectConnect.wnd:ButtonJoin":
                            var comboboxRemoteIp = (ComboBox) control.Window.Controls.FindControl(ComboboxRemoteIPPrefix);
                            if (System.Net.IPAddress.TryParse(comboboxRemoteIp.Controls[0].Text, out var ipAddress))
                            {
                                var endPoint = new System.Net.IPEndPoint(ipAddress, Ports.SkirmishHost);
                                NetworkUtils.JoinGame(context, endPoint);
                            }
                            break;
                    }
                    break;
            }
        }

        public static async void NetworkDirectConnectInit(Window window, Game game)
        {
            // Initialize player name
            var editPlayerName = (TextBox) window.Controls.FindControl(EditPlayerNamePrefix);
            editPlayerName.Text = game.LobbyManager.Username;

            if (UPnP.Status == UPnPStatus.Uninitialized)
            {
                await UPnP.InitializeAsync(TimeSpan.FromSeconds(5));
            }

            // Initialize local ip
            var staticLocalIp = (Label) window.Controls.FindControl(StaticLocalIPPrefix);
            staticLocalIp.Text = (UPnP.Status == UPnPStatus.Enabled ? IPAddress.External : IPAddress.Local).ToString();
        }
    }
}
