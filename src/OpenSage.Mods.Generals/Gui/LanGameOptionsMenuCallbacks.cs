using System;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Mathematics;
using OpenSage.Network;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    class LanGameOptionsMenuCallbacks
    {
        private const string TextEntryChatPrefix = "LanGameOptionsMenu.wnd:TextEntryChat";
        private const string ListboxChatWindowLanGamePrefix = "LanGameOptionsMenu.wnd:ListboxChatWindowLanGame";

        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static GameOptionsUtil GameOptions { get; private set; }

        public static void LanGameOptionsMenuInput(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            Logger.Trace($"Have message {message.MessageType} for control {message.Element.DisplayName}");
        }

        public static async void LanGameOptionsMenuSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            Logger.Trace($"Have message {message.MessageType} for control {control.Name}");
            if (!await GameOptions.HandleSystemAsync(control, message, context))
            {
                switch (message.MessageType)
                {
                    case WndWindowMessageType.SelectedButton:
                        switch (message.Element.Name)
                        {
                            case "LanGameOptionsMenu.wnd:ButtonBack":
                                context.Game.SkirmishManager.Stop();

                                if (UPnP.Status == UPnPStatus.PortsForwarded)
                                {
                                    await UPnP.RemovePortForwardingAsync();
                                }

                                context.WindowManager.SetWindow(@"Menus\LanLobbyMenu.wnd");
                                break;

                            default:
                                Logger.Warn($"No callback for {message.Element.Name}");
                                break;
                        }
                        break;
                }
            }
        }

        public static async void LanGameOptionsMenuInit(Window window, Game game)
        {
            GameOptions = new GameOptionsUtil(window, game, "Lan");

            if (game.SkirmishManager.IsHosting)
            {
                game.SkirmishManager.Settings.MapName = GameOptions.CurrentMap.Name;
            }

            // Clear chat field
            var textChat = (TextBox)window.Controls.FindControl(TextEntryChatPrefix);
            textChat.Text = string.Empty;

            
            var buttonStart = (Button) window.Controls.FindControl($"LanGameOptionsMenu.wnd:ButtonStart");
            //TODO: Use the right language strings
            buttonStart.Text = game.SkirmishManager.IsHosting ? "Play Game" : "Accept";

            //game.SkirmishManager.OnStop += () =>
            //{
            //    //TODO: somehow make this work
            //    game.Scene2D.WndWindowManager.SetWindow(@"Menus\LanLobbyMenu.wnd");
            //};

            if (window.Tag == NetworkUtils.OnlineTag && game.SkirmishManager.IsHosting)
            {
                var listBoxChat = (ListBox) window.Controls.FindControl(ListboxChatWindowLanGamePrefix);
                var listBoxItem = new ListBoxDataItem(null, new string[] { "Checking UPnP status..." }, ColorRgbaF.White);
                listBoxChat.Items = new[] { listBoxItem }; 

                if (UPnP.Status == UPnPStatus.Enabled)
                {
                    if (await UPnP.ForwardPortsAsync())
                    {
                        listBoxItem.ColumnData[0] = $"Ports forwarded via UPnP. Your external IP is {UPnP.ExternalIP?.ToString() ?? "unknown."}";
                    }
                    else
                    {
                        listBoxItem.ColumnData[0] = $"Failed to forward ports via UPnP. Your external IP is {UPnP.ExternalIP?.ToString() ?? "unknown."}";
                    }
                }
                else
                {
                    listBoxItem.ColumnData[0] = "UPnP is disabled.";
                }
            }
        }

        public static void LanGameOptionsMenuUpdate(Window window, Game game)
        {
            GameOptions.UpdateUI(window);
        }
    }
}
