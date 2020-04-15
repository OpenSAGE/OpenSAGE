﻿using System;
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
                            context.Game.LobbyManager.Hosting = true;
                            context.Game.LobbyManager.InLobby = true;
                            context.WindowManager.SetWindow(@"Menus\LanGameOptionsMenu.wnd");
                            break;
                        case "LanLobbyMenu.wnd:ButtonJoin":
                            context.Game.LobbyManager.InLobby = true;
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

        private static void LanLobbyGameAdd(object sender, Network.LobbyScanSession.LobbyGameScannedEventArgs args)
        {
            _game.LobbyManager.Players.Remove(args.Host);
            _game.LobbyManager.Updated = true;
        }

        private static void LanLobbyPlayerAdd(object sender, Network.LobbyScanSession.LobbyPlayerScannedEventArgs args)
        {
            _game.LobbyManager.Games.Remove(args.Host);
            _game.LobbyManager.Updated = true;
        }

        public static void LanLobbyMenuUpdate(Window window, Game game)
        {
            if(!_game.LobbyManager.Updated)
            {
                return;
            }

            // Update games
            var listBoxGames = (ListBox) window.Controls.FindControl(ListBoxGamesPrefix);
            var items = new List<ListBoxDataItem>();

            foreach (var lobbyGame in game.LobbyManager.Games)
            {
                items.Add(new ListBoxDataItem(lobbyGame, new[] { lobbyGame.Value.Name }, listBoxGames.TextColor));
            }

            listBoxGames.Items = items.ToArray();

            // Update players
            var listBoxPlayers = (ListBox) window.Controls.FindControl(ListBoxPlayersPrefix);
            items = new List<ListBoxDataItem>();

            items.Add(new ListBoxDataItem(game.LobbyManager.Username, new[] { game.LobbyManager.Username }, listBoxGames.TextColor));
            foreach (var lobbyPlayer in game.LobbyManager.Players)
            {
                items.Add(new ListBoxDataItem(lobbyPlayer, new[] { lobbyPlayer.Value.Name }, listBoxGames.TextColor));
            }

            listBoxPlayers.Items = items.ToArray();
            game.LobbyManager.Updated = false;
        }

        private static void ClearPlayerName(object sender, EventArgs args)
        {
            var buttonClear = (Button) sender;
            var textEditPlayerName = (TextBox) buttonClear.Parent.Controls.FindControl(TextEntryPlayerNamePrefix);
            textEditPlayerName.Text = "";
            _game.LobbyManager.Username = "";
            _game.LobbyManager.Updated = true;
        }

        public static void LanLobbyMenuInit(Window window, Game game)
        {
            _game = game;
            _window = window;

            // Initialize player name
            var textEditPlayerName = (TextBox) _window.Controls.FindControl(TextEntryPlayerNamePrefix);
            textEditPlayerName.Text = game.LobbyManager.Username;

            // Setup clear button
            var buttonClear = (Button) _window.Controls.FindControl(ButtonClearPrefix);
            buttonClear.Click += ClearPlayerName;

            game.LobbyManager.LobbyGameDetected += LanLobbyGameAdd;
            game.LobbyManager.LobbyPlayerDetected += LanLobbyPlayerAdd;

            game.LobbyManager.Start();
            game.LobbyManager.Updated = true;
        }
    }
}
