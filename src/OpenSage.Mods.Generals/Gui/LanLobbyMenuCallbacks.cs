using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Network;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class LanLobbyMenuCallbacks
    {
        private static Game _game;

        private const string ListBoxGamesPrefix = "LanLobbyMenu.wnd:ListboxGames";
        private const string ListBoxPlayersPrefix = "LanLobbyMenu.wnd:ListboxPlayers";
        private const string TextEntryPlayerNamePrefix = "LanLobbyMenu.wnd:TextEntryPlayerName";
        private const string ButtonClearPrefix = "LanLobbyMenu.wnd:ButtonClear";
        private const string TextEntryChatPrefix = "LanLobbyMenu.wnd:TextEntryChat";

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void LanLobbyMenuSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {

            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    logger.Info($"Have message {message.MessageType} for control {message.Element.Name}");
                    switch (message.Element.Name)
                    {
                        case "LanLobbyMenu.wnd:ButtonBack":
                            LeaveLobby(context);
                            context.WindowManager.SetWindow(@"Menus\MainMenu.wnd");
                            // TODO: Go back to Multiplayer sub-menu
                            break;
                        case "LanLobbyMenu.wnd:ButtonHost":
                            NetworkUtils.HostGame(context);
                            break;
                        case "LanLobbyMenu.wnd:ButtonJoin":

                            var listBoxGames = (ListBox) control.Window.Controls.FindControl(ListBoxGamesPrefix);

                            if (listBoxGames.SelectedIndex < 0)
                            {
                                return;
                            }

                            var selectedItemIndex = listBoxGames.SelectedIndex;
                            var selectedItem = listBoxGames.Items[selectedItemIndex];

                            var player = (LobbyPlayer)selectedItem.DataItem;

                            NetworkUtils.JoinGame(context, player.EndPoint);
                            break;
                        case "LanLobbyMenu.wnd:ButtonDirectConnect":
                            context.WindowManager.SetWindow(@"Menus\NetworkDirectConnect.wnd");
                            break;
                    }
                    break;
            }
        }

        private static void LeaveLobby(ControlCallbackContext context)
        {
            context.Game.LobbyManager.Stop();
        }

        public static void LanLobbyMenuShutdown(Window window, Game game)
        {
            game.LobbyManager.Stop();
        }

        public static void LanLobbyMenuUpdate(Window window, Game game)
        {
            // Update games
            var listBoxGames = (ListBox) window.Controls.FindControl(ListBoxGamesPrefix);

            listBoxGames.Items = (from player in _game.LobbyManager.Players
                                  where player.IsHosting
                                  select new ListBoxDataItem(player, new[] { player.Username }, listBoxGames.TextColor)).ToArray();

            // Update players
            var listBoxPlayers = (ListBox) window.Controls.FindControl(ListBoxPlayersPrefix);

            listBoxPlayers.Items = (from player in _game.LobbyManager.Players
                                    where !player.IsHosting
                                    select new ListBoxDataItem(player, new[] { player.Username }, listBoxGames.TextColor)).ToArray();
        }

        private static void ClearPlayerName(object sender, EventArgs args)
        {
            var buttonClear = (Button) sender;
            var textEditPlayerName = (TextBox) buttonClear.Parent.Controls.FindControl(TextEntryPlayerNamePrefix);
            textEditPlayerName.Text = string.Empty;
        }

        public static void LanLobbyMenuInit(Window window, Game game)
        {
            _game = game;

            // Initialize player name
            var textEditPlayerName = (TextBox) window.Controls.FindControl(TextEntryPlayerNamePrefix);
            textEditPlayerName.Text = game.LobbyManager.Username;

            textEditPlayerName.OnTextChanged += TextEditPlayerName_OnTextChanged;

            // Setup clear button
            var buttonClear = (Button) window.Controls.FindControl(ButtonClearPrefix);
            buttonClear.Click += ClearPlayerName;

            // Clear chat field
            var textChat = (TextBox) window.Controls.FindControl(TextEntryChatPrefix);
            textChat.Text = string.Empty;

            game.LobbyManager.Start();
        }

        public static void LanLobbyMenuInput(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            logger.Info($"Have message {message.MessageType} for control {control.Name}");
        }

        private static void TextEditPlayerName_OnTextChanged(object sender, string text)
        {
            _game.LobbyManager.Username = string.IsNullOrEmpty(text) ? Environment.MachineName : text;
        }
    }
}
