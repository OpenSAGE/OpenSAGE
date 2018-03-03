using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Network;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class SkirmishGameOptionsMenuCallbacks
    {
        public static void SkirmishGameOptionsMenuSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "SkirmishGameOptionsMenu.wnd:ButtonSelectMap":
                            context.WindowManager.PushWindow(@"Menus\SkirmishMapSelectMenu.wnd");
                            break;

                        case "SkirmishGameOptionsMenu.wnd:ButtonStart":
                            context.Game.Scene2D.WndWindowManager.PopWindow();
                            context.Game.StartGame(
                                @"maps\Alpine Assault\Alpine Assault.map", // TODO
                                new EchoConnection());
                            break;

                        case "SkirmishGameOptionsMenu.wnd:ButtonBack":
                            context.WindowManager.SetWindow(@"Menus\MainMenu.wnd");
                            // TODO: Go back to Single Player sub-menu
                            break;
                    }
                    break;
            }
        }
    }
}
