using System;
using System.Collections.Generic;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class LanLobbyMenuCallbacks
    {
        private static Window _window;
        private static Game _game;

        private const string ListBoxGamesPrefix = "LanLobbyMenu.wnd:ListboxGames";
        private const string ListBoxPlayersPrefix = "LanLobbyMenu.wnd:ListboxPlayers";
        private const string TextEntryPlayerNamePrefix = "LanLobbyMenu.wnd:TextEntryPlayerName";
        private const string ButtonClearPrefix = "LanLobbyMenu.wnd:ButtonClear";

        public static void LanLobbyMenuSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "LanLobbyMenu.wnd:ButtonBack":
                            LeaveLobby(context);
                            context.WindowManager.SetWindow(@"Menus\MainMenu.wnd");
                            // TODO: Go back to Multiplayer sub-menu
                            break;
                        case "LanLobbyMenu.wnd:ButtonHost":
                            context.Game.LobbyBrowser.Hosting = true;
                            context.Game.LobbyBrowser.InLobby = true;
                            context.WindowManager.SetWindow(@"Menus\LanGameOptionsMenu.wnd");
                            break;
                        case "LanLobbyMenu.wnd:ButtonJoin":
                            context.Game.LobbyBrowser.InLobby = true;
                            // TODO: Connect to the currently selected game
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
            context.Game.LobbyScanSession.Stop();
            context.Game.LobbyBroadcastSession.Stop();
        }

        private static void LanLobbyGameAdd(object sender, Network.LobbyScanSession.LobbyScannedEventArgs args)
        {
            //TODO: add new lobby
            _game.LobbyBrowser.Updated = true;
        }

        public static void LanLobbyMenuUpdate(Window window, Game game)
        {
            if(!_game.LobbyBrowser.Updated)
            {
                return;
            }

            // Update games
            var listBoxGames = (ListBox) window.Controls.FindControl(ListBoxGamesPrefix);
            var items = new List<ListBoxDataItem>();

            foreach (var lobbyGame in game.LobbyBrowser.Games)
            {
                items.Add(new ListBoxDataItem(lobbyGame, new[] { lobbyGame.Value.Name }, listBoxGames.TextColor));
            }

            listBoxGames.Items = items.ToArray();

            // Update players
            var listBoxPlayers = (ListBox) window.Controls.FindControl(ListBoxPlayersPrefix);
            items = new List<ListBoxDataItem>();

            items.Add(new ListBoxDataItem(game.LobbyBrowser.Username, new[] { game.LobbyBrowser.Username }, listBoxGames.TextColor));
            foreach (var lobbyPlayer in game.LobbyBrowser.Players)
            {
                items.Add(new ListBoxDataItem(lobbyPlayer, new[] { lobbyPlayer.Value.Name }, listBoxGames.TextColor));
            }

            listBoxPlayers.Items = items.ToArray();
            game.LobbyBrowser.Updated = false;
        }

        private static void ClearPlayerName(object sender, EventArgs args)
        {
            var buttonClear = (Button) sender;
            var textEditPlayerName = (TextBox) buttonClear.Parent.Controls.FindControl(TextEntryPlayerNamePrefix);
            textEditPlayerName.Text = "";
            _game.LobbyBrowser.Username = "";
            _game.LobbyBrowser.Updated = true;
        }

        public static void LanLobbyMenuInit(Window window, Game game)
        {
            _game = game;
            _window = window;

            // Initialize player name
            var textEditPlayerName = (TextBox) _window.Controls.FindControl(TextEntryPlayerNamePrefix);
            textEditPlayerName.Text = game.LobbyBrowser.Username;

            // Setup clear button
            var buttonClear = (Button) _window.Controls.FindControl(ButtonClearPrefix);
            buttonClear.Click += ClearPlayerName;

            game.LobbyScanSession.LobbyDetected += LanLobbyGameAdd;
            game.LobbyScanSession.Start();
            game.LobbyBroadcastSession.Start();
        }
    }
}
