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
            if (!GameOptions.HandleSystem(control, message, context))
            {
                switch (message.MessageType)
                {
                    case WndWindowMessageType.SelectedButton:
                        switch (message.Element.Name)
                        {
                            case "LanGameOptionsMenu.wnd:ButtonBack":
                                //this should be called by the OnStop callback

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

        public static void LanGameOptionsMenuInit(Window window, Game game)
        {
            if (window.Tag == NetworkUtils.OnlineTag && game.SkirmishManager.IsHosting && IPAddress.NatExternal != null)
            {
                var listBoxChat = (ListBox)window.Controls.FindControl(ListboxChatWindowLanGamePrefix);
                listBoxChat.Items = new[] { new ListBoxDataItem(null, new string[] { $"Your external IP address is {IPAddress.NatExternal}" }, ColorRgbaF.White) };
            }

            GameOptions = new GameOptionsUtil(window, game, "Lan");

            GameOptions.OnSlotIndexChange += (index, name, value) =>
            {
                if (game?.SkirmishManager?.SkirmishGame == null)
                {
                    return;
                }

                var slot = game.SkirmishManager.SkirmishGame.Slots[index];

                switch (name)
                {
                    case GameOptionsUtil.ComboBoxColorPrefix:
                        Logger.Trace($"Changed the color box to {value}");
                        slot.ColorIndex = (byte) value;
                        break;
                    case GameOptionsUtil.ComboBoxPlayerPrefix:
                        Logger.Trace($"Changed the player type box to {value}");
                        
                        break;
                    case GameOptionsUtil.ComboBoxPlayerTemplatePrefix:
                        Logger.Trace($"Changed the faction box to {value}");
                        slot.FactionIndex = (byte) value;
                        break;
                    case GameOptionsUtil.ComboBoxTeamPrefix:
                        Logger.Trace($"Changed the team box to {value}");
                        slot.Team = (byte) value;
                        break;
                }

            };

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
        }

        public static void LanGameOptionsMenuUpdate(Window window, Game game)
        {
            foreach (var slot in game.SkirmishManager.SkirmishGame.Slots)
            {
                var colorCombo = (ComboBox)window.Controls.FindControl($"LanGameOptionsMenu.wnd:ComboBoxColor{slot.Index}");
                if (colorCombo.SelectedIndex != slot.ColorIndex)
                    colorCombo.SelectedIndex = slot.ColorIndex;

                var teamCombo = (ComboBox)window.Controls.FindControl($"LanGameOptionsMenu.wnd:ComboBoxTeam{slot.Index}");
                if (teamCombo.SelectedIndex != slot.Team)
                    teamCombo.SelectedIndex = slot.Team;

                var playerTemplateCombo = (ComboBox)window.Controls.FindControl($"LanGameOptionsMenu.wnd:ComboBoxPlayerTemplate{slot.Index}");
                if (playerTemplateCombo.SelectedIndex != slot.FactionIndex)
                    playerTemplateCombo.SelectedIndex = slot.FactionIndex;

                var buttonAccepted = (Button) window.Controls.FindControl($"LanGameOptionsMenu.wnd:ButtonAccept{slot.Index}");
                var playerCombo = (ComboBox) window.Controls.FindControl($"LanGameOptionsMenu.wnd:ComboBoxPlayer{slot.Index}");

                var isLocalSlot = slot == game.SkirmishManager.SkirmishGame.LocalSlot;
                var editable = isLocalSlot || (game.SkirmishManager.IsHosting && slot.State != SkirmishSlotState.Human);

                playerCombo.Enabled = !isLocalSlot && game.SkirmishManager.IsHosting;

                buttonAccepted.Visible = slot.State == SkirmishSlotState.Human;

                if (slot.State == SkirmishSlotState.Human)
                {
                    if (buttonAccepted.Enabled != slot.Ready)
                        buttonAccepted.Enabled = slot.Ready;

                    playerCombo.Controls[0].Text = slot.PlayerName;
                }
                else
                {
                    playerCombo.Controls[0].Text = slot.State.ToString();
                }

                colorCombo.Enabled = editable;
                teamCombo.Enabled = editable;
                playerTemplateCombo.Enabled = editable;


            };

            var buttonAccept = (Button) window.Controls.FindControl($"LanGameOptionsMenu.wnd:ButtonStart");
            buttonAccept.Enabled = game.SkirmishManager.IsHosting ? game.SkirmishManager.IsReady : !(game.SkirmishManager.SkirmishGame.LocalSlot?.Ready ?? false);
        }

    }
}
