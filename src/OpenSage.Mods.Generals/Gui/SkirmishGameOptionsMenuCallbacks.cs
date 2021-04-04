using System;
using System.Linq;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Mathematics;
using OpenSage.Network;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class SkirmishGameOptionsMenuCallbacks
    {
        public static GameOptionsUtil GameOptions { get; private set; }

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static async void SkirmishGameOptionsMenuSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            if (!await GameOptions.HandleSystemAsync(control, message, context))
            {
                switch (message.MessageType)
                {
                    case WndWindowMessageType.SelectedButton:
                        switch (message.Element.Name)
                        {
                            case "SkirmishGameOptionsMenu.wnd:ButtonBack":
                                context.Game.SkirmishManager.Stop();
                                context.WindowManager.SetWindow(@"Menus\MainMenu.wnd");
                                // TODO: Go back to Single Player sub-menu
                                break;
                        }
                        break;
                }
            }
        }

        public static void SkirmishGameOptionsMenuInit(Window window, Game game)
        {
            GameOptions = new GameOptionsUtil(window, game, "Skirmish");
        }

        public static void SkirmishGameOptionsMenuUpdate(Window window, Game game)
        {
            GameOptions.UpdateUI(window);
        }
    }
}
