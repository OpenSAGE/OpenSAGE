using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using static OpenSage.Network.LobbyManager;

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
                            context.Game.LobbyManager.Hosting = true;
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
            if(!_game.LobbyManager.Updated)
            {
                return;
            }

            // Update games
            var games = _game.LobbyManager.Players.Where(x => x.Value.IsHosting);

            var listBoxGames = (ListBox) window.Controls.FindControl(ListBoxGamesPrefix);
            var items = new List<ListBoxDataItem>(listBoxGames.Items);
            
            //remove items that are no longer in the list
            items.RemoveAll(x => games.Where(y => y.Key.Equals(((KeyValuePair<IPEndPoint, LobbyPlayer>) x.DataItem).Key)).Count() == 0);

            //update the items that are in the list
            items.ForEach(x => x.ColumnData = new[] { ((KeyValuePair<IPEndPoint, LobbyPlayer>) x.DataItem).Value.Name });

            //add the missing items to the list
            foreach (var lobbyPlayer in games)
            {
                var existing = items.Find(x => ((KeyValuePair<IPEndPoint, LobbyPlayer>) x.DataItem).Key.Equals(lobbyPlayer.Key));
                if (existing == null)
                {
                    existing = new ListBoxDataItem(lobbyPlayer, new[] { lobbyPlayer.Value.Name }, listBoxGames.TextColor);
                    items.Add(existing);
                }

                existing.ColumnData = new[] { lobbyPlayer.Value.Name };
            }

            listBoxGames.Items = items.ToArray();

            // Update players
            var listBoxPlayers = (ListBox) window.Controls.FindControl(ListBoxPlayersPrefix);
            items = new List<ListBoxDataItem>(listBoxPlayers.Items);

            var players = _game.LobbyManager.Players.Where(x => x.Value.IsHosting == false);

            //remove items that are no longer in the list
            items.RemoveAll(x => players.Where(y => y.Key.Equals(((KeyValuePair<IPEndPoint, LobbyPlayer>) x.DataItem).Key)).Count() == 0);

            //add the missing items to the list
            foreach (var lobbyPlayer in players)
            {
                var existing = items.Find(x => ((KeyValuePair<IPEndPoint, LobbyPlayer>) x.DataItem).Key.Equals(lobbyPlayer.Key));
                if(existing == null)
                {
                    existing = new ListBoxDataItem(lobbyPlayer, new[] { lobbyPlayer.Value.Name }, listBoxPlayers.TextColor);
                    items.Add(existing);
                }

                existing.ColumnData = new[] { lobbyPlayer.Value.Name };   
            }

            listBoxPlayers.Items = items.ToArray();



            game.LobbyManager.Updated = false;
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
            game.LobbyManager.Updated = true;
        }

        private static void TextEditPlayerName_OnTextChanged(object sender, string Text)
        {
            _game.LobbyManager.Username = Text;
        }
    }
}
