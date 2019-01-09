﻿using OpenSage.Data.Ini;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    class LanGameOptionsMenuCallbacks
    {
        private static Window _window;
        private static Game _game;
        private static MapCache _currentMap;

        public static GameOptionsUtil GameOptions { get; private set; }

        public static void LanGameOptionsMenuSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            if (!GameOptions.HandleSystem(control, message, context))
            {
                switch (message.MessageType)
                {
                    case WndWindowMessageType.SelectedButton:
                        switch (message.Element.Name)
                        {
                            case "LanGameOptionsMenu.wnd:ButtonBack":
                                context.WindowManager.SetWindow(@"Menus\LanLobbyMenu.wnd");
                                // TODO: Go back to Single Player sub-menu
                                break;
                        }
                        break;
                }
            }
        }

        public static void LanGameOptionsMenuInit(Window window, Game game)
        {
            GameOptions = new GameOptionsUtil(window, game, "Lan");
        }
    }
}
